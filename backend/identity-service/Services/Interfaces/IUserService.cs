using System;
using identity_service.Dtos;
using identity_service.Dtos.RefreshToken;
using identity_service.Dtos.User;
using identity_service.Dtos.UserSession;

namespace identity_service.Services.Interfaces;

public interface IUserService
{
    public Task<Result<UserResponseDto>> registerAsync(UserForCreateDto loginDto); 
    Task<Result<UserResponseDto>> UpdateAsync(string performedByUserId, Guid userId, UserForUpdateDto dto);
    public Task<PaginatedList<UserResponseDto>> GetUsersAsync(int pageNumber, int pageSize);
    
    public Task<(string token, DateTime expires)> GenerateAccessTokenAsync(string userId, string sessionJti, string systemName, string? scope, string device, string? ip);

    public Task<Result<bool>> ValidateTokenAsync(string token);
    
    public Task<Result<IEnumerable<UserSessionDto>>> GetActiveSessionsAsync(string userId);
    public Task<Result<IEnumerable<string>>> GetUserRolesAsync(string userId);
    
    // Multiple Roles Management
    public Task<Result<UserRolesResponseDto>> AssignRolesToUserAsync(string performedByUserId, string userId, AssignRolesDto dto);
    public Task<Result<UserRolesResponseDto>> GetUserRolesDetailedAsync(string userId);

    // Email Verification Methods
    public Task<Result<VerificationCodeResponseDto>> SendVerificationEmailAsync(string email);
    public Task<Result<EmailVerificationResponseDto>> VerifyEmailAsync(string email, string verificationCode);
    public Task<Result<bool>> IsEmailVerifiedAsync(string userId);
    public Task<Result<bool>> ResendVerificationEmailAsync(string email);

    // Two-Factor Authentication (MFA) Methods
    public Task<Result<MfaSetupResponseDto>> GenerateMfaSecretAsync(string userId);
    public Task<Result<MfaVerificationResponseDto>> VerifyMfaSetupAsync(string userId, string code);
    public Task<Result<MfaStatusResponseDto>> GetMfaStatusAsync(string userId);
    public Task<Result<bool>> DisableMfaAsync(string userId);
    public Task<Result<bool>> ValidateMfaCodeAsync(string userId, string code);

    public Task<Result<bool>> ChangePasswordAsync(string userId, ChangePasswordDto dto);

    // Enable/Disable user
    public Task<Result<bool>> SetUserEnabledAsync(string performedByUserId, string userId, bool enabled);
}

