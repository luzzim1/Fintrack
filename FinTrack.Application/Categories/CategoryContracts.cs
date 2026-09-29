using System.ComponentModel.DataAnnotations;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;

namespace FinTrack.Application.Categories;

public record CategoryRequest([Required, StringLength(80)] string Name, [EnumDataType(typeof(TransactionType))] TransactionType Type);
public record CategoryResponse(Guid Id, string Name, TransactionType Type);
public interface ICategoryRepository
{
    Task<List<Category>> List(Guid userId, CancellationToken ct);
    Task<Category?> Find(Guid userId, Guid id, CancellationToken ct);
    Task<bool> IsUsed(Guid id, CancellationToken ct);
    void Add(Category category);
    void Remove(Category category);
    Task Save(CancellationToken ct);
}
