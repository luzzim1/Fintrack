using FinTrack.Application.Common;
using FinTrack.Application.Transactions;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence;

public class TransactionRepository(FinTrackDbContext db) : ITransactionRepository
{
    public Task<Transaction?> Find(Guid userId, Guid id, CancellationToken ct) =>
        db.Transactions.SingleOrDefaultAsync(transaction => transaction.UserId == userId && transaction.Id == id, ct);

    public Task<TransactionResponse?> Get(Guid userId, Guid id, CancellationToken ct) =>
        Project(db.Transactions.AsNoTracking().Where(transaction => transaction.UserId == userId && transaction.Id == id)).SingleOrDefaultAsync(ct);

    public async Task<Page<TransactionResponse>> List(Guid userId, TransactionFilter filter, CancellationToken ct)
    {
        var query = Filter(userId, filter);
        var total = await query.CountAsync(ct);
        var items = await Project(query.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Id)
            .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize)).ToListAsync(ct);
        return new(items, total, filter.Page, filter.PageSize);
    }

    public async Task<FinancialSummary> Summary(Guid userId, TransactionFilter filter, CancellationToken ct)
    {
        var query = Filter(userId, filter);
        var totals = await query.GroupBy(transaction => 1).Select(group => new
        {
            Income = group.Sum(transaction => transaction.Type == TransactionType.Income ? transaction.Amount : 0),
            Expense = group.Sum(transaction => transaction.Type == TransactionType.Expense ? transaction.Amount : 0),
            Count = group.Count()
        }).SingleOrDefaultAsync(ct);
        var categoryRows = await (
            from transaction in query
            join category in db.Categories on transaction.CategoryId equals category.Id into matches
            from category in matches.DefaultIfEmpty()
            group transaction by new { transaction.CategoryId, CategoryName = category == null ? null : category.Name, transaction.Type }
            into grouped
            select new
            {
                grouped.Key.CategoryId, grouped.Key.CategoryName, grouped.Key.Type,
                Amount = grouped.Sum(transaction => transaction.Amount)
            })
            .OrderByDescending(category => category.Amount).ToListAsync(ct);
        var byCategory = categoryRows.Select(category => new CategoryTotal(category.CategoryId,
            category.CategoryName ?? "Sem categoria", category.Type, category.Amount)).ToList();
        return new(totals?.Income ?? 0, totals?.Expense ?? 0, (totals?.Income ?? 0) - (totals?.Expense ?? 0), totals?.Count ?? 0, byCategory);
    }

    private IQueryable<Transaction> Filter(Guid userId, TransactionFilter filter)
    {
        var query = db.Transactions.AsNoTracking().Where(transaction => transaction.UserId == userId);
        if (filter.From.HasValue) query = query.Where(transaction => transaction.Date >= filter.From.Value);
        if (filter.To.HasValue) query = query.Where(transaction => transaction.Date <= filter.To.Value);
        if (filter.Type.HasValue) query = query.Where(transaction => transaction.Type == filter.Type.Value);
        if (filter.CategoryId.HasValue) query = query.Where(transaction => transaction.CategoryId == filter.CategoryId.Value);
        return query;
    }

    private IQueryable<TransactionResponse> Project(IQueryable<Transaction> query) =>
        from transaction in query
        join category in db.Categories on transaction.CategoryId equals category.Id into matches
        from category in matches.DefaultIfEmpty()
        select new TransactionResponse(transaction.Id, transaction.Description, transaction.Amount, transaction.Type,
            transaction.Date, transaction.CreatedAt, transaction.CategoryId, category == null ? null : category.Name);

    public void Add(Transaction transaction) => db.Transactions.Add(transaction);
    public void Remove(Transaction transaction) => db.Transactions.Remove(transaction);
    public async Task Save(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        { throw new ConflictException("A categoria foi alterada ou removida. Atualize os dados e tente novamente."); }
    }
}
