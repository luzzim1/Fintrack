using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;

namespace FinTrack.Tests.Domain;

public class CategoryTests
{
    [Fact]
    public void NormalizesNameAndPreservesOwnerOnUpdate()
    {
        var owner = Guid.NewGuid();
        var category = new Category(owner, "  Saúde ", TransactionType.Expense);
        Assert.Equal("SAÚDE", category.NormalizedName);
        category.Update("Lazer", TransactionType.Expense);
        Assert.Equal(owner, category.UserId);
        Assert.Equal("Lazer", category.Name);
    }
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void RejectsEmptyName(string name) =>
        Assert.Throws<ArgumentException>(() => new Category(Guid.NewGuid(), name, TransactionType.Income));
    [Fact]
    public void RejectsInvalidOwnerTypeAndLength()
    {
        Assert.Throws<ArgumentException>(() => new Category(Guid.Empty, "A", TransactionType.Income));
        Assert.Throws<ArgumentException>(() => new Category(Guid.NewGuid(), "A", (TransactionType)3));
        Assert.Throws<ArgumentException>(() => new Category(Guid.NewGuid(), new string('a', 81), TransactionType.Income));
    }
}
