namespace identity_service.Dtos.User;

public class EnableMfaDto
{
    public required string UserId { get; set; }
}

public class MfaSetupResponseDto
{
    public bool Success { get; set; }
    public string? Secret { get; set; }
    public string? QrCodeUrl { get; set; }
    public string? Message { get; set; }
    public List<string>? BackupCodes { get; set; }
}

public class VerifyMfaSetupDto
{
    public required string UserId { get; set; }
    public required string Code { get; set; }
}

public class MfaVerificationResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public bool MfaEnabled { get; set; }
    public List<string>? BackupCodes { get; set; }
}

public class VerifyMfaCodeDto
{
    public required string Code { get; set; }
}

public class MfaCodeVerificationResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
}

public class DisableMfaDto
{
    public required string UserId { get; set; }
}

public class MfaStatusResponseDto
{
    public bool MfaEnabled { get; set; }
    public string? PhoneNumberForMfa { get; set; }
    public DateTime? MfaEnabledDate { get; set; }
}
