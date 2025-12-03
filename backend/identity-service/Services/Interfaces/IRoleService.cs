using System;
using identity_service.Dtos;
using identity_service.Dtos.Role;

namespace identity_service.Services.Interfaces;

public interface IRoleService
{
    // Roles
    Task<Result<RoleDto>> CreateRoleAsync(string performedByUserId, CreateRoleDto dto);
    Task<Result<RoleDto>> UpdateRoleAsync(string performedByUserId, string roleId, UpdateRoleDto dto);
    Task<Result<bool>> DeleteRoleAsync(string performedByUserId, string roleId);
    Task<Result<PaginatedList<RoleDto>>> GetRolesAsync(int page, int size);
    Task<Result<RoleDto>> GetRoleByIdAsync(string roleId);

    // User-role assignment
    Task<Result<bool>> AddRoleToUserAsync(string performedByUserId, string userId, string roleName);
    Task<Result<bool>> RemoveRoleFromUserAsync(string performedByUserId, string userId, string roleName);
    Task<Result<List<RoleDto>>> GetUserRolesAsync(string userId);

    // Role claims
    Task<Result<bool>> AddClaimToRoleAsync(string performedByUserId, string roleId, string claimType, string claimValue);
    Task<Result<bool>> RemoveClaimFromRoleAsync(string performedByUserId, string roleId, string claimType, string claimValue);
    Task<Result<List<RoleClaimDto>>> GetRoleClaimsAsync(string roleId);

    // Menu integration & access
    Task<Result<bool>> UserHasAccessToMenuAsync(string userId, Guid menuId);
    Task<Result<List<MenuDto>>> GetAllowedMenusAsync(string userId);

    // Audit
    Task<Result<bool>> RecordRoleAuditAsync(string action, string performedByUserId, object details);

    // Enable/Disable role
    Task<Result<bool>> SetRoleEnabledAsync(string performedByUserId, string roleId, bool enabled);
}
