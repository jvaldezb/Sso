using System;
using identity_service.Dtos;
using identity_service.Dtos.RoleMenu;

namespace identity_service.Services.Interfaces;

public interface IRoleMenuService
{
    Task<Result<PaginatedList<RoleMenuResponseDto>>> GetAllAsync(int page = 1, int size = 10);
    Task<Result<RoleMenuResponseDto>> GetByIdAsync(Guid id);    
    Task<Result<List<MenuWithRoleMenuDto>>> GetMenusByRoleAsync(string roleId);
    Task<Result<bool>> UpdateAccessLevelsForRoleAsync(string performedByUserId, string roleId, List<RoleMenuAccessUpdateDto> updates);
    Task<Result<RoleMenuResponseDto>> CreateAsync(string performedByUserId, RoleMenuCreateDto dto);
    Task<Result<RoleMenuResponseDto>> UpdateAsync(string performedByUserId, Guid id, RoleMenuUpdateDto dto);
    Task<Result<bool>> DeleteAsync(string performedByUserId, Guid id);
}
