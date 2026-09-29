using System.ComponentModel.DataAnnotations;
using FinTrack.Application.Common;
using FinTrack.Domain.Entities;

namespace FinTrack.Application.Categories;

public class CategoryService(ICategoryRepository repository)
{
    public async Task<List<CategoryResponse>> List(Guid userId, CancellationToken ct) =>
        (await repository.List(userId, ct)).Select(Map).ToList();

    public async Task<CategoryResponse> Get(Guid userId, Guid id, CancellationToken ct) => Map(await Find(userId, id, ct));

    public async Task<CategoryResponse> Create(Guid userId, CategoryRequest request, CancellationToken ct)
    {
        Validator.ValidateObject(request, new ValidationContext(request), true);
        var category = new Category(userId, request.Name, request.Type);
        repository.Add(category);
        await repository.Save(ct);
        return Map(category);
    }

    public async Task<CategoryResponse> Update(Guid userId, Guid id, CategoryRequest request, CancellationToken ct)
    {
        Validator.ValidateObject(request, new ValidationContext(request), true);
        var category = await Find(userId, id, ct);
        if (request.Type != category.Type && await repository.IsUsed(id, ct))
            throw new ConflictException("Não é possível mudar o tipo de uma categoria utilizada.");
        category.Update(request.Name, request.Type);
        await repository.Save(ct);
        return Map(category);
    }

    public async Task Delete(Guid userId, Guid id, CancellationToken ct)
    {
        var category = await Find(userId, id, ct);
        if (await repository.IsUsed(id, ct)) throw new ConflictException("A categoria possui transações vinculadas.");
        repository.Remove(category);
        await repository.Save(ct);
    }

    private async Task<Category> Find(Guid userId, Guid id, CancellationToken ct) =>
        await repository.Find(userId, id, ct) ?? throw new NotFoundException("Categoria não encontrada.");
    private static CategoryResponse Map(Category category) => new(category.Id, category.Name, category.Type);
}
