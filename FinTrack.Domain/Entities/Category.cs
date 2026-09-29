using FinTrack.Domain.Enums;

namespace FinTrack.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string NormalizedName { get; private set; } = null!;
    public TransactionType Type { get; private set; }
    private Category() { }
    public Category(Guid userId, string name, TransactionType type)
    {
        if (userId == Guid.Empty) throw new ArgumentException("Proprietário inválido.");
        UserId = userId;
        Update(name, type);
    }
    public void Update(string name, TransactionType type)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 80)
            throw new ArgumentException("A categoria deve ter de 1 a 80 caracteres.");
        if (!Enum.IsDefined(type)) throw new ArgumentException("Tipo de categoria inválido.");
        Name = name.Trim();
        NormalizedName = Name.ToUpperInvariant();
        Type = type;
    }
}
