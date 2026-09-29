using FinTrack.Application.Auth;
using Microsoft.AspNetCore.Identity;

namespace FinTrack.Infrastructure.Auth;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<object> hasher = new();
    private static readonly object Subject = new();
    private static readonly string DummyHash = new PasswordHasher<object>().HashPassword(Subject, Guid.NewGuid().ToString());
    public string Hash(string password) => hasher.HashPassword(Subject, password);
    public bool Verify(string hash, string password)
    {
        var result = hasher.VerifyHashedPassword(Subject, string.IsNullOrEmpty(hash) ? DummyHash : hash, password);
        return !string.IsNullOrEmpty(hash) && result != PasswordVerificationResult.Failed;
    }
}
