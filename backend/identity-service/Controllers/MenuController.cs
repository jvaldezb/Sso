using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using identity_service.Dtos.Menu;
using identity_service.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace identity_service.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    /// <summary>
    /// Create a new menu
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMenuDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _menuService.CreateAsync(userId, dto);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return CreatedAtAction(nameof(GetById), new { Id = result.Data?.Id }, result.Data);
    }

    /// <summary>
    /// Update an existing menu
    /// </summary>
    [HttpPut("{Id}")]
    public async Task<IActionResult> Update(Guid Id, [FromBody] UpdateMenuDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _menuService.UpdateAsync(userId, Id, dto);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    

    /// <summary>
    /// Delete a menu
    /// </summary>
    [HttpDelete("{Id}")]
    public async Task<IActionResult> Delete(Guid Id)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _menuService.DeleteAsync(userId, Id);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(new { message = "Menú eliminado correctamente." });
    }

    /// <summary>
    /// Get all menus with pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var result = await _menuService.GetAllAsync(page, size);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    /// <summary>
    /// Get a menu by ID
    /// </summary>
    [HttpGet("{Id}")]
    public async Task<IActionResult> GetById(Guid Id)
    {
        var result = await _menuService.GetByIdAsync(Id);
        if (!result.IsSuccess)
            return NotFound(result.ErrorMessage);

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all menus by system ID
    /// </summary>
    [HttpGet("system/{systemId}")]
    public async Task<IActionResult> GetBySystemId(Guid systemId)
    {
        var result = await _menuService.GetBySystemIdAsync(systemId);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    /// <summary>
    /// Update multiple menus by system
    /// </summary>
    [HttpPost("system/{systemId}/batch")]
    public async Task<IActionResult> UpdateMenusBySystem(Guid systemId, [FromBody] UpdateMenusBySystemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            return Unauthorized("Token inválido");

            var result = await _menuService.BulkUpsertMenusBySystemAsync(userId, systemId, dto);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    /// <summary>
    /// Get menu hierarchy by system ID (includes child menus)
    /// </summary>
    [HttpGet("system/{systemId}/hierarchy")]
    public async Task<IActionResult> GetMenuHierarchyBySystemId(Guid systemId)
    {
        var result = await _menuService.GetMenuHierarchyBySystemIdAsync(systemId);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    /// <summary>
    /// Get child menus by parent ID
    /// </summary>
    [HttpGet("{Id}/children")]
    public async Task<IActionResult> GetChildMenus(Guid Id)
    {
        var result = await _menuService.GetChildMenusAsync(Id);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }
}
