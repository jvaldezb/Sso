using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using identity_service.Dtos;
using identity_service.Dtos.Role;
using identity_service.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace identity_service.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    #region Role Management

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _roleService.CreateRoleAsync(userId, dto);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return CreatedAtAction(nameof(GetRoleById), new { roleId = result.Data?.Id }, result.Data);
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    [HttpPut("{roleId}")]
    public async Task<IActionResult> UpdateRole(string roleId, [FromBody] UpdateRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _roleService.UpdateRoleAsync(userId, roleId, dto);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    [HttpDelete("{roleId}")]
    public async Task<IActionResult> DeleteRole(string roleId)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _roleService.DeleteRoleAsync(userId, roleId);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(new { message = "Role eliminado correctamente." });
    }

    /// <summary>
    /// Get all roles with pagination
    /// </summary>
    [HttpGet]    
    public async Task<IActionResult> GetRoles([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var result = await _roleService.GetRolesAsync(page, size);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    /// <summary>
    /// Get a role by ID
    /// </summary>
    [HttpGet("{roleId}")]    
    public async Task<IActionResult> GetRoleById(string roleId)
    {
        var result = await _roleService.GetRoleByIdAsync(roleId);
        if (!result.IsSuccess)
            return NotFound(result.ErrorMessage);

        return Ok(result.Data);
    }

    #endregion

    #region User-Role Assignment

    /// <summary>
    /// Add a role to a user
    /// </summary>
    [HttpPost("{roleId}/users/{userId}")]
    public async Task<IActionResult> AddRoleToUser(string roleId, string userId)
    {
        var performedByUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (performedByUserId == null)
            return Unauthorized("Token inválido");

        // Get role name from roleId
        var roleResult = await _roleService.GetRoleByIdAsync(roleId);
        if (!roleResult.IsSuccess)
            return NotFound("Role no encontrado");

        var result = await _roleService.AddRoleToUserAsync(performedByUserId, userId, roleResult.Data!.Name);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(new { message = $"Role '{roleResult.Data.Name}' agregado al usuario correctamente." });
    }

    /// <summary>
    /// Remove a role from a user
    /// </summary>
    [HttpDelete("{roleId}/users/{userId}")]
    public async Task<IActionResult> RemoveRoleFromUser(string roleId, string userId)
    {
        var performedByUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (performedByUserId == null)
            return Unauthorized("Token inválido");

        // Get role name from roleId
        var roleResult = await _roleService.GetRoleByIdAsync(roleId);
        if (!roleResult.IsSuccess)
            return NotFound("Role no encontrado");

        var result = await _roleService.RemoveRoleFromUserAsync(performedByUserId, userId, roleResult.Data!.Name);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(new { message = $"Role '{roleResult.Data.Name}' eliminado del usuario correctamente." });
    }

    /// <summary>
    /// Get all roles assigned to a user
    /// </summary>
    [HttpGet("users/{userId}/roles")]
    public async Task<IActionResult> GetUserRoles(string userId)
    {
        var result = await _roleService.GetUserRolesAsync(userId);
        if (!result.IsSuccess)
            return NotFound(result.ErrorMessage);

        return Ok(result.Data);
    }

    #endregion

    #region Role Claims

    /// <summary>
    /// Add a claim to a role
    /// </summary>
    [HttpPost("{roleId}/claims")]
    public async Task<IActionResult> AddClaimToRole(string roleId, [FromBody] RoleClaimDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _roleService.AddClaimToRoleAsync(userId, roleId, dto.Type, dto.Value);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(new { message = "Claim agregado al role correctamente." });
    }

    /// <summary>
    /// Remove a claim from a role
    /// </summary>
    [HttpDelete("{roleId}/claims")]
    public async Task<IActionResult> RemoveClaimFromRole(string roleId, [FromQuery] string claimType, [FromQuery] string claimValue)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _roleService.RemoveClaimFromRoleAsync(userId, roleId, claimType, claimValue);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(new { message = "Claim eliminado del role correctamente" });
    }

    /// <summary>
    /// Get all claims for a role
    /// </summary>
    [HttpGet("{roleId}/claims")]    
    public async Task<IActionResult> GetRoleClaims(string roleId)
    {
        var result = await _roleService.GetRoleClaimsAsync(roleId);
        if (!result.IsSuccess)
            return NotFound(result.ErrorMessage);

        return Ok(result.Data);
    }

    #endregion

    #region Menu Integration & Access

    /// <summary>
    /// Check if a user has access to a specific menu
    /// </summary>
    [HttpGet("users/{userId}/menus/{menuId}/access")]    
    public async Task<IActionResult> UserHasAccessToMenu(string userId, Guid menuId)
    {
        var result = await _roleService.UserHasAccessToMenuAsync(userId, menuId);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(new { userId, menuId, hasAccess = result.Data });
    }

    /// <summary>
    /// Get all menus a user has access to
    /// </summary>
    [HttpGet("users/{userId}/menus")]    
    public async Task<IActionResult> GetAllowedMenus(string userId)
    {
        var result = await _roleService.GetAllowedMenusAsync(userId);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all menus a role grants access to
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    [HttpGet("{roleId}/menus")]    
    public async Task<IActionResult> GetRoleMenus(string roleId)
    {
        var result = await _roleService.GetRoleMenusAsync(roleId);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        return Ok(result.Data);
    }

    /// <summary>
    /// Set access menus for a role
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    [HttpPatch("{roleId}/menus")]
    public async Task<IActionResult> SetRoleMenus(string roleId, [FromBody] List<MenuRoleRwxRequestDto> menus)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _roleService.SetRoleMenusAsync(userId, roleId, menus);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(new { message = "Menus del role actualizados correctamente." });
    }

    #endregion

    #region Enable/Disable Role

    [HttpPatch("{roleId}/enable")]
    public async Task<IActionResult> EnableRole(string roleId)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _roleService.SetRoleEnabledAsync(userId, roleId, true);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(new { message = "Role habilitado correctamente" });
    }

    [HttpPatch("{roleId}/disable")]
    public async Task<IActionResult> DisableRole(string roleId)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            return Unauthorized("Token inválido");

        var result = await _roleService.SetRoleEnabledAsync(userId, roleId, false);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(new { message = "Role deshabilitado correctamente" });
    }

    #endregion
}
