using System.ComponentModel.DataAnnotations;
using FinTrack.Application.Common;
using FinTrack.Domain.Entities;

namespace FinTrack.Application.Auth;

public class AuthService(IUserRepository users, IPasswordService passwords, ITokenIssuer tokens)
{
    public async Task<AuthResponse> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        Validator.ValidateObject(request, new ValidationContext(request), true);
        var email = request.Email.Trim().ToLowerInvariant();
        if (await users.FindByEmail(email, cancellationToken) is not null)
            throw new ConflictException("Não foi possível cadastrar este email.");
        var user = new User(request.Name, email, passwords.Hash(request.Password));
        await users.Add(user, cancellationToken);
        return tokens.Issue(user);
    }

    public async Task<AuthResponse> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        Validator.ValidateObject(request, new ValidationContext(request), true);
        var user = await users.FindByEmail(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        // Also perform the password verification work for unknown accounts.
        if (!passwords.Verify(user?.PasswordHash ?? "", request.Password) || user is null)
            throw new AuthenticationException();
        return tokens.Issue(user);
    }
}
