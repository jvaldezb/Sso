using System;

namespace identity_service.Dtos.Role;

public record CreateRoleDto(
    string Name, 
    Guid SystemId, 
    List<MenuRoleRwxRequestDto>? Menus = null
);