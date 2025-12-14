using System;
using System.Security.Cryptography;
using identity_service.Data;
using identity_service.Dtos.ExchangeToken;
using identity_service.Models;
using identity_service.Repositories.Interfaces;
using identity_service.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace identity_service.Services;

public class ExchangeTokenService : IExchangeTokenService
{
    private readonly AppDbContext _context;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IConfiguration _configuration;

    public ExchangeTokenService(
        AppDbContext context,
        ITokenGenerator tokenGenerator,
        IRefreshTokenService refreshTokenService,
        IConfiguration configuration,
        IUserSessionRepository userSessionRepository)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
        _refreshTokenService = refreshTokenService;
        _configuration = configuration;
        _userSessionRepository = userSessionRepository;
    }

    public string GenerateExchangeCode(
        Guid userId,
        Guid systemId,
        Guid sessionId,
        string? ipAddress,
        string? userAgent)
    {
        // Generate a unique exchange code (JWT ID)
        var jti = Guid.NewGuid();
        
        // Create exchange token record
        var exchangeToken = new ExchangeToken
        {
            Jti = jti,
            SystemId = systemId,
            UserId = userId,
            SessionId = sessionId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5), // Exchange codes expire in 5 minutes
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        _context.ExchangeTokens.Add(exchangeToken);
        _context.SaveChanges();

        // Return the exchange code (the JTI)
        return jti.ToString();
    }

    public async Task<ExchangeResponseDto> ExchangeCode(
        string exchangeCode, 
        Guid systemId, 
        string clientSecret)
    {
        // 1. Validate exchange code format first
        if (!Guid.TryParse(exchangeCode, out var jti))
            throw new UnauthorizedException("Codigo de intercambio invalido");

        // 2. Validate system and client secret
        var system = await _context.SystemRegistries
            .FirstOrDefaultAsync(s => s.Id == systemId && s.IsEnabled);

        if (system == null)
            throw new UnauthorizedException("Sistema no encontrado o deshabilitado");

        if (system.ApiKey != clientSecret)
            throw new UnauthorizedException("ApiKey invalido");

        // 3. Get and mark exchange token as used atomically
        var exchangeToken = await _context.ExchangeTokens
            .FirstOrDefaultAsync(x => 
                x.Jti == jti && 
                x.SystemId == systemId &&
                x.UsedAt == null &&
                x.ExpiresAt > DateTime.UtcNow);

        if (exchangeToken == null)
            throw new UnauthorizedException("Codigo de intercambio invalido, usado o expirado");

        // Mark as used immediately
        exchangeToken.UsedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // 4. Get user information
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == exchangeToken.UserId.ToString());
        
        if (user == null)
            throw new UnauthorizedException("Usuario no encontrado");

        if (!user.IsEnabled)
            throw new UnauthorizedException("Usuario deshabilitado");    

        // TODO: Lockout check
        //if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
        //    throw new UnauthorizedException("Usuario bloqueado temporalmente");    

        // 5. Validate session is still active
        if (!await _userSessionRepository.IsActiveAsync(exchangeToken.SessionId))
            throw new UnauthorizedException("Sesion invalida o expirada");

        // 6. Get user roles for this specific system
        var userRoles = await _context.UserRoles
            .Join(_context.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => new { ur.UserId, Role = r })
            .Where(x => x.UserId == user.Id && x.Role.SystemId == systemId)
            .Select(x => x.Role.Name!)
            .ToListAsync();

        // 7. Create user session for access token
        var jtiAccess = Guid.NewGuid().ToString();
        var expire = DateTime.UtcNow.AddMinutes(10);

        var accessSession = new UserSession
        {
            UserId = user.Id,
            JwtId = jtiAccess,
            TokenType = "access",
            SystemName = system.SystemCode,
            Device = exchangeToken.UserAgent ?? "Unknown",
            IpAddress = exchangeToken.IpAddress,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = expire,
            Audience = system.SystemCode,
            Scope = "read",
            UserCreate = user.UserName!
        };

        await _userSessionRepository.AddAsync(accessSession);        

        // 8. Generate access token for the specific system
        var (accessToken, expiresAt) = _tokenGenerator.GenerateSystemToken(
            user,
            accessSession,
            userRoles,
            system.SystemCode,
            "read",
            10
        );

        // 9. Generate refresh token
        var refreshToken = _refreshTokenService.GenerateRefreshToken(
            exchangeToken.IpAddress,
            exchangeToken.UserAgent,
            systemId,
            exchangeToken.SessionId
        );

        // 10. Save refresh token to database
        await _refreshTokenService.SaveRefreshTokenAsync(user, refreshToken);

        return new ExchangeResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = expiresAt,
            TokenType = "Bearer"
        };
    }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}
