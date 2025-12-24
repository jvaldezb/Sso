using System;

namespace identity_service.Dtos.Role;

public record UpdateRoleDto(
    string Name, 
    Guid SystemId, 
    List<MenuRoleRwxRequestDto>? Menus = null
);