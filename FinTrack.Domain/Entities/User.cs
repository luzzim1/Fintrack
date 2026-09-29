namespace FinTrack.Domain.Entities;

public class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    private User() { }
    public User(string name, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
            throw new ArgumentException("O nome deve ter de 1 a 100 caracteres.");
        Name = name.Trim();
        Email = email;
        PasswordHash = passwordHash;
    }
}
