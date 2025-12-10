using System;

namespace identity_service.Dtos.Auth;

public class LoginEmailDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
