using System;
using System.Net;
using System.Net.Mail;
using identity_service.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace identity_service.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendVerificationEmailAsync(string email, string verificationCode, string userName)
    {
        try
        {
            var subject = "Verify Your Email - SSO Identity Service";
            var body = GenerateEmailVerificationBody(userName, verificationCode);
            
            return await SendEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending verification email to {email}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendMfaCodeAsync(string email, string mfaCode, string userName)
    {
        try
        {
            var subject = "Your Two-Factor Authentication Code";
            var body = GenerateMfaCodeBody(userName, mfaCode);
            
            return await SendEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending MFA code to {email}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string userName)
    {
        try
        {
            var subject = "Reset Your Password - SSO Identity Service";
            var body = GeneratePasswordResetBody(userName, resetToken);
            
            return await SendEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending password reset email to {email}: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderPassword = _configuration["EmailSettings:SenderPassword"];
            var senderName = _configuration["EmailSettings:SenderName"] ?? "SSO Identity Service";

            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail))
            {
                _logger.LogWarning("Email settings not configured properly");
                return false;
            }

            using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending email: {ex.Message}");
            return false;
        }
    }

    private string GenerateEmailVerificationBody(string userName, string verificationCode)
    {
        return $@"
            <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Email Verification</h2>
                    <p>Hello {userName},</p>
                    <p>Please use the following code to verify your email address:</p>
                    <p style='font-size: 24px; font-weight: bold; color: #0066cc;'>{verificationCode}</p>
                    <p>This code will expire in 15 minutes.</p>
                    <p>If you did not request this verification, please ignore this email.</p>
                    <p>Best regards,<br/>SSO Identity Service Team</p>
                </body>
            </html>";
    }

    private string GenerateMfaCodeBody(string userName, string mfaCode)
    {
        return $@"
            <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Two-Factor Authentication Code</h2>
                    <p>Hello {userName},</p>
                    <p>Your two-factor authentication code is:</p>
                    <p style='font-size: 24px; font-weight: bold; color: #0066cc;'>{mfaCode}</p>
                    <p>This code will expire in 5 minutes.</p>
                    <p>If you did not request this code, please secure your account immediately.</p>
                    <p>Best regards,<br/>SSO Identity Service Team</p>
                </body>
            </html>";
    }

    private string GeneratePasswordResetBody(string userName, string resetToken)
    {
        return $@"
            <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Password Reset Request</h2>
                    <p>Hello {userName},</p>
                    <p>We received a request to reset your password. Use the following token to complete the process:</p>
                    <p style='font-size: 18px; font-weight: bold; color: #0066cc;'>{resetToken}</p>
                    <p>This token will expire in 1 hour.</p>
                    <p>If you did not request a password reset, please ignore this email.</p>
                    <p>Best regards,<br/>SSO Identity Service Team</p>
                </body>
            </html>";
    }
}
