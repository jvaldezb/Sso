using System;

namespace identity_service.Dtos.RoleMenu;

public class RoleMenuAccessUpdateDto
{
    public Guid MenuId { get; set; }
    public int AccessLevel { get; set; }
}
