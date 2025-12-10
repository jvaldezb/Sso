using System;

namespace identity_service.Dtos.RefreshToken;

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = null!;
    /// <summary>
    /// "session" = pedir AccessToken Fase A (token central)
    /// "access" = pedir AccessToken Fase B (token por sistema)
    /// </summary>
    public string TokenType { get; set; } = "session";
    public string? System { get; set; }   // requerido si TokenType == "access"
    public string? Scope { get; set; }    // opcional scope para Fase B
    public string? Device { get; set; }   // info opcional del cliente
    public string? IpAddress { get; set; }
}
