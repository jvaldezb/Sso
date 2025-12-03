namespace identity_service.Dtos.User;

public class VerifyEmailDto
{
    public required string Email { get; set; }
    public required string VerificationCode { get; set; }
}

public class EmailVerificationResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public bool EmailConfirmed { get; set; }
}
