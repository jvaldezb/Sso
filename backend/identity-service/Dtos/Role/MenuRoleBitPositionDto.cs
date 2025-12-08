using System;

namespace identity_service.Dtos.Role;

public class MenuRoleBitPositionDto
{
    public Guid Id { get; set; }
    public required string Module { get; set; }    
    public required int BitPosition { get; set; } 
}
