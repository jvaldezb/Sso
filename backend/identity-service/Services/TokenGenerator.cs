using System;
using identity_service.Models;
using identity_service.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace identity_service.Services;

public class TokenGenerator : ITokenGenerator
{
    private readonly IConfiguration _configuration;

    public TokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime Expires, string Jti) GenerateCentralToken(
        ApplicationUser user,
        IEnumerable<string> systems,
        string scope,
        int minutesValid)
    {
        var jti = Guid.NewGuid().ToString();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTSettings:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(minutesValid);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim("token_type", "session"),            
            new Claim("scope", scope)
        };
    
        foreach (var sys in systems)
        {
            claims.Add(new Claim("system", sys));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["JWTSettings:ValidIssuer"],
            audience: _configuration["JWTSettings:ValidAudience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutesValid),
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expires, jti);
    }

    public (string Token, DateTime Expires, string Jti) GenerateSystemToken(
        ApplicationUser user,
        IEnumerable<string> roles,
        string systemName,
        string scope,
        int minutesValid)
    {
        var jti = Guid.NewGuid().ToString();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTSettings:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(minutesValid);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim("token_type", "access"),
            new Claim("system", systemName),
            new Claim("scope", scope)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim("role", role));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["JWTSettings:ValidIssuer"],
            audience: systemName,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutesValid),
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expires, jti);
    }
}