using System;
using System.Text.Json;
using identity_service.Data;
using identity_service.Dtos;
using identity_service.Dtos.Auth;
using identity_service.Dtos.RefreshToken;
using identity_service.Dtos.SystemRegistry;
using identity_service.Models;
using identity_service.Repositories.Interfaces;
using identity_service.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace identity_service.Services;

public class AuthService : IAuthService
{
private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly AppDbContext _context;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthService(
        IRefreshTokenService refreshTokenService,
        ITokenGenerator tokenGenerator,
        AppDbContext context,
        IUserSessionRepository userSessionRepository,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _refreshTokenService = refreshTokenService;
        _tokenGenerator = tokenGenerator;
        _context = context;
        _userSessionRepository = userSessionRepository;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<Result<AuthResponseDto>> LoginDocumentAsync(LoginDocumentDto dto, string device, string? ip)
    {
        // 1. Buscar usuario
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u =>
                u.DocumentType == dto.DocumentType &&
                u.DocumentNumber == dto.DocumentNumber);

        if (user == null)
            return Result<AuthResponseDto>.Failure("Invalid credentials");

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            return Result<AuthResponseDto>.Failure("Invalid credentials");

        // 2. Encontrar sistemas asignados por roles
        var userRoles = await _userManager.GetRolesAsync(user);

        var roles = await _roleManager.Roles
            .Where(r => userRoles.Contains(r.Name!))
            .ToListAsync();

        var systemIds = roles
            .OfType<ApplicationRole>()
            .Where(r => r.SystemId.HasValue)
            .Select(r => r.SystemId!.Value)
            .ToList();

        var listSystemsRegistry = await _context.SystemRegistries
            .Where(sr => systemIds.Contains(sr.Id))
            .ToListAsync();

        var systems = listSystemsRegistry.Select(sr => sr.SystemCode).ToList();

        // 3. Generar access token Fase A
        var (accessToken, expires, jti) = _tokenGenerator.GenerateCentralToken(user, systems, "global", 60); /*GenerateJwtTokenCentral(
            user,
            "session",
            systems,
            "global",
            60);*/


        // 4. Registrar sesión
        var session = new UserSession
        {
            UserId = user.Id,
            JwtId = jti,
            TokenType = "session",
            SystemName = "SSO-CENTRAL",
            Device = device.Length > 500 ? device.Substring(0, 500) : device,
            IpAddress = ip,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            Audience = "sso-central",
            Scope = "global"
        };

        await _userSessionRepository.AddAsync(session);

        // 5. Generar el refresh token (nuevo)
        var refreshToken = _refreshTokenService.GenerateRefreshToken(ip, device);
        await _refreshTokenService.SaveRefreshTokenAsync(user, refreshToken);

        // 6. Retornar access + refresh token
        var result = new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName!,
            AccessToken = accessToken,
            AccessTokenExpires = session.ExpiresAt,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpires = refreshToken.ExpiresAt,

            Systems = listSystemsRegistry.Select(sr => new SystemRegistryResponseDto
            {
                Id = sr.Id,
                SystemCode = sr.SystemCode,
                SystemName = sr.SystemName,
                Description = sr.Description,
                BaseUrl = sr.BaseUrl,
                IconUrl = sr.IconUrl,
                Category = sr.Category,
                ContactEmail = sr.ContactEmail
            }).ToList()
        };

        return Result<AuthResponseDto>.Success(result);
    }

    public async Task<Result<AuthResponseDto>> LoginDocumentSystemAsync(LoginDocumentSystemDto dto, string device, string? ip)
    {
        // 1. Validar Sistema
        var system = await _context.SystemRegistries
            .FirstOrDefaultAsync(s => s.SystemCode == dto.SystemCode);

        if (system == null)
            return Result<AuthResponseDto>.Failure("Código de sistema no válido.");

        if (system.ApiKey != dto.ApiKey)
            return Result<AuthResponseDto>.Failure("ApiKey no válida para este sistema.");

        // 2. Validar Usuario
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u =>
                u.DocumentType == dto.DocumentType &&
                u.DocumentNumber == dto.DocumentNumber);

        if (user == null)
            return Result<AuthResponseDto>.Failure("Credenciales inválidas.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            return Result<AuthResponseDto>.Failure("Credenciales inválidas.");

        // 3. Roles para este sistema
        var roles = await _roleManager.Roles
            .Where(r => r.SystemId == system.Id)
            .Where(r => r.Name != null)
            .ToListAsync();

        var userRoles = await _userManager.GetRolesAsync(user);
        var roleCodesForSystem = roles
            .Where(r => userRoles.Contains(r.Name!))
            .Select(r => r.Name!)
            .ToList();

        // 4. Generar AccessToken del Sistema (Fase B)
        var (accessToken, accessExpires, jti) =
            _tokenGenerator.GenerateSystemToken(user, roleCodesForSystem, system.SystemCode, "read", 10);

        // 5. Crear RefreshToken
        var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(
            user, 
            ip, 
            device
        );

        // 6. Registrar sesión del AccessToken de este sistema
        var session = new UserSession
        {
            UserId = user.Id,
            JwtId = jti,
            TokenType = "access",
            SystemName = system.SystemCode,
            Device = device,
            IpAddress = ip,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = accessExpires,
            Audience = system.SystemCode,
            Scope = "read"
        };

        await _userSessionRepository.AddAsync(session);

        // 7. Respuesta Final
        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName!,
            AccessToken = accessToken,
            AccessTokenExpires = accessExpires,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpires = refreshToken.ExpiresAt,
            Systems = new List<SystemRegistryResponseDto> // Habitualmente solo 1
            {
                new SystemRegistryResponseDto
                {
                    Id = system.Id,
                    SystemCode = system.SystemCode,
                    SystemName = system.SystemName,
                    Description = system.Description,
                    BaseUrl = system.BaseUrl,
                    IconUrl = system.IconUrl,
                    Category = system.Category,
                    ContactEmail = system.ContactEmail
                }
            }
        });
    }

    public async Task<Result<AccessTokenDto>> loginEmailAsync(LoginEmailDto loginEmailDto, string device, string? ip)
    {
        var user = await _userManager.FindByEmailAsync(loginEmailDto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginEmailDto.Password))
            return Result<AccessTokenDto>.Failure("Invalid credentials");

        // 3. Obtener roles del usuario
        var userRoles = await _userManager.GetRolesAsync(user);
        var roles = await _roleManager.Roles
            .Where(r => userRoles.Contains(r.Name!))
            .ToListAsync();

        var applicationRoles = roles
            .OfType<ApplicationRole>()
            .ToList();
        
        // 4. Obtener sistemas por roles asignados
        var systemIds = applicationRoles
            .Select(r => r.SystemId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

        List<string> systems;
        var listSystemsRegistry = await _context.SystemRegistries
                .Where(sr => systemIds.Contains(sr.Id))
                .ToListAsync();

        if (!systemIds.Any())
        {
            systems = new List<string>();
        }
        else
        {
            systems = await _context.SystemRegistries
                .Where(sr => systemIds.Contains(sr.Id))
                .Select(sr => sr.SystemCode)
                .ToListAsync();
        }

        var (token, expires, jti) = _tokenGenerator.GenerateCentralToken(user, systems, "global", 60); //GenerateJwtTokenCentral(user, "session", systems, "global", 60);

        var session = new UserSession
        {
            UserId = user.Id,
            JwtId = jti,
            TokenType = "session",
            SystemName = "SSO-CENTRAL",
            Device = device,
            IpAddress = ip,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            Audience = "sso-central",
            Scope = "global"
        };

        await _userSessionRepository.AddAsync(session);

        return Result<AccessTokenDto>.Success(new AccessTokenDto
        {
            UserId = user.Id,
            FullName = user.FullName!,
            Token = token,
            Expires = session.ExpiresAt, 
            Systems = listSystemsRegistry.Select(sr => new SystemRegistryResponseDto
            {
                Id = sr.Id,
                SystemCode = sr.SystemCode,
                SystemName = sr.SystemName,
                Description = sr.Description,
                BaseUrl = sr.BaseUrl,
                IconUrl = sr.IconUrl,
                Category = sr.Category,
                ContactEmail = sr.ContactEmail
            }).ToList()
        }); 
    }

    public async Task<Result<SystemAccessTokenDto>> GenerateSystemAccessTokenAsync(
    string userId,
    string sessionJti,
    string systemName,
    string? scope,
    string device,
    string? ip)
    {
        // 1. Validar la sesión
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s =>
                s.UserId == userId &&
                s.JwtId == sessionJti &&
                s.TokenType == "session" &&
                !s.IsRevoked);

        if (session == null)
            return Result<SystemAccessTokenDto>.Failure("Invalid or expired session token");

        // 2. Buscar usuario
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<SystemAccessTokenDto>.Failure("User not found");

        // 3. Buscar sistema
        var system = await _context.SystemRegistries
            .FirstOrDefaultAsync(sr => sr.SystemCode == systemName);

        if (system == null)
            return Result<SystemAccessTokenDto>.Failure("Unknown system");

        // 4. Roles del usuario en ese sistema
        var roles = await _roleManager.Roles
            .Where(r => r.SystemId == system.Id)
            .Select(r => r.Name!)
            .ToListAsync();

        // 5. Generar access token
        var (token, expires, jti) = _tokenGenerator
            .GenerateSystemToken(user, roles, systemName, scope ?? "read", 10);

        // 6. Registrar sesión
        var sysSession = new UserSession
        {
            UserId = userId,
            JwtId = jti,
            TokenType = "access",
            SystemName = systemName,
            Device = device,
            IpAddress = ip,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = expires,
            Audience = systemName,
            Scope = scope
        };

        await _userSessionRepository.AddAsync(sysSession);

        // 7. Respuesta
        return Result<SystemAccessTokenDto>.Success(new SystemAccessTokenDto
        {
            AccessToken = token,
            Expires = expires,
            System = systemName
        });
    }

    public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto req)
    {
        // 1) Rotate and validate refresh token
        var (success, user, newToken) =
            await _refreshTokenService.RotateRefreshTokenAsync(
                req.RefreshToken, 
                req.IpAddress, 
                req.Device
            );

        if (!success || user == null || newToken == null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        // Determine type: Fase A or Fase B
        if (req.TokenType?.Equals("access", StringComparison.OrdinalIgnoreCase) == true)
        {
            // ========================
            // FASE B TOKEN
            // ========================
            if (string.IsNullOrWhiteSpace(req.System))
                throw new ArgumentException("system is required for access tokens");

            var system = await _context.SystemRegistries
                .FirstOrDefaultAsync(s => s.SystemCode == req.System);

            if (system == null)
                throw new ArgumentException("Unknown system");

            var roles = await _roleManager.Roles
                .Where(r => r.SystemId == system.Id)
                .Select(r => r.Name!)
                .ToListAsync();

            var (accessToken, accessExpires, jti) =
                _tokenGenerator.GenerateSystemToken(
                    user,
                    roles,
                    req.System,
                    req.Scope ?? "read",
                    minutesValid: 10
                );

            var accessSession = new UserSession
            {
                UserId = user.Id,
                JwtId = jti,
                TokenType = "access",
                SystemName = req.System,
                Device = req.Device,
                IpAddress = req.IpAddress,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = accessExpires,
                Audience = req.System,
                Scope = req.Scope
            };

            await _userSessionRepository.AddAsync(accessSession);

            return new RefreshTokenResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpires = accessExpires,
                RefreshToken = newToken.Token,
                RefreshTokenExpires = newToken.ExpiresAt
            };
        }
        else
        {
            // ========================
            // FASE A TOKEN
            // ========================
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var roles = await _roleManager.Roles
                .Where(r => userRoles.Contains(r.Id))
                .ToListAsync();

            var systemIds = roles.OfType<ApplicationRole>()
                .Where(r => r.SystemId.HasValue)
                .Select(r => r.SystemId!.Value)
                .ToList();

            var systemsRegistry = await _context.SystemRegistries
                .Where(sr => systemIds.Contains(sr.Id))
                .ToListAsync();

            var systemsCodes = systemsRegistry.Select(sr => sr.SystemCode).ToList();

            var (accessToken, accessExpires, jti) =
                _tokenGenerator.GenerateCentralToken(
                    user,
                    systemsCodes,
                    "global",
                    60
                );

            var session = new UserSession
            {
                UserId = user.Id,
                JwtId = jti,
                TokenType = "session",
                SystemName = "SSO-CENTRAL",
                Device = req.Device,
                IpAddress = req.IpAddress,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = accessExpires,
                Audience = "sso-central",
                Scope = "global"
            };

            await _userSessionRepository.AddAsync(session);

            return new RefreshTokenResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpires = accessExpires,
                RefreshToken = newToken.Token,
                RefreshTokenExpires = newToken.ExpiresAt,
                Systems = systemsRegistry
                    .Select(sr => new SystemRegistryResponseDto
                    {
                        Id = sr.Id,
                        SystemCode = sr.SystemCode,
                        SystemName = sr.SystemName,
                        Description = sr.Description,
                        BaseUrl = sr.BaseUrl,
                        IconUrl = sr.IconUrl,
                        Category = sr.Category,
                        ContactEmail = sr.ContactEmail
                    }).ToList()
            };
        }
    }  

    public async Task<Result<bool>> LogoutAsync(string userId, string? jti = null, string? ip = null, string? userAgent = null)
    {
        var sessions = _context.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked);

        if (!string.IsNullOrEmpty(jti))
            sessions = sessions.Where(s => s.JwtId == jti);

        var list = await sessions.ToListAsync();
        if (!list.Any())
            return Result<bool>.Failure("No active sessions found");

        foreach (var session in list)
        {
            session.IsRevoked = true;
            session.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Registrar en el log de auditoría
        await RecordAuditEventAsync(userId, "logout", ip, userAgent, new { RevokedCount = list.Count });

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
}

