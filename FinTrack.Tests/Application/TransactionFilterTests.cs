using FinTrack.Application.Transactions;
using FinTrack.Domain.Enums;

namespace FinTrack.Tests.Application;

public class TransactionFilterTests
{
    [Fact]
    public void AcceptsInclusiveSameDayPeriod() =>
        new TransactionFilter { From = new(2026, 1, 1), To = new(2026, 1, 1) }.Validate();
    [Fact]
    public void RejectsInvertedDates() =>
        Assert.Throws<ArgumentException>(() => new TransactionFilter { From = new(2026, 2, 1), To = new(2026, 1, 1) }.Validate());
    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(int.MaxValue, 20)]
    public void RejectsInvalidPagination(int page, int pageSize) =>
        Assert.Throws<ArgumentException>(() => new TransactionFilter { Page = page, PageSize = pageSize }.Validate());
    [Fact]
    public void RejectsInvalidTypeAndCategory()
    {
        Assert.Throws<ArgumentException>(() => new TransactionFilter { Type = (TransactionType)0 }.Validate());
        Assert.Throws<ArgumentException>(() => new TransactionFilter { CategoryId = Guid.Empty }.Validate());
    }
}
