using System;

namespace identity_service.Dtos.Auth;

public class LoginLdapDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}
