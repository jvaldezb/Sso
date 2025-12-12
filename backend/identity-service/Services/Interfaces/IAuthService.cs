using System;
using identity_service.Dtos;
using identity_service.Dtos.Auth;
using identity_service.Dtos.RefreshToken;
using identity_service.Dtos.User;

namespace identity_service.Services.Interfaces;

public interface IAuthService
{
    public Task<Result<AuthResponseSsoDto>> LoginDocumentAsync(LoginDocumentDto dto, string device, string? ip);
    public Task<Result<AuthResponseDto>> LoginDocumentSystemAsync(LoginDocumentSystemDto dto, string device, string? ip);
    public Task<Result<AccessTokenDto>> loginEmailAsync(LoginEmailDto loginEmailDto, string device, string? ip);
    Task<Result<SystemAccessTokenDto>> GenerateSystemAccessTokenAsync(string userId, string sessionJti, string system, string scope, string device, string ip);
    Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<Result<bool>> LogoutAsync(string userId, string? jwtId, string? ip, string? device);
    Task<Result<MeResponseDto>> GetCurrentUserAsync(string userId);
}
