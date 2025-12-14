using System;
using identity_service.Models;

namespace identity_service.Services.Interfaces;

public interface IRefreshTokenService
{
    /// <summary>
    /// Genera un nuevo Refresh Token seguro y aleatorio.
    /// </summary>
    RefreshToken GenerateRefreshToken(string? ipAddress, string? deviceInfo,
        Guid? systemId = null, Guid? sessionId = null);

    /// <summary>
    /// Guarda un Refresh Token asociado a un usuario.
    /// </summary>
    Task SaveRefreshTokenAsync(ApplicationUser user, RefreshToken token);

    /// <summary>
    /// Valida y rota un Refresh Token, generando uno nuevo.
    /// </summary>
    /// <param name="refreshToken">Token enviado por el cliente.</param>
    /// <param name="ipAddress">Dirección IP actual.</param>
    /// <param name="deviceInfo">Información del dispositivo.</param>
    /// <returns>
    /// (Success, User, NewToken):
    /// Success = false si el token no es válido o está revocado.
    /// </returns>
    Task<(bool Success, ApplicationUser? User, RefreshToken? NewToken)> RotateRefreshTokenAsync(
        string refreshToken,
        string? ipAddress,
        string? deviceInfo);

    /// <summary>
    /// Revoca un Refresh Token específico.
    /// </summary>
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Revoca todos los Refresh Tokens de un usuario (logout global).
    /// </summary>
    Task RevokeAllTokensForUserAsync(string userId);

    /// <summary>
    /// Verifica si un Refresh Token es válido sin rotarlo.
    /// </summary>
    Task<bool> IsRefreshTokenValidAsync(string refreshToken);

    Task<RefreshToken> CreateRefreshTokenAsync(ApplicationUser user, string? ipAddress, string? deviceInfo);
}
