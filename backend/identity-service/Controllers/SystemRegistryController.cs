using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using identity_service.Dtos.SystemRegistry;
using identity_service.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace identity_service.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class SystemRegistryController : ControllerBase
{
    private readonly ISystemRegistryService _systemRegistryService;

    public SystemRegistryController(ISystemRegistryService systemRegistryService)
    {
        _systemRegistryService = systemRegistryService;
    }

    /// <summary>
    /// Create a new system registry
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSystemRegistryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _systemRegistryService.CreateAsync(userId, dto);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result.Data);
    }

    /// <summary>
    /// Update an existing system registry
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSystemRegistryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _systemRegistryService.UpdateAsync(userId, id, dto);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete a system registry
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _systemRegistryService.DeleteAsync(userId, id);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(new { message = "Sistema eliminado correctamente." });
    }

    /// <summary>
    /// Get all system registries
    /// <remarks>
    /// </remarks>    
    [HttpGet]    
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10)
    {        
        // Otherwise, get all with pagination
        var allResult = await _systemRegistryService.GetAllAsync(page, size);
        if (!allResult.IsSuccess)
            return BadRequest(allResult.ErrorMessage);

        return Ok(allResult.Data);
    }

    /// <summary>
    /// Get a system registry by ID
    /// </summary>
    [HttpGet("{id}")]    
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _systemRegistryService.GetByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound(result.ErrorMessage);

        return Ok(result.Data);
    }

    /// <summary>
    /// Get a system registry by system code
    /// </summary>      
    [HttpGet("code/{systemCode}")]    
    public async Task<IActionResult> GetByCode(string systemCode)
    {
        var result = await _systemRegistryService.GetByCodeAsync(systemCode);
        if (!result.IsSuccess)
            return NotFound(result.ErrorMessage);

        return Ok(result.Data);
    }

    /// <summary>
    /// Enable a system registry
    /// </summary>
    [HttpPost("{id}/enable")]
    public async Task<IActionResult> Enable(Guid id)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _systemRegistryService.SetEnabledAsync(userId, id, true);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(new { message = "Sistema habilitado correctamente." });
    }

    /// <summary>
    /// Disable a system registry
    /// </summary>
    [HttpPost("{id}/disable")]
    public async Task<IActionResult> Disable(Guid id)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            return Unauthorized("Token inválido");

            var result = await _systemRegistryService.SetEnabledAsync(userId, id, false);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(new { message = "Sistema deshabilitado correctamente." });
    }
}
