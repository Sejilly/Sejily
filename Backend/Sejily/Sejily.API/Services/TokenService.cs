using Microsoft.IdentityModel.Tokens;
using Sejily.API.Extensions;
using Sejily.API.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Sejily.API.Services;

// The single place that builds JWTs. Login and account switching both use it,
// so any new claim (e.g. the account type) is added here once.
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateToken(
        User activeUser, int? actorUserId = null, DateTime? expiresAt = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, activeUser.Id.ToString()),
            new Claim(ClaimTypes.Name, activeUser.UserName!),
            // Same claim name the login endpoint uses, so policies work for
            // both a normal login and an account-switch token.
            new Claim("AccountType", activeUser.AccountType.ToString())
        };

        if (actorUserId.HasValue)
        {
            claims.Add(new Claim(
                ClaimsPrincipalExtensions.ActorClaimType,
                actorUserId.Value.ToString()));
        }

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(
            securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt ?? DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
