using System;
using identity_service.Dtos.SystemRegistry;

namespace identity_service.Dtos.RefreshToken;

public class RefreshTokenResponseDto
{
    public string AccessToken { get; set; } = null!;
    public DateTimeOffset AccessTokenExpires { get; set; }

    public string RefreshToken { get; set; } = null!;
    public DateTimeOffset RefreshTokenExpires { get; set; }

    // Opcional: si Fase A, puedes devolver lista de sistemas
    public List<SystemRegistryResponseDto>? Systems { get; set; }
}
