using System;
using System.Security.Cryptography;
using identity_service.Data;
using identity_service.Models;
using identity_service.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace identity_service.Services;

public class RefreshTokenService: IRefreshTokenService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public RefreshTokenService(
        AppDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ============================================================
    // 1) CREAR TOKEN ALEATORIO
    // ============================================================
    public RefreshToken GenerateRefreshToken(string? ipAddress, string? deviceInfo)
    {
        return new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IpAddress = ipAddress,
            DeviceInfo = deviceInfo,
            IsRevoked = false
        };
    }

    // ============================================================
    // 2) GUARDAR EN BD
    // ============================================================
    public async Task SaveRefreshTokenAsync(ApplicationUser user, RefreshToken token)
    {
        token.UserId = user.Id;
        token.UserCreate = user.UserName;
        token.DateCreate = DateTime.UtcNow;

        await _context.RefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    // ============================================================
    // 3) VALIDAR + ROTAR REFRESH TOKEN
    // ============================================================
    public async Task<(bool Success, ApplicationUser? User, RefreshToken? NewToken)>
        RotateRefreshTokenAsync(string refreshToken, string? ipAddress, string? deviceInfo)
    {
        var storedToken = await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (storedToken == null)
            return (false, null, null);

        if (storedToken.IsRevoked)
            return (false, null, null);

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            return (false, null, null);

        var user = storedToken.User;

        // Crear un nuevo token (rotación segura)
        var newToken = GenerateRefreshToken(ipAddress, deviceInfo);

        // Marcar el token viejo como revocado
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByToken = newToken.Token;
        storedToken.UserUpdate = user.UserName;
        storedToken.DateUpdate = DateTime.UtcNow;

        // Registrar nuevo token
        newToken.UserId = user.Id;

        await _context.RefreshTokens.AddAsync(newToken);
        await _context.SaveChangesAsync();

        return (true, user, newToken);
    }

    // ============================================================
    // 4) REVOCAR UN REFRESH TOKEN ESPECÍFICO
    // ============================================================
    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token == null)
            return false;

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // ============================================================
    // 5) REVOCAR TODOS LOS TOKENS DEL USUARIO (LOGOUT GLOBAL)
    // ============================================================
    public async Task RevokeAllTokensForUserAsync(string userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync();

        foreach (var t in tokens)
        {
            t.IsRevoked = true;
            t.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    // ============================================================
    // 6) VALIDAR TOKEN SIN ROTAR (para diagnósticos)
    // ============================================================
    public async Task<bool> IsRefreshTokenValidAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token == null || token.IsRevoked)
            return false;

        return token.ExpiresAt > DateTime.UtcNow;
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(ApplicationUser user, string? ipAddress, string? deviceInfo)
    {
        var token = GenerateRefreshToken(ipAddress, deviceInfo);
        await SaveRefreshTokenAsync(user, token);
        return token;
    }
}
