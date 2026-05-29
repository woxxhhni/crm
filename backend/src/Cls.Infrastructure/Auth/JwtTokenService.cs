using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Cls.Shared.Exceptions;
namespace Cls.Infrastructure.Auth;
public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public string Generate(User user, out DateTime expiresIn)
    {
        var key = configuration["Jwt:Key"] ?? throw new InvalidActionException("JWT key not configured");
        var issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing");
        var audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is missing");
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        
        var claims = new ClaimsIdentity ( new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.MobilePhone, user.Phone ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("full_name", user.Name)
        });

        var handler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;
        expiresIn = now.AddHours(12);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = claims,
            Audience = audience,
            Issuer = issuer,
            IssuedAt = now,
            NotBefore = now,
            Expires = expiresIn,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256Signature
            )
        };
        return handler.WriteToken(handler.CreateToken(descriptor)); ;
    }
}
