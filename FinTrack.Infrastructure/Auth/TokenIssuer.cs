using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinTrack.Application.Auth;
using FinTrack.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinTrack.Infrastructure.Auth;

public class TokenIssuer(IOptions<JwtSettings> settings) : ITokenIssuer
{
    public AuthResponse Issue(User user)
    {
        var options = settings.Value;
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(options.LifetimeMinutes);
        var token = new JwtSecurityToken(options.Issuer, options.Audience,
            [new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())],
            now.UtcDateTime, expires.UtcDateTime,
            new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)), SecurityAlgorithms.HmacSha256));
        return new(new JwtSecurityTokenHandler().WriteToken(token), expires, user.Id, user.Name);
    }
}
