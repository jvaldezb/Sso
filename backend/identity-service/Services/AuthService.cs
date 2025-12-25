using System;
using System.Text.Json;
using AutoMapper;
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
    private readonly IMapper _mapper;
    private readonly ILdapAuthenticationService _ldapAuthService;
    private readonly IConfiguration _configuration;

    public AuthService(
        IRefreshTokenService refreshTokenService,
        ITokenGenerator tokenGenerator,
        AppDbContext context,
        IUserSessionRepository userSessionRepository,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        ILdapAuthenticationService ldapAuthService,
        IConfiguration configuration)
    {
        _refreshTokenService = refreshTokenService;
        _tokenGenerator = tokenGenerator;
        _context = context;
        _userSessionRepository = userSessionRepository;
        _roleManager = roleManager;
        _userManager = userManager;
        _mapper = mapper;
        _ldapAuthService = ldapAuthService;
        _configuration = configuration;
    }

    public async Task<Result<AuthResponseSsoDto>> LoginDocumentAsync(LoginDocumentDto dto, string device, string? ip)
    {
        // Validar que el sistema SSO Central esté configurado
        var ssoSystem = await _context.SystemRegistries.FirstOrDefaultAsync(sr => sr.IsCentralAdmin == true);
        if (ssoSystem == null)
            return Result<AuthResponseSsoDto>.Failure("SSO Central system not configured.");        

        // 1. Buscar usuario
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u =>
                u.DocumentType == dto.DocumentType &&
                u.DocumentNumber == dto.DocumentNumber);

        if (user == null)
            return Result<AuthResponseSsoDto>.Failure("Invalid credentials");

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            return Result<AuthResponseSsoDto>.Failure("Invalid credentials");

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

        var jti = Guid.NewGuid().ToString();

        // 3. Registrar sesión
        var session = new UserSession
        {
            UserId = user.Id,
            JwtId = jti,
            TokenType = "session",
            SystemName = "SSO-CENTRAL",
            Device = device.Length > 500 ? device.Substring(0, 500) : device,
            IpAddress = ip,
            IssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(60),
            Audience = "sso-central",
            Scope = "global"
        };

        await _userSessionRepository.AddAsync(session);

        // 4. Generar access token Fase A
        var (accessToken, expires) = _tokenGenerator.GenerateCentralToken(user, session, systems, "global", 60);         

        var menuDtos = new List<MenuDto>();
        if (listSystemsRegistry.Any(x => x.IsCentralAdmin == true))
        {            
            var menus = await _context.Menus
                .Where(m => m.SystemId == ssoSystem.Id)
                .OrderBy(m => m.OrderIndex)            
                .ToListAsync();                

            menuDtos = _mapper.Map<List<MenuDto>>(menus);
        }   

        var accessTokenexpiresAt = DateTimeOffset.UtcNow.AddMinutes(60);                 

        // 5. Generar el refresh token (nuevo)
        var refreshToken = _refreshTokenService.GenerateRefreshToken(ip, device);
        await _refreshTokenService.SaveRefreshTokenAsync(user, refreshToken);

        // 6. Roles para todos los sistemas
        var ssoRoles = roles            
            .Select(r => new AuthRoleDto
            {
                RoleId = r.Id,
                RoleName = r.Name!
            })
            .ToList();

        // 7. Retornar access + refresh token
        var result = new AuthResponseSsoDto
        {
            UserId = user.Id,
            FullName = user.FullName!,
            AccessToken = accessToken,
            AccessTokenExpires = accessTokenexpiresAt,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpires = refreshToken.ExpiresAt,            
            SsoSystemId = ssoSystem.Id,            
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
            }
            ).ToList(),
            Menus = menuDtos,
            Roles = ssoRoles
        };

        return Result<AuthResponseSsoDto>.Success(result);
    }

    public async Task<Result<AuthResponseSsoDto>> LoginLdapAsync(LoginLdapDto dto, string device, string? ip)
    {
        // Verificar si LDAP está habilitado
        var ldapEnabled = bool.Parse(_configuration["LdapSettings:Enabled"] ?? "false");
        if (!ldapEnabled)
        {
            return Result<AuthResponseSsoDto>.Failure("LDAP authentication is not enabled");
        }

        // Validar que el sistema SSO Central esté configurado
        var ssoSystem = await _context.SystemRegistries.FirstOrDefaultAsync(sr => sr.IsCentralAdmin == true);
        if (ssoSystem == null)
            return Result<AuthResponseSsoDto>.Failure("SSO Central system not configured.");

        // 1. Autenticar contra LDAP
        var ldapResult = await _ldapAuthService.AuthenticateAsync(dto.Username, dto.Password);
        if (!ldapResult.IsSuccess || ldapResult.Data == null)
        {
            return Result<AuthResponseSsoDto>.Failure(ldapResult.ErrorMessage ?? "LDAP authentication failed");
        }

        var ldapUserInfo = ldapResult.Data;

        // 2. Buscar o crear usuario en la base de datos local
        var user = await _userManager.FindByNameAsync(dto.Username);
        
        if (user == null)
        {
            // Crear nuevo usuario si no existe
            user = new ApplicationUser
            {
                UserName = dto.Username,
                Email = ldapUserInfo.Email,
                EmailConfirmed = true,
                FullName = ldapUserInfo.DisplayName,
                IsEnabled = true,
                DocumentType = "LDAP",
                DocumentNumber = dto.Username,
                DateCreate = DateTime.UtcNow,
                UserCreate = "LDAP_SYSTEM"
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return Result<AuthResponseSsoDto>.Failure($"Failed to create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            // Actualizar información del usuario existente
            user.Email = ldapUserInfo.Email;
            user.FullName = ldapUserInfo.DisplayName;
            user.DateUpdate = DateTime.UtcNow;
            user.UserUpdate = "LDAP_SYSTEM";
            await _userManager.UpdateAsync(user);
        }

        // 3. Encontrar sistemas asignados por roles
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

        var jti = Guid.NewGuid().ToString();

        // 4. Registrar sesión
        var session = new UserSession
        {
            UserId = user.Id,
            JwtId = jti,
            TokenType = "session",
            SystemName = "SSO-CENTRAL",
            Device = device.Length > 500 ? device.Substring(0, 500) : device,
            IpAddress = ip,
            IssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(60),
            Audience = "sso-central",
            Scope = "global"
        };

        await _userSessionRepository.AddAsync(session);

        // 5. Generar access token Fase A
        var (accessToken, expires) = _tokenGenerator.GenerateCentralToken(user, session, systems, "global", 60);

        var menuDtos = new List<MenuDto>();
        if (listSystemsRegistry.Any(x => x.IsCentralAdmin == true))
        {
            var menus = await _context.Menus
                .Where(m => m.SystemId == ssoSystem.Id)
                .OrderBy(m => m.OrderIndex)
                .ToListAsync();

            menuDtos = _mapper.Map<List<MenuDto>>(menus);
        }

        var accessTokenexpiresAt = DateTimeOffset.UtcNow.AddMinutes(60);

        // 6. Generar el refresh token
        var refreshToken = _refreshTokenService.GenerateRefreshToken(ip, device);
        await _refreshTokenService.SaveRefreshTokenAsync(user, refreshToken);

        // 7. Roles para todos los sistemas
        var ssoRoles = roles
            .Select(r => new AuthRoleDto
            {
                RoleId = r.Id,
                RoleName = r.Name!
            })
            .ToList();

        // 8. Retornar access + refresh token
        var result = new AuthResponseSsoDto
        {
            UserId = user.Id,
            FullName = user.FullName!,
            AccessToken = accessToken,
            AccessTokenExpires = accessTokenexpiresAt,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpires = refreshToken.ExpiresAt,
            SsoSystemId = ssoSystem.Id,
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
            }
            ).ToList(),
            Menus = menuDtos,
            Roles = ssoRoles
        };

        return Result<AuthResponseSsoDto>.Success(result);
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

        var jti = Guid.NewGuid().ToString();
        var expireAt = DateTimeOffset.UtcNow.AddMinutes(10);    

        // 6. Registrar sesión del AccessToken de este sistema
        var session = new UserSession
        {
            UserId = user.Id,
            JwtId = jti,
            TokenType = "access",
            SystemName = system.SystemCode,
            Device = device,
            IpAddress = ip,
            IssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = expireAt,
            Audience = system.SystemCode,
            Scope = "read"
        };

        await _userSessionRepository.AddAsync(session);    

        // 4. Generar AccessToken del Sistema (Fase B)
        var (accessToken, accessExpires) =
            _tokenGenerator.GenerateSystemToken(user, session, roleCodesForSystem, system.SystemCode, "read", 10);

        // 5. Crear RefreshToken
        var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(
            user, 
            ip, 
            device
        );        

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

        var jti = Guid.NewGuid().ToString();

        var session = new UserSession
        {
            UserId = user.Id,
            JwtId = jti,
            TokenType = "session",
            SystemName = "SSO-CENTRAL",
            Device = device,
            IpAddress = ip,
            IssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(60),
            Audience = "sso-central",
            Scope = "global"
        };

        await _userSessionRepository.AddAsync(session);

        var (token, expires) = _tokenGenerator.GenerateCentralToken(user, session, systems, "global", 60);

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

    public async Task<Result<AuthResponseDto>> GenerateSystemAccessTokenAsync(
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
            return Result<AuthResponseDto>.Failure("Invalid or expired session token");

        // 2. Buscar usuario
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<AuthResponseDto>.Failure("User not found");

        // 3. Buscar sistema
        var system = await _context.SystemRegistries
            .FirstOrDefaultAsync(sr => sr.SystemCode == systemName);

        if (system == null)
            return Result<AuthResponseDto>.Failure("Unknown system");

        // 4. Roles del usuario en ese sistema
        var roles = await _roleManager.Roles
            .Where(r => r.SystemId == system.Id)
            .Select(r => r.Name!)
            .ToListAsync();

        var jti = Guid.NewGuid().ToString();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(10);

        // 7. Registrar sesión
        var sysSession = new UserSession
        {
            UserId = userId,
            JwtId = jti,
            TokenType = "access",
            SystemName = systemName,
            Device = device,
            IpAddress = ip,
            IssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = expiresAt,
            Audience = systemName,
            Scope = scope
        };

        await _userSessionRepository.AddAsync(sysSession);    

        // 5. Generar access token
        var (token, expires) = _tokenGenerator
            .GenerateSystemToken(user, sysSession, roles, systemName, scope ?? "read", 10);

        // 6. Crear RefreshToken
        var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(
            user, 
            ip, 
            device
        );

        // 8. Respuesta
        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName!,
            AccessToken = token,
            AccessTokenExpires = expires,
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

            var jti = Guid.NewGuid().ToString();
            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(10);

            var accessSession = new UserSession
            {
                UserId = user.Id,
                JwtId = jti,
                TokenType = "access",
                SystemName = req.System,
                Device = req.Device ?? "Unknown",
                IpAddress = req.IpAddress,
                IssuedAt = DateTimeOffset.UtcNow,
                ExpiresAt = expiresAt,
                Audience = req.System,
                Scope = req.Scope
            };

            await _userSessionRepository.AddAsync(accessSession);    

            var (accessToken, accessExpires) =
                _tokenGenerator.GenerateSystemToken(
                    user,
                    accessSession,
                    roles,
                    req.System,
                    req.Scope ?? "read",
                    minutesValid: 10
                );

            

            return new RefreshTokenResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpires = accessExpires,
                RefreshToken = newToken.Token,
                RefreshTokenExpires = newToken.ExpiresAt,
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

            var jti = Guid.NewGuid().ToString();            

            var session = new UserSession
            {
                UserId = user.Id,
                JwtId = jti,
                TokenType = "session",
                SystemName = "SSO-CENTRAL",
                Device = req.Device ?? "Unknown",
                IpAddress = req.IpAddress,
                IssuedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(60),
                Audience = "sso-central",
                Scope = "global"
            };

            await _userSessionRepository.AddAsync(session);


            var (accessToken, accessExpires) =
                _tokenGenerator.GenerateCentralToken(
                    user,
                    session,
                    systemsCodes,
                    "global",
                    60
                );            

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
            session.RevokedAt = DateTimeOffset.UtcNow;
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
            EventDate = DateTimeOffset.UtcNow,
            Details = details != null
                ? JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(details))
                : null
        };

        _context.AuthAuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<Result<MeResponseDto>> GetCurrentUserAsync(string userId)
    {
        // Validar que el sistema SSO Central esté configurado
        var ssoSystem = await _context.SystemRegistries.FirstOrDefaultAsync(sr => sr.IsCentralAdmin == true);
        if (ssoSystem == null)
            return Result<MeResponseDto>.Failure("SSO Central system not configured.");  

        var user = await _userManager.FindByIdAsync(userId);        
        if (user == null)
            return Result<MeResponseDto>.Failure("Usuario no encontrado");

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

        var listSystemsRegistry = new List<SystemRegistry>();

        if (systemIds.Any())
        {
            listSystemsRegistry = await _context.SystemRegistries
            .Where(sr => systemIds.Contains(sr.Id))
            .ToListAsync();  
        }

        var menuDtos = new List<MenuDto>();
        if (listSystemsRegistry.Any(x => x.IsCentralAdmin == true))
        {            
            var menus = await _context.Menus
                .Where(m => m.SystemId == ssoSystem.Id)
                .OrderBy(m => m.OrderIndex)            
                .ToListAsync();                

            menuDtos = _mapper.Map<List<MenuDto>>(menus);
        }   

        // Roles para todos los sistemas
        var ssoRoles = roles
            .Select(r => new AuthRoleDto
            {
                RoleId = r.Id,
                RoleName = r.Name!
            })
            .ToList();

        // IMPORTANTE:
        // El endpoint /me NO genera nuevo token.
        // Solo devuelve la misma info del login.
        return Result<MeResponseDto>.Success(new MeResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName!,     
            SsoSystemId = ssoSystem.Id,       
            Systems = listSystemsRegistry
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
                })
                .ToList(),
            Menus = menuDtos,
            Roles = ssoRoles
        });
    }
}

