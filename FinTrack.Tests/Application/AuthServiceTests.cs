using FinTrack.Application.Auth;
using FinTrack.Application.Common;
using FinTrack.Domain.Entities;

namespace FinTrack.Tests.Application;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterNormalizesEmailAndRejectsDuplicates()
    {
        var users = new Users();
        var service = new AuthService(users, new Passwords(), new Tokens());
        var response = await service.Register(new("Ana", "Ana@Example.com", "long-password"), default);
        Assert.Equal("ana@example.com", users.Stored!.Email);
        Assert.NotEqual("long-password", users.Stored.PasswordHash);
        Assert.Equal(users.Stored.Id, response.UserId);
        await Assert.ThrowsAsync<ConflictException>(() => service.Register(new("Ana", "ana@example.com", "long-password"), default));
    }

    [Fact]
    public async Task LoginRejectsUnknownAccountAndWrongPassword()
    {
        var users = new Users();
        var service = new AuthService(users, new Passwords(), new Tokens());
        await Assert.ThrowsAsync<AuthenticationException>(() => service.Login(new("ana@example.com", "wrong"), default));
        await service.Register(new("Ana", "ana@example.com", "long-password"), default);
        await Assert.ThrowsAsync<AuthenticationException>(() => service.Login(new("ana@example.com", "wrong"), default));
        Assert.Equal(users.Stored!.Id, (await service.Login(new("ana@example.com", "long-password"), default)).UserId);
    }

    private class Users : IUserRepository
    {
        public User? Stored;
        public Task<User?> FindByEmail(string email, CancellationToken cancellationToken) => Task.FromResult(Stored?.Email == email ? Stored : null);
        public Task Add(User user, CancellationToken cancellationToken) { Stored = user; return Task.CompletedTask; }
    }
    private class Passwords : IPasswordService
    {
        public string Hash(string password) => "hashed:" + password;
        public bool Verify(string hash, string password) => hash == Hash(password);
    }
    private class Tokens : ITokenIssuer
    {
        public AuthResponse Issue(User user) => new("test-token", DateTimeOffset.UtcNow.AddHours(1), user.Id, user.Name);
    }
}
