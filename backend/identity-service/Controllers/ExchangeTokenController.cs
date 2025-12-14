using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using identity_service.Dtos.ExchangeToken;
using identity_service.Services;
using identity_service.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace identity_service.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ExchangeTokenController : ControllerBase
{
    private readonly IExchangeTokenService _exchangeTokenService;

    public ExchangeTokenController(IExchangeTokenService exchangeTokenService)
    {
        _exchangeTokenService = exchangeTokenService;
    }

    /// <summary>
    /// Generate an exchange code for SSO authentication flow
    /// Requires a valid session token (from SSO central login)
    /// </summary>
    [HttpPost("generate")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult GenerateExchangeCode([FromBody] GenerateExchangeCodeDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token" });

            if (!Guid.TryParse(userId, out var userGuid))
                return BadRequest(new { message = "Invalid user ID format" });

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            var exchangeCode = _exchangeTokenService.GenerateExchangeCode(
                userGuid,
                dto.SystemId,
                dto.SessionId,
                ipAddress,
                userAgent
            );

            return Ok(new GenerateExchangeCodeResponseDto
            {
                ExchangeCode = exchangeCode,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Exchange a code for access and refresh tokens
    /// This endpoint is called by client systems with their client secret
    /// No authentication required as this is the authentication step
    /// </summary>
    [HttpPost("exchange")]
    public async Task<IActionResult> ExchangeCode([FromBody] ExchangeCodeRequestDto dto)
    {
        try
        {
            var result = await _exchangeTokenService.ExchangeCode(
                dto.ExchangeCode,
                dto.SystemId,
                dto.ClientSecret
            );

            return Ok(result);
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
