using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using identity_service.Dtos.Auth;
using identity_service.Dtos.RefreshToken;
using identity_service.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace identity_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login-document")]
        public async Task<IActionResult> LoginDocument(LoginDocumentDto dto)
        {
            var device = Request.Headers["User-Agent"].ToString();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            var result = await _authService.LoginDocumentAsync(dto, device, ip);

            if (!result.IsSuccess)
                return Unauthorized(result);
                
            return Ok(result);
        }

        [HttpPost("login-document-system")]
        public async Task<IActionResult> LoginDocumentSystem(LoginDocumentSystemDto dto)
        {
            var device = Request.Headers["User-Agent"].ToString();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.LoginDocumentSystemAsync(dto, device, ip);
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

            var result = await _authService.loginEmailAsync(loginEmailDto, device, ip);
            if (!result.IsSuccess)
            {
                return Unauthorized(result.ErrorMessage);
            }
            return Ok(result);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("token-for-system")]
        public async Task<IActionResult> TokenForSystem([FromBody] SystemTokenRequestDto dto)
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);

            if (userId == null || jti == null)
                return Unauthorized("Invalid token claims");

            var device = Request.Headers["User-Agent"].ToString();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            var result = await _authService.GenerateSystemAccessTokenAsync(userId, jti, dto.SystemName, dto.Scope, device, ip);

            if (!result.IsSuccess)
                return Unauthorized(result.ErrorMessage);

            return Ok(result.Data);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto dto)
        {
            var response = await _authService.RefreshTokenAsync(dto);
            return Ok(response);
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

            var result = await _authService.LogoutAsync(userId, dto.JwtId, ip, ua);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
