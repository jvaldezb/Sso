using System;
using identity_service.Dtos.SystemRegistry;

namespace identity_service.Dtos.User;

public class MeResponseDto
{
    public required string UserId { get; set; }
    public required string FullName { get; set; }
    public required List<SystemRegistryResponseDto> Systems { get; set; }
}