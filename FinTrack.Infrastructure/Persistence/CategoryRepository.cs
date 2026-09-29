using FinTrack.Application.Categories;
using FinTrack.Application.Common;
using FinTrack.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence;

public class CategoryRepository(FinTrackDbContext db) : ICategoryRepository
{
    public Task<List<Category>> List(Guid userId, CancellationToken ct) =>
        db.Categories.AsNoTracking().Where(category => category.UserId == userId).OrderBy(category => category.Name).ToListAsync(ct);
    public Task<Category?> Find(Guid userId, Guid id, CancellationToken ct) =>
        db.Categories.SingleOrDefaultAsync(category => category.Id == id && category.UserId == userId, ct);
    public Task<bool> IsUsed(Guid id, CancellationToken ct) => db.Transactions.AnyAsync(transaction => transaction.CategoryId == id, ct);
    public void Add(Category category) => db.Categories.Add(category);
    public void Remove(Category category) => db.Categories.Remove(category);
    public async Task Save(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        { throw new ConflictException("Já existe uma categoria com este nome e tipo."); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        { throw new ConflictException("A categoria possui vínculos que impedem esta operação."); }
    }
}
