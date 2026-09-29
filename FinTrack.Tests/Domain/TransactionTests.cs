using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;

namespace FinTrack.Tests.Domain;

public class TransactionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1.001)]
    [InlineData(10000000000000000)]
    public void RejectsInvalidAmounts(double amount) =>
        Assert.Throws<ArgumentException>(() => new Transaction("Salário", (decimal)amount, TransactionType.Income, new(2026, 1, 1)));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RejectsMissingDescription(string? description) =>
        Assert.Throws<ArgumentException>(() => new Transaction(description!, 10, TransactionType.Income, new(2026, 1, 1)));

    [Fact]
    public void RejectsLongDescriptionInvalidTypeAndMissingDate()
    {
        Assert.Throws<ArgumentException>(() => new Transaction(new string('a', 501), 10, TransactionType.Income, new(2026, 1, 1)));
        Assert.Throws<ArgumentException>(() => new Transaction("Teste", 10, (TransactionType)0, new(2026, 1, 1)));
        Assert.Throws<ArgumentException>(() => new Transaction("Teste", 10, TransactionType.Income, default));
    }

    [Fact]
    public void UpdatePreservesIdentityAndCreationTimeAndValidatesBeforeMutation()
    {
        var transaction = new Transaction("  Salário  ", 100, TransactionType.Income, new(2026, 1, 1));
        var id = transaction.Id;
        var created = transaction.CreatedAt;
        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal(TimeSpan.Zero, created.Offset);
        Assert.Equal("Salário", transaction.Description);
        transaction.Update("Mercado", 50.25m, TransactionType.Expense, new(2026, 2, 1));
        Assert.Equal(id, transaction.Id);
        Assert.Equal(created, transaction.CreatedAt);
        Assert.Throws<ArgumentException>(() => transaction.Update("Inválida", -1, TransactionType.Income, new(2026, 3, 1)));
        Assert.Equal("Mercado", transaction.Description);
        Assert.Equal(50.25m, transaction.Amount);
    }
}
