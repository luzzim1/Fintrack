using System.ComponentModel.DataAnnotations;
using FinTrack.Domain.Entities;

namespace FinTrack.Application.Auth;

public record RegisterRequest(
    [Required, StringLength(100)] string Name,
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(128, MinimumLength = 10)] string Password);
public record LoginRequest(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(128)] string Password);
public record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, Guid UserId, string Name);
public interface IUserRepository
{
    Task<User?> FindByEmail(string email, CancellationToken cancellationToken);
    Task Add(User user, CancellationToken cancellationToken);
}
public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string hash, string password);
}
public interface ITokenIssuer
{
    AuthResponse Issue(User user);
}
