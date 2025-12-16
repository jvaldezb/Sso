using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using identity_service.Dtos;
using identity_service.Dtos.RoleMenu;
using identity_service.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace identity_service.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class RoleMenuController : ControllerBase
{
    private readonly IRoleMenuService _service;

    public RoleMenuController(IRoleMenuService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var result = await _service.GetAllAsync(page, size);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpGet("roles/{roleId}")]
    public async Task<IActionResult> GetByRoleId(Guid roleId)
    {
        var result = await _service.GetByRoleIdAsync(roleId);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpGet("menus-by-role/{roleId}")]
    public async Task<IActionResult> GetMenusByRole(Guid roleId)
    {
        var result = await _service.GetMenusByRoleAsync(roleId);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpPatch("roles/{roleId}/access-levels")]
    public async Task<IActionResult> UpdateAccessLevels(Guid roleId, [FromBody] List<RoleMenuAccessUpdateDto> updates)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null) return Unauthorized("Token inválido");

        var result = await _service.UpdateAccessLevelsForRoleAsync(userId, roleId, updates);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

        return Ok(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RoleMenuCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null) return Unauthorized("Token inválido");

        var result = await _service.CreateAsync(userId, dto);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RoleMenuUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null) return Unauthorized("Token inválido");

        var result = await _service.UpdateAsync(userId, id, dto);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null) return Unauthorized("Token inválido");

        var result = await _service.DeleteAsync(userId, id);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

        return Ok(new { message = "RoleMenu eliminado correctamente." });
    }
}
