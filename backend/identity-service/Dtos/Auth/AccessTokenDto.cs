using System;
using identity_service.Dtos.SystemRegistry;

namespace identity_service.Dtos.Auth;

public class AccessTokenDto
{
    public required string UserId { get; set; }
    public required string FullName { get; set; }
    public required string Token { get; set; }
    public required DateTime Expires { get; set; }
    public required List<SystemRegistryResponseDto> Systems { get; set; }
}


