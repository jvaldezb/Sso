namespace identity_service.Services.Interfaces;

public interface IEmailService
{
    Task<bool> SendVerificationEmailAsync(string email, string verificationCode, string userName);
    Task<bool> SendMfaCodeAsync(string email, string mfaCode, string userName);
    Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string userName);
}
