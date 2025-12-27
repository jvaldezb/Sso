using identity_service.Dtos;
using identity_service.Dtos.ProviderConfiguration;
using identity_service.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace identity_service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProviderConfigurationController : ControllerBase
{
    private readonly IProviderConfigurationService _service;
    private readonly ILogger<ProviderConfigurationController> _logger;

    public ProviderConfigurationController(
        IProviderConfigurationService service,
        ILogger<ProviderConfigurationController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las configuraciones de providers
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Result<List<ProviderConfigurationResponseDto>>>> GetAll()
    {
        try
        {
            var result = await _service.GetAllAsync();
            if (!result.IsSuccess)
                return StatusCode(500, result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving provider configurations");
            return StatusCode(500, Result<List<ProviderConfigurationResponseDto>>.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Obtiene una configuración específica por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Result<ProviderConfigurationResponseDto>>> GetById(Guid id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving provider configuration {Id}", id);
            return StatusCode(500, Result<ProviderConfigurationResponseDto>.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Crea una nueva configuración de provider
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Result<ProviderConfigurationResponseDto>>> Create([FromBody] ProviderConfigurationCreateDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            var result = await _service.CreateAsync(dto, userId);
            if (!result.IsSuccess)
                return StatusCode(500, result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating provider configuration");
            return StatusCode(500, Result<ProviderConfigurationResponseDto>.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Actualiza una configuración existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Result<ProviderConfigurationResponseDto>>> Update(Guid id, [FromBody] ProviderConfigurationUpdateDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            var result = await _service.UpdateAsync(id, dto, userId);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating provider configuration {Id}", id);
            return StatusCode(500, Result<ProviderConfigurationResponseDto>.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Elimina una configuración
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<Result<bool>>> Delete(Guid id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting provider configuration {Id}", id);
            return StatusCode(500, Result<bool>.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Obtiene la configuración LDAP actual (para testing)
    /// </summary>
    [HttpGet("ldap/current")]
    public async Task<ActionResult<Result<object>>> GetCurrentLdapConfiguration()
    {
        try
        {
            var result = await _service.GetLdapConfigurationAsync();
            if (!result.IsSuccess)
                return StatusCode(500, Result<object>.Failure(result.ErrorMessage!));

            var config = result.Data;
            if (config == null)
                return Ok(Result<object>.Success(new { Enabled = false, Message = "LDAP not configured" }));

            // No retornar contraseñas en la respuesta
            var safeConfig = new
            {
                config.Value.Enabled,
                config.Value.Server,
                config.Value.Port,
                config.Value.AdminDn,
                Settings = config.Value.Settings
            };

            return Ok(Result<object>.Success(safeConfig));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current LDAP configuration");
            return StatusCode(500, Result<object>.Failure("Internal server error"));
        }
    }
}
