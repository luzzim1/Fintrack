using FinTrack.Application.Categories;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[Route("api/categories")]
public class CategoriesController(CategoryService service) : AuthenticatedController
{
    [HttpGet]
    public Task<List<CategoryResponse>> List(CancellationToken ct) => service.List(UserId, ct);
    [HttpGet("{id:guid}")]
    public Task<CategoryResponse> Get(Guid id, CancellationToken ct) => service.Get(UserId, id, ct);
    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(CategoryRequest request, CancellationToken ct)
    {
        var category = await service.Create(UserId, request, ct);
        return CreatedAtAction(nameof(Get), new { id = category.Id }, category);
    }
    [HttpPut("{id:guid}")]
    public Task<CategoryResponse> Update(Guid id, CategoryRequest request, CancellationToken ct) => service.Update(UserId, id, request, ct);
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.Delete(UserId, id, ct);
        return NoContent();
    }
}
