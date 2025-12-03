namespace identity_service.Dtos.User;

public class SendVerificationEmailDto
{
    public required string Email { get; set; }
}

public class VerificationCodeResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
