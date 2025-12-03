using System;

namespace identity_service.Dtos.User;

public class ValidateTokenRequestDto
{
    public string Token { get; set; } = default!;
}
