using System;

namespace identity_service.Dtos.Role;

public class MenuRoleRwxRequestDto
{
    public Guid Id { get; set; }
    public required int RwxValue { get; set; }
}
