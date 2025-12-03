using System;
using identity_service.Dtos;
using identity_service.Dtos.AuthAuditLog;
using identity_service.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace identity_service.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AuthAuditLogController : ControllerBase
{
    private readonly IAuthAuditLogService _authAuditLogService;

    public AuthAuditLogController(IAuthAuditLogService authAuditLogService)
    {
        _authAuditLogService = authAuditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _authAuditLogService.GetAllAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _authAuditLogService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(result.ErrorMessage);
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AuthAuditLogForCreateDto authAuditLogForCreateDto)
    {
        var result = await _authAuditLogService.CreateAsync(authAuditLogForCreateDto);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }
        if (result.Data == null)
        {
            return BadRequest("La creación del log fallo.");
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data.UserId }, result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _authAuditLogService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(result.ErrorMessage);
        }
        return Ok(result);
    }
}
