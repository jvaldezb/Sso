using System;

namespace identity_service.Dtos.User;

public class UserForUpdateDto
{
    public required string FullName { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string DocumentType { get; set; }
    public required string DocumentNumber { get; set; }
    public List<Guid> RoleIds { get; set; } = new();
}
