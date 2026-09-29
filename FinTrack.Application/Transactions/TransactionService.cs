using System.ComponentModel.DataAnnotations;
using FinTrack.Application.Categories;
using FinTrack.Application.Common;
using FinTrack.Domain.Entities;

namespace FinTrack.Application.Transactions;

public class TransactionService(ITransactionRepository repository, ICategoryRepository categories)
{
    public Task<Page<TransactionResponse>> List(Guid userId, TransactionFilter filter, CancellationToken ct)
    { filter.Validate(); return repository.List(userId, filter, ct); }

    public Task<FinancialSummary> Summary(Guid userId, TransactionFilter filter, CancellationToken ct)
    { filter.Validate(); return repository.Summary(userId, filter, ct); }

    public async Task<TransactionResponse> Get(Guid userId, Guid id, CancellationToken ct) =>
        await repository.Get(userId, id, ct) ?? throw new NotFoundException("Transação não encontrada.");

    public async Task<TransactionResponse> Create(Guid userId, TransactionRequest request, CancellationToken ct)
    {
        await ValidateCategory(userId, request, ct);
        var transaction = new Transaction(request.Description, request.Amount, request.Type, request.Date);
        transaction.AssignOwner(userId);
        transaction.AssignCategory(request.CategoryId);
        repository.Add(transaction);
        await repository.Save(ct);
        return await Get(userId, transaction.Id, ct);
    }

    public async Task<TransactionResponse> Update(Guid userId, Guid id, TransactionRequest request, CancellationToken ct)
    {
        var transaction = await Find(userId, id, ct);
        await ValidateCategory(userId, request, ct);
        transaction.Update(request.Description, request.Amount, request.Type, request.Date);
        transaction.AssignCategory(request.CategoryId);
        await repository.Save(ct);
        return await Get(userId, id, ct);
    }

    public async Task Delete(Guid userId, Guid id, CancellationToken ct)
    {
        repository.Remove(await Find(userId, id, ct));
        await repository.Save(ct);
    }

    private async Task<Transaction> Find(Guid userId, Guid id, CancellationToken ct) =>
        await repository.Find(userId, id, ct) ?? throw new NotFoundException("Transação não encontrada.");

    private async Task ValidateCategory(Guid userId, TransactionRequest request, CancellationToken ct)
    {
        Validator.ValidateObject(request, new ValidationContext(request), true);
        if (request.CategoryId is not Guid categoryId) return;
        var category = await categories.Find(userId, categoryId, ct) ?? throw new NotFoundException("Categoria não encontrada.");
        if (category.Type != request.Type) throw new ArgumentException("A categoria deve ser do mesmo tipo da transação.");
    }
}
