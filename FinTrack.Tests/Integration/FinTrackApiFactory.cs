using System.Security.Cryptography;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Tests.Integration;

public sealed class FinTrackApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string databaseName = $"FinTrack_Tests_{Guid.NewGuid():N}";
    private readonly string signingKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));

    private string ConnectionString
    {
        get
        {
            var configured = Environment.GetEnvironmentVariable("FINTRACK_TEST_CONNECTION")
                ?? "Server=localhost;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
            return new SqlConnectionStringBuilder(configured) { InitialCatalog = databaseName }.ConnectionString;
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:FinTrack"] = ConnectionString,
                ["Jwt:Key"] = signingKey,
                ["Jwt:Issuer"] = "FinTrack.Tests",
                ["Jwt:Audience"] = "FinTrack.Tests",
                ["Jwt:LifetimeMinutes"] = "15",
                ["RateLimits:AuthPermitLimit"] = "100"
            }));
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FinTrackDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FinTrackDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        Dispose();
    }
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ApiCollection : ICollectionFixture<FinTrackApiFactory>
{
    public const string Name = "FinTrack API";
}
