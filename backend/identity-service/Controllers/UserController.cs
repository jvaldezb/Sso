using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using identity_service.Dtos;
using identity_service.Dtos.User;
using identity_service.Repositories;
using identity_service.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace identity_service.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserForCreateDto userDto)
    {
        var result = await _userService.registerAsync(userDto);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    [HttpPost("login-document")]
    public async Task<IActionResult> LoginDocument(LoginDocumentDto dto)
    {
        var device = Request.Headers["User-Agent"].ToString();
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _userService.LoginDocumentAsync(dto, device, ip);

        if (!result.IsSuccess)
            return Unauthorized(result);
            
        return Ok(result);
    }

    [HttpPost("login-document-system")]
    public async Task<IActionResult> LoginDocumentSystem(LoginDocumentSystemDto dto)
    {
        var device = Request.Headers["User-Agent"].ToString();
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _userService.LoginDocumentSystemAsync(dto, device, ip);
        if (!result.IsSuccess)
        {
            return Unauthorized(result.ErrorMessage);
        }
        return Ok(result);
    }

    [HttpPost("login-email")]
    public async Task<IActionResult> LoginEmail(LoginEmailDto loginEmailDto)
    {
        var device = Request.Headers["User-Agent"].ToString();
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _userService.loginEmailAsync(loginEmailDto, device, ip);
        if (!result.IsSuccess)
        {
            return Unauthorized(result.ErrorMessage);
        }
        return Ok(result);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPatch("{userId}/enable")]
    public async Task<IActionResult> EnableUser(string userId)
    {
        var performedByUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (performedByUserId == null)
            return Unauthorized("Token inválido");

        var result = await _userService.SetUserEnabledAsync(performedByUserId, userId, true);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(new { message = "Usuarion habilitado correctamente." });
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPatch("{userId}/disable")]
    public async Task<IActionResult> DisableUser(string userId)
    {
        var performedByUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (performedByUserId == null)
            return Unauthorized("Token inválido");

        var result = await _userService.SetUserEnabledAsync(performedByUserId, userId, false);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(new { message = "Usuario deshabilitado correctamente." });
    }

    [HttpPost("token-for-system")]
    public async Task<IActionResult> TokenForSystem([FromBody] SystemTokenRequestDto dto)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);

        if (userId == null || jti == null)
            return Unauthorized("Invalido token claims");

        var device = Request.Headers["User-Agent"].ToString();
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        try
        {
            var (token, expires) = await _userService.GenerateAccessTokenAsync(userId, jti, dto.SystemName, dto.Scope, device, ip);
            return Ok(new { token, expires, system = dto.SystemName });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    // =====================================================================
    // TOKEN VALIDATION & SESSION MANAGEMENT
    // =====================================================================

    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequestDto dto)
    {
        var result = await _userService.ValidateTokenAsync(dto.Token);

        if (!result.IsSuccess)
            return Unauthorized(result);

        return Ok(result);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userId == null)
            return Unauthorized("token inválido");

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers["User-Agent"].ToString();

        var result = await _userService.LogoutAsync(userId, dto.JwtId, ip, ua);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _userService.GetActiveSessionsAsync(userId);

        return Ok(result);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _userService.GetUserRolesAsync(userId);

        return Ok(result);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest("Numbero de página o tamaño de página inválido.");

        var result = await _userService.GetUsersAsync(pageNumber, pageSize);

        return Ok(result);
    }

    // =====================================================================
    // EMAIL VERIFICATION ENDPOINTS
    // =====================================================================

    [HttpPost("send-verification-email")]
    public async Task<IActionResult> SendVerificationEmail([FromBody] SendVerificationEmailDto dto)
    {
        var result = await _userService.SendVerificationEmailAsync(dto.Email);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result.Data);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
    {
        var result = await _userService.VerifyEmailAsync(dto.Email, dto.VerificationCode);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result.Data);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("email-verified")]
    public async Task<IActionResult> IsEmailVerified()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _userService.IsEmailVerifiedAsync(userId);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(new { emailVerified = result.Data });
    }

    [HttpPost("resend-verification-email")]
    public async Task<IActionResult> ResendVerificationEmail([FromBody] SendVerificationEmailDto dto)
    {
        var result = await _userService.ResendVerificationEmailAsync(dto.Email);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(new { success = true, message = "Verificación de email enviado" });
    }

    // =====================================================================
    // TWO-FACTOR AUTHENTICATION (MFA) ENDPOINTS
    // =====================================================================

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("mfa/setup")]
    public async Task<IActionResult> SetupMfa()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _userService.GenerateMfaSecretAsync(userId);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result.Data);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("mfa/verify-setup")]
    public async Task<IActionResult> VerifyMfaSetup([FromBody] VerifyMfaSetupDto dto)
    {
        var result = await _userService.VerifyMfaSetupAsync(dto.UserId, dto.Code);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result.Data);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("mfa/status")]
    public async Task<IActionResult> GetMfaStatus()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _userService.GetMfaStatusAsync(userId);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result.Data);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("mfa/disable")]
    public async Task<IActionResult> DisableMfa([FromBody] DisableMfaDto dto)
    {
        var result = await _userService.DisableMfaAsync(dto.UserId);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(new { success = true, message = "MFA deshabilitado correctamente." });
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("mfa/validate-code")]
    public async Task<IActionResult> ValidateMfaCode([FromBody] VerifyMfaCodeDto dto)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userId == null)
            return Unauthorized("Invalid token");

        var result = await _userService.ValidateMfaCodeAsync(userId, dto.Code);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(new { success = true, message = "Código de MFA verificado" });
    }

    // =====================================================================
    // Obtener usuario actual
    // =====================================================================
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _userService.GetCurrentUserAsync(userId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result.Data);
    }
}
