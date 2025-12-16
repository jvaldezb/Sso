using System;
using identity_service.Dtos.SystemRegistry;

namespace identity_service.Dtos.Auth;

public class MeResponseDto
{
    public required string UserId { get; set; }
    public required string FullName { get; set; }
    public Guid SsoSystemId { get; set; }

    public List<MenuDto> Menus { get; set; } = new();
    public List<AuthRoleDto> Roles { get; set; } = new();
    public required List<SystemRegistryResponseDto> Systems { get; set; }
}