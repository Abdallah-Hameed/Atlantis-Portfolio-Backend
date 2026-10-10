using Atlantis_Portfolio_Backend_DAL.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Atlantis_Portfolio_Backend_BLL;

public class clsTokenService
{
    private readonly IConfiguration _configuration;

    public clsTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public virtual string GenerateAccessToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        string keyValue = _configuration["AtlantisPortfolio:Jwt:Key"]
            ?? throw new InvalidOperationException(
                "Atlantis Portfolio JWT signing key is not configured.");

        string issuer = _configuration["AtlantisPortfolio:Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "Atlantis Portfolio JWT issuer is not configured.");

        string audience = _configuration["AtlantisPortfolio:Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "Atlantis Portfolio JWT audience is not configured.");

        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ArgumentException(
                "User email cannot be empty.", nameof(user));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Email)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(keyValue));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public virtual string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }

    public virtual byte[] HashRefreshToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        byte[] tokenBytes = Encoding.UTF8.GetBytes(token);

        return SHA256.HashData(tokenBytes);
    }
}