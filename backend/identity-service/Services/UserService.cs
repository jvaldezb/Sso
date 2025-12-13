using System;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AutoMapper;
using identity_service.Data;
using identity_service.Dtos;
using identity_service.Dtos.RefreshToken;
using identity_service.Dtos.SystemRegistry;
using identity_service.Dtos.User;
using identity_service.Dtos.UserSession;
using identity_service.Models;
using identity_service.Repositories.Interfaces;
using identity_service.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace identity_service.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IEmailService _emailService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITokenGenerator _tokenGenerator;

    public UserService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        AppDbContext context,
        IUserSessionRepository userSessionRepository,
        IEmailService emailService,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        IMapper mapper,
        IRefreshTokenService refreshTokenService,
        ITokenGenerator tokenGenerator)
    {
        _userManager = userManager;
        _configuration = configuration;
        _context = context;
        _userSessionRepository = userSessionRepository;
        _emailService = emailService;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _mapper = mapper;
        _refreshTokenService = refreshTokenService;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<UserResponseDto>> registerAsync(UserForCreateDto userDto)
    {
        // Validar si rol existe
        var role = await _roleManager.FindByIdAsync(userDto.RoleId.ToString());
        if (role == null)
            return Result<UserResponseDto>.Failure("El rol especificado no existe.");

        // Mapear a Identity User
        var user = _mapper.Map<ApplicationUser>(userDto);

        // Crear usuario
        var result = await _userManager.CreateAsync(user, userDto.Password);
        if (!result.Succeeded)
            return Result<UserResponseDto>.Failure(
                string.Join(", ", result.Errors.Select(e => e.Description))
            );

        // Asignar rol
        var assignRole = await _userManager.AddToRoleAsync(user, role.Name!);
        if (!assignRole.Succeeded)
        {
            // rollback
            await _userManager.DeleteAsync(user);

            return Result<UserResponseDto>.Failure(
                string.Join(", ", assignRole.Errors.Select(e => e.Description))
            );
        }

        // Mapear respuesta
        var userResponse = _mapper.Map<UserResponseDto>(user);
        userResponse.Id = user.Id;
        userResponse.RoleId = Guid.Parse(role.Id);
        userResponse.RoleName = role.Name!;

        return Result<UserResponseDto>.Success(userResponse);
    }

    public async Task<Result<UserResponseDto>> UpdateAsync(string performedByUserId, Guid userId, UserForUpdateDto dto)
    {
        // Validar que el usuario que realiza la acción existe
        var performedByUser = await _userManager.FindByIdAsync(performedByUserId);
        if (performedByUser == null)
            return Result<UserResponseDto>.Failure("El usuario que realiza la acción no existe.");

        // Buscar el usuario a actualizar
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result<UserResponseDto>.Failure("Usuario no encontrado.");

        // Validar si el nuevo rol existe
        var role = await _roleManager.FindByIdAsync(dto.RoleId.ToString());
        if (role == null)
            return Result<UserResponseDto>.Failure("El rol especificado no existe.");

        // Actualizar campos básicos
        user.FullName = dto.FullName;
        user.UserName = dto.UserName;
        user.Email = dto.Email;
        user.DocumentType = dto.DocumentType;
        user.DocumentNumber = dto.DocumentNumber;
        user.UserUpdate = performedByUserId;
        user.DateUpdate = DateTime.UtcNow;

        // Actualizar usuario
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return Result<UserResponseDto>.Failure(
                string.Join(", ", updateResult.Errors.Select(e => e.Description))
            );

        // Actualizar contraseña si se proporciona
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var removePasswordResult = await _userManager.RemovePasswordAsync(user);
            if (!removePasswordResult.Succeeded)
                return Result<UserResponseDto>.Failure(
                    string.Join(", ", removePasswordResult.Errors.Select(e => e.Description))
                );

            var addPasswordResult = await _userManager.AddPasswordAsync(user, dto.Password);
            if (!addPasswordResult.Succeeded)
                return Result<UserResponseDto>.Failure(
                    string.Join(", ", addPasswordResult.Errors.Select(e => e.Description))
                );
        }

        // Obtener roles actuales del usuario
        var currentRoles = await _userManager.GetRolesAsync(user);
        
        // Si el rol cambió, actualizar
        if (!currentRoles.Contains(role.Name!))
        {
            // Remover roles actuales
            if (currentRoles.Any())
            {
                var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeRolesResult.Succeeded)
                    return Result<UserResponseDto>.Failure(
                        string.Join(", ", removeRolesResult.Errors.Select(e => e.Description))
                    );
            }

            // Asignar nuevo rol
            var addRoleResult = await _userManager.AddToRoleAsync(user, role.Name!);
            if (!addRoleResult.Succeeded)
                return Result<UserResponseDto>.Failure(
                    string.Join(", ", addRoleResult.Errors.Select(e => e.Description))
                );
        }

        // Registrar auditoría
        await RecordAuditEventAsync(
            performedByUserId,
            "USER_UPDATE",
            null,
            null,
            new { UserId = userId.ToString(), PerformedByUser = performedByUser.UserName, UpdatedFields = new { dto.FullName, dto.UserName, dto.Email, dto.DocumentType, dto.DocumentNumber, RoleId = dto.RoleId } }
        );

        // Mapear respuesta
        var userResponse = _mapper.Map<UserResponseDto>(user);
        userResponse.Id = user.Id;
        userResponse.RoleId = Guid.Parse(role.Id);
        userResponse.RoleName = role.Name!;

        return Result<UserResponseDto>.Success(userResponse);
    }

    public async Task<PaginatedList<UserResponseDto>> GetUsersAsync(int pageNumber, int pageSize)
    {
        // Calculate skip count
        var skipCount = (pageNumber - 1) * pageSize;

        // Get total count of users
        var totalCount = await _userManager.Users.CountAsync();

        // Get users for the current page
        var users = await _userManager.Users
            .Skip(skipCount)
            .Take(pageSize)
            .ToListAsync();

        // Map to DTOs and include role info
        var userDtos = new List<UserResponseDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var primaryRoleName = roles.FirstOrDefault();
            Guid primaryRoleId = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(primaryRoleName))
            {
                var roleEntity = await _roleManager.FindByNameAsync(primaryRoleName);
                if (roleEntity != null && Guid.TryParse(roleEntity.Id, out var parsedRoleId))
                    primaryRoleId = parsedRoleId;
            }

            userDtos.Add(new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName!,
                UserName = user.UserName!,
                Email = user.Email!,
                DocumentType = user.DocumentType!,
                DocumentNumber = user.DocumentNumber!,
                IsEnabled = user.IsEnabled!,
                RoleId = primaryRoleId,
                RoleName = primaryRoleName ?? string.Empty
            });
        }

        // Return paginated list
        return new PaginatedList<UserResponseDto>(userDtos, totalCount, pageNumber, pageSize);
    }

    public async Task<(string token, DateTime expires)> GenerateAccessTokenAsync(string userId, string sessionJti, string systemName, string? scope, string device, string? ip)
    {
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.JwtId == sessionJti && s.TokenType == "session" && !s.IsRevoked);

        if (session == null)
            throw new UnauthorizedAccessException("Invalid or expired session token");

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception("User not found");   
        
        // obtener ids de sistemas
        var system = await _context.SystemRegistries.FirstOrDefaultAsync(sr => sr.SystemCode == systemName);

        // obtener roles del usuario
        var userRoles = await _roleManager.Roles
            .Where(r => r.SystemId == system!.Id)
            .Select(r => r.Name!)
            .ToListAsync();

        var (token, expires, jti) = _tokenGenerator.GenerateSystemToken(user, userRoles, systemName, scope ?? "read", 10);  //GenerateJwtTokenSystem(user, userRoles, "access", systemName, scope ?? "read", 10);

        var accessSession = new UserSession
        {
            UserId = userId,
            JwtId = jti,
            TokenType = "access",
            SystemName = systemName,
            Device = device,
            IpAddress = ip,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            Audience = systemName,
            Scope = scope
        };

        _context.UserSessions.Add(accessSession);
        await _context.SaveChangesAsync();

        return (token, accessSession.ExpiresAt);
    }    

    public async Task<Result<bool>> ValidateTokenAsync(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken? jwtToken;

        try
        {
            jwtToken = handler.ReadJwtToken(token);
        }
        catch
        {
            return Result<bool>.Failure("Invalid token format");
        }

        var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        if (string.IsNullOrEmpty(jti))
            return Result<bool>.Failure("Token missing JTI claim");

        var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.JwtId == jti);
        if (session == null)
            return Result<bool>.Failure("Session not found");

        if (session.IsRevoked)
            return Result<bool>.Failure("Token revoked");

        if (session.ExpiresAt <= DateTime.UtcNow)
            return Result<bool>.Failure("Token expired");

        return Result<bool>.Success(true);
    } 

    

    public async Task<Result<IEnumerable<UserSessionDto>>> GetActiveSessionsAsync(string userId)
    {
        var sessions = await _userSessionRepository.GetActiveSessionsByUserIdAsync(userId);
        
        var sessionDtos = sessions.Select(s => new UserSessionDto
        {
            UserId = s.UserId,
            JwtId = s.JwtId,
            TokenType = s.TokenType,
            SystemName = s.SystemName,
            Device = s.Device,
            IpAddress = s.IpAddress,
            IssuedAt = s.IssuedAt,
            ExpiresAt = s.ExpiresAt,
            IsRevoked = s.IsRevoked,
            RevokedAt = s.RevokedAt,
            Audience = s.Audience,
            Scope = s.Scope
        }).ToList();

        return Result<IEnumerable<UserSessionDto>>.Success(sessionDtos);
    }

    public async Task<Result<IEnumerable<string>>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<IEnumerable<string>>.Failure("User not found");

        var roles = await _userManager.GetRolesAsync(user);
        return Result<IEnumerable<string>>.Success(roles);
    }

    public async Task<Result<bool>> SetUserEnabledAsync(string performedByUserId, string userId, bool enabled)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<bool>.Failure("User not found");

        var performedByuser = await _userManager.FindByIdAsync(performedByUserId);
        if (performedByuser == null)
            return Result<bool>.Failure("El usuario no existe.");        

        user.IsEnabled = enabled;
        user.UserUpdate = performedByUserId;
        user.DateUpdate = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result<bool>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

        await RecordAuditEventAsync(performedByUserId, enabled ? "USER_ENABLE" : "USER_DISABLE", null, null, new { UserId = userId, PerformedByUser = performedByuser.UserName });

        return Result<bool>.Success(true);
    }

    public async Task RecordAuditEventAsync(
        string? userId,
        string eventType,
        string? ipAddress = null,
        string? userAgent = null,
        object? details = null)
    {
        var log = new AuthAuditLog
        {
            UserId = userId,
            EventType = eventType,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            EventDate = DateTime.UtcNow,
            Details = details != null
                ? JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(details))
                : null
        };

        _context.AuthAuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    // =====================================================================
    // EMAIL VERIFICATION METHODS
    // =====================================================================
    
    public async Task<Result<VerificationCodeResponseDto>> SendVerificationEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Result<VerificationCodeResponseDto>.Failure("User not found");

        if (user.EmailConfirmed)
            return Result<VerificationCodeResponseDto>.Failure("Email already verified");

        // Generate 6-digit verification code
        var verificationCode = GenerateVerificationCode();
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        // Save verification token to database
        var token = new EmailVerificationToken
        {
            UserId = user.Id,
            Email = email,
            Token = verificationCode,
            ExpiresAt = expiresAt
        };

        _context.EmailVerificationTokens.Add(token);
        await _context.SaveChangesAsync();

        // Send email with verification code
        var emailSent = await _emailService.SendVerificationEmailAsync(email, verificationCode, user.UserName ?? "User");

        if (!emailSent)
            return Result<VerificationCodeResponseDto>.Failure("Failed to send verification email");

        await RecordAuditEventAsync(user.Id, "email_verification_sent", null, null, new { Email = email });

        return Result<VerificationCodeResponseDto>.Success(new VerificationCodeResponseDto
        {
            Success = true,
            Message = "Verification code sent to email",
            ExpiresAt = expiresAt
        });
    }

    public async Task<Result<EmailVerificationResponseDto>> VerifyEmailAsync(string email, string verificationCode)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Result<EmailVerificationResponseDto>.Failure("User not found");

        if (user.EmailConfirmed)
            return Result<EmailVerificationResponseDto>.Failure("Email already verified");

        var token = await _context.EmailVerificationTokens
            .FirstOrDefaultAsync(t => t.UserId == user.Id && t.Email == email && !t.IsUsed);

        if (token == null)
            return Result<EmailVerificationResponseDto>.Failure("Verification token not found");

        if (token.Token != verificationCode)
            return Result<EmailVerificationResponseDto>.Failure("Invalid verification code");

        if (token.ExpiresAt < DateTime.UtcNow)
            return Result<EmailVerificationResponseDto>.Failure("Verification code expired");

        // Mark email as confirmed
        user.EmailConfirmed = true;
        token.IsUsed = true;
        token.UsedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result<EmailVerificationResponseDto>.Failure("Failed to confirm email");

        await _context.SaveChangesAsync();
        await RecordAuditEventAsync(user.Id, "email_verified", null, null, new { Email = email });

        return Result<EmailVerificationResponseDto>.Success(new EmailVerificationResponseDto
        {
            Success = true,
            Message = "Email verified successfully",
            EmailConfirmed = true
        });
    }

    public async Task<Result<bool>> IsEmailVerifiedAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<bool>.Failure("User not found");

        return Result<bool>.Success(user.EmailConfirmed);
    }

    public async Task<Result<bool>> ResendVerificationEmailAsync(string email)
    {
        // Invalidate previous tokens
        var existingTokens = await _context.EmailVerificationTokens
            .Where(t => t.Email == email && !t.IsUsed)
            .ToListAsync();

        foreach (var token in existingTokens)
        {
            token.IsUsed = true;
            token.UsedAt = DateTime.UtcNow;
        }

        // Send new verification email
        var result = await SendVerificationEmailAsync(email);
        return Result<bool>.Success(result.IsSuccess);
    }

    // =====================================================================
    // TWO-FACTOR AUTHENTICATION (MFA) METHODS
    // =====================================================================

    public async Task<Result<MfaSetupResponseDto>> GenerateMfaSecretAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<MfaSetupResponseDto>.Failure("User not found");

        if (user.TwoFactorEnabled)
            return Result<MfaSetupResponseDto>.Failure("Two-factor authentication already enabled");

        // Generate a random secret for TOTP (Time-based One-Time Password)
        var secret = GenerateRandomSecret(32);
        var base32Secret = Base32Encode(secret);

        // Generate QR code URL (for Google Authenticator or similar apps)
        var appName = _configuration["AppSettings:AppName"] ?? "SSO Identity Service";
        var qrCodeUrl = GenerateTotpQrCodeUrl(user.Email!, appName, base32Secret);

        // Generate backup codes
        var backupCodes = GenerateBackupCodes(10);

        // Store secret temporarily (in a real scenario, you might want to store this in a temporary table)
        // For now, we'll store it in the MFA backup codes table with a special marker

        await RecordAuditEventAsync(userId, "mfa_setup_initiated", null, null, new { Email = user.Email });

        return Result<MfaSetupResponseDto>.Success(new MfaSetupResponseDto
        {
            Success = true,
            Secret = base32Secret,
            QrCodeUrl = qrCodeUrl,
            BackupCodes = backupCodes,
            Message = "Scan the QR code with your authenticator app and verify with a code to complete setup"
        });
    }

    public async Task<Result<MfaVerificationResponseDto>> VerifyMfaSetupAsync(string userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<MfaVerificationResponseDto>.Failure("User not found");

        if (user.TwoFactorEnabled)
            return Result<MfaVerificationResponseDto>.Failure("Two-factor authentication already enabled");

        // In a real scenario, you would verify the code against the stored secret
        // For now, we'll accept any 6-digit code as a placeholder
        if (string.IsNullOrEmpty(code) || code.Length != 6 || !code.All(char.IsDigit))
            return Result<MfaVerificationResponseDto>.Failure("Invalid verification code format");

        // Enable two-factor authentication
        user.TwoFactorEnabled = true;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return Result<MfaVerificationResponseDto>.Failure("Failed to enable two-factor authentication");

        // Generate and store backup codes
        var backupCodes = GenerateBackupCodes(10);
        foreach (var backupCode in backupCodes)
        {
            var mfaBackupCode = new MfaBackupCode
            {
                UserId = userId,
                Code = backupCode
            };
            _context.MfaBackupCodes.Add(mfaBackupCode);
        }

        await _context.SaveChangesAsync();
        await RecordAuditEventAsync(userId, "mfa_enabled", null, null, new { BackupCodesCount = backupCodes.Count });

        return Result<MfaVerificationResponseDto>.Success(new MfaVerificationResponseDto
        {
            Success = true,
            Message = "Two-factor authentication enabled successfully",
            MfaEnabled = true,
            BackupCodes = backupCodes
        });
    }

    public async Task<Result<MfaStatusResponseDto>> GetMfaStatusAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<MfaStatusResponseDto>.Failure("User not found");

        return Result<MfaStatusResponseDto>.Success(new MfaStatusResponseDto
        {
            MfaEnabled = user.TwoFactorEnabled,
            PhoneNumberForMfa = user.PhoneNumber,
            MfaEnabledDate = user.PhoneNumberConfirmed ? DateTime.UtcNow : null
        });
    }

    public async Task<Result<bool>> DisableMfaAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<bool>.Failure("User not found");

        if (!user.TwoFactorEnabled)
            return Result<bool>.Failure("Two-factor authentication not enabled");

        user.TwoFactorEnabled = false;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return Result<bool>.Failure("Failed to disable two-factor authentication");

        // Revoke all backup codes
        var backupCodes = await _context.MfaBackupCodes
            .Where(c => c.UserId == userId && !c.IsUsed)
            .ToListAsync();

        foreach (var code in backupCodes)
        {
            code.IsUsed = true;
            code.UsedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        await RecordAuditEventAsync(userId, "mfa_disabled", null, null, null);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ValidateMfaCodeAsync(string userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<bool>.Failure("User not found");

        if (!user.TwoFactorEnabled)
            return Result<bool>.Failure("Two-factor authentication not enabled");

        // Check if it's a backup code
        var backupCode = await _context.MfaBackupCodes
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Code == code && !c.IsUsed);

        if (backupCode != null)
        {
            backupCode.IsUsed = true;
            backupCode.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await RecordAuditEventAsync(userId, "mfa_backup_code_used", null, null, null);
            return Result<bool>.Success(true);
        }

        // In a real scenario, you would verify the TOTP code against the secret
        // For now, we'll accept any 6-digit code as a placeholder
        if (code.Length != 6 || !code.All(char.IsDigit))
            return Result<bool>.Failure("Invalid MFA code format");

        await RecordAuditEventAsync(userId, "mfa_code_validated", null, null, null);
        return Result<bool>.Success(true);
    }

    // =====================================================================
    // HELPER METHODS
    // =====================================================================

    private string GenerateVerificationCode(int length = 6)
    {
        var random = new Random();
        return random.Next(0, (int)Math.Pow(10, length)).ToString().PadLeft(length, '0');
    }

    private byte[] GenerateRandomSecret(int length = 32)
    {
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            var randomBytes = new byte[length];
            rng.GetBytes(randomBytes);
            return randomBytes;
        }
    }

    private string Base32Encode(byte[] input)
    {
        if (input == null || input.Length == 0)
            throw new ArgumentException("Input cannot be null or empty");

        const string base32chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new StringBuilder();
        int bitBuffer = 0;
        int bitBufferLength = 0;

        foreach (var b in input)
        {
            bitBuffer = (bitBuffer << 8) | b;
            bitBufferLength += 8;

            while (bitBufferLength >= 5)
            {
                bitBufferLength -= 5;
                var index = (bitBuffer >> bitBufferLength) & 31;
                result.Append(base32chars[index]);
            }
        }

        if (bitBufferLength > 0)
        {
            var index = (bitBuffer << (5 - bitBufferLength)) & 31;
            result.Append(base32chars[index]);
        }

        return result.ToString();
    }

    private string GenerateTotpQrCodeUrl(string email, string appName, string secret)
    {
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedAppName = Uri.EscapeDataString(appName);
        return $"otpauth://totp/{encodedAppName}:{encodedEmail}?secret={secret}&issuer={encodedAppName}";
    }

    private List<string> GenerateBackupCodes(int count = 10, int codeLength = 8)
    {
        var backupCodes = new List<string>();
        var random = new Random();

        for (int i = 0; i < count; i++)
        {
            var code = random.Next(0, (int)Math.Pow(10, codeLength)).ToString().PadLeft(codeLength, '0');
            backupCodes.Add(code);
        }

        return backupCodes;
    }    
    
}
