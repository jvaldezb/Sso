using System;

namespace identity_service.Dtos.Auth;

public class AuthResponseSsoDto : AuthResponseDto
{
    public Guid SsoSystemId { get; set; }

    public List<MenuDto> Menus { get; set; } = new();

    public List<AuthRoleDto> Roles { get; set; } = new();
}
