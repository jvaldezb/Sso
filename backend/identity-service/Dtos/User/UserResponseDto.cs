using System;

namespace identity_service.Dtos.User;

public class UserResponseDto
{
    public string Id { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string DocumentType { get; set; } = null!;
    public string DocumentNumber { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public List<UserRoleDto> Roles { get; set; } = new();
    public string RoleName { get; set; } = null!;
}
