using System;

namespace identity_service.Dtos.User;

public class LoginEmailDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
