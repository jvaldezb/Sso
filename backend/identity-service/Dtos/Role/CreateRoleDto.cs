using System;

namespace identity_service.Dtos.Role;

public record CreateRoleDto(string Name, int? SystemId);