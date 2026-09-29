using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Id)
            .ValueGeneratedNever();

        builder.Property(transaction => transaction.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(transaction => transaction.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(transaction => transaction.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(transaction => transaction.Date)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(transaction => transaction.CreatedAt)
            .IsRequired()
            .HasColumnType("datetimeoffset");
    }
}
