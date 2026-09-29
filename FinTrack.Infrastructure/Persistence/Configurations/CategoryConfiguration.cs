using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Id).ValueGeneratedNever();
        builder.Property(category => category.Name).HasMaxLength(80).IsRequired();
        builder.Property(category => category.NormalizedName).HasMaxLength(80).IsRequired();
        builder.Property(category => category.Type).HasConversion<int>();
        builder.HasIndex(category => new { category.UserId, category.Type, category.NormalizedName }).IsUnique();
        builder.HasOne<User>().WithMany().HasForeignKey(category => category.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
