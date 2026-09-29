using System.ComponentModel.DataAnnotations;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;

namespace FinTrack.Application.Transactions;

public record TransactionRequest(
    [Required, StringLength(500)] string Description,
    decimal Amount,
    [EnumDataType(typeof(TransactionType))] TransactionType Type,
    DateOnly Date,
    Guid? CategoryId);

public record TransactionResponse(Guid Id, string Description, decimal Amount, TransactionType Type,
    DateOnly Date, DateTimeOffset CreatedAt, Guid? CategoryId, string? CategoryName);
public record Page<T>(List<T> Items, int TotalCount, int PageNumber, int PageSize);
public record CategoryTotal(Guid? CategoryId, string Name, TransactionType Type, decimal Amount);
public record FinancialSummary(decimal Income, decimal Expense, decimal Balance, int Count, List<CategoryTotal> Categories);

public class TransactionFilter
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public TransactionType? Type { get; set; }
    public Guid? CategoryId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public void Validate()
    {
        if (From > To) throw new ArgumentException("O início do período deve ser anterior ao fim.");
        if (Type.HasValue && !Enum.IsDefined(Type.Value)) throw new ArgumentException("Tipo inválido.");
        if (CategoryId == Guid.Empty) throw new ArgumentException("Categoria inválida.");
        if (Page < 1 || Page > 1000000 || PageSize is < 1 or > 100) throw new ArgumentException("Paginação inválida. Use de 1 a 100 itens por página.");
    }
}
public interface ITransactionRepository
{
    Task<Transaction?> Find(Guid userId, Guid id, CancellationToken ct);
    Task<TransactionResponse?> Get(Guid userId, Guid id, CancellationToken ct);
    Task<Page<TransactionResponse>> List(Guid userId, TransactionFilter filter, CancellationToken ct);
    Task<FinancialSummary> Summary(Guid userId, TransactionFilter filter, CancellationToken ct);
    void Add(Transaction transaction);
    void Remove(Transaction transaction);
    Task Save(CancellationToken ct);
}
