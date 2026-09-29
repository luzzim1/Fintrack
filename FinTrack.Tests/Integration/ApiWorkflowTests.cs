using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FinTrack.Tests.Integration;

[Collection(ApiCollection.Name)]
public class ApiWorkflowTests(FinTrackApiFactory factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Financial_flow_persists_and_summarizes_user_data()
    {
        using var client = factory.CreateClient();
        await Authenticate(client, "Fluxo completo");

        var categoryResponse = await client.PostAsJsonAsync("/api/categories", new { name = "Salário", type = 1 });
        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        var category = await Read(categoryResponse);

        var transactionResponse = await client.PostAsJsonAsync("/api/transactions", new
        {
            description = "Pagamento mensal", amount = 5250.75m, type = 1,
            date = "2026-09-15", categoryId = category.GetProperty("id").GetGuid()
        });
        Assert.Equal(HttpStatusCode.Created, transactionResponse.StatusCode);

        var list = await client.GetFromJsonAsync<JsonElement>("/api/transactions?from=2026-09-01&to=2026-09-30&type=1&page=1&pageSize=10");
        Assert.Equal(1, list.GetProperty("totalCount").GetInt32());
        Assert.Equal("Pagamento mensal", list.GetProperty("items")[0].GetProperty("description").GetString());

        var summary = await client.GetFromJsonAsync<JsonElement>("/api/dashboard?from=2026-09-01&to=2026-09-30");
        Assert.Equal(5250.75m, summary.GetProperty("income").GetDecimal());
        Assert.Equal(0m, summary.GetProperty("expense").GetDecimal());
        Assert.Equal(5250.75m, summary.GetProperty("balance").GetDecimal());
        Assert.Equal(1, summary.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task Protected_resources_reject_anonymous_access_and_isolate_users()
    {
        using var owner = factory.CreateClient();
        using var otherUser = factory.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await owner.GetAsync("/api/categories")).StatusCode);

        await Authenticate(owner, "Proprietário");
        var created = await owner.PostAsJsonAsync("/api/categories", new { name = "Privada", type = 2 });
        var category = await Read(created);

        await Authenticate(otherUser, "Outro usuário");
        var access = await otherUser.GetAsync($"/api/categories/{category.GetProperty("id").GetGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, access.StatusCode);
    }

    [Fact]
    public async Task Api_returns_problem_details_for_invalid_input()
    {
        using var client = factory.CreateClient();
        var invalidRegistration = await client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "", email = "not-an-email", password = "short"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidRegistration.StatusCode);
        Assert.Equal("application/problem+json", invalidRegistration.Content.Headers.ContentType?.MediaType);

        await Authenticate(client, "Validação");
        var invalidFilter = await client.GetAsync("/api/transactions?from=2026-10-01&to=2026-09-01");
        Assert.Equal(HttpStatusCode.BadRequest, invalidFilter.StatusCode);
        var problem = await Read(invalidFilter);
        Assert.Contains("início do período", problem.GetProperty("title").GetString());
    }

    private static async Task Authenticate(HttpClient client, string name)
    {
        var unique = Guid.NewGuid().ToString("N");
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            name, email = $"{unique}@fintrack.test", password = $"{Guid.NewGuid():N}aA1!"
        });
        response.EnsureSuccessStatusCode();
        var payload = await Read(response);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", payload.GetProperty("accessToken").GetString());
    }

    private static async Task<JsonElement> Read(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        return payload;
    }
}
