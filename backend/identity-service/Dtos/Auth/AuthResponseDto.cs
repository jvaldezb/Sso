using System;
using identity_service.Dtos.SystemRegistry;

namespace identity_service.Dtos.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = null!;
    public DateTimeOffset AccessTokenExpires { get; set; }

    public string RefreshToken { get; set; } = null!;
    public DateTimeOffset RefreshTokenExpires { get; set; }

    public string UserId { get; set; } = null!;
    public string FullName { get; set; } = null!;

    public List<SystemRegistryResponseDto> Systems { get; set; } = new();
}
