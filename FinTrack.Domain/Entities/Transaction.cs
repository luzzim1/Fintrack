using FinTrack.Domain.Enums;

namespace FinTrack.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public required string Description { get; set; }

    public decimal Amount { get; set; }

    public TransactionType Type { get; set; }

    public DateOnly Date { get; set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
