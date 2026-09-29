using FinTrack.Domain.Enums;

namespace FinTrack.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Description { get; private set; } = null!;

    public decimal Amount { get; private set; }

    public TransactionType Type { get; private set; }

    public DateOnly Date { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private Transaction() { }

    public Transaction(string description, decimal amount, TransactionType type, DateOnly date)
    {
        Update(description, amount, type, date);
    }

    public void Update(string description, decimal amount, TransactionType type, DateOnly date)
    {
        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length > 500)
            throw new ArgumentException("A descrição deve ter de 1 a 500 caracteres.", nameof(description));
        if (amount <= 0 || amount > 9999999999999999.99m || decimal.Round(amount, 2) != amount)
            throw new ArgumentException("O valor deve ser positivo, com até 16 inteiros e 2 casas decimais.", nameof(amount));
        if (!Enum.IsDefined(type))
            throw new ArgumentException("Tipo de movimentação inválido.", nameof(type));
        if (date == default)
            throw new ArgumentException("Informe uma data válida.", nameof(date));

        Description = description.Trim();
        Amount = amount;
        Type = type;
        Date = date;
    }
}
