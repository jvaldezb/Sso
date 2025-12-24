using System;
using identity_service.Dtos;
using identity_service.Dtos.Menu;

namespace identity_service.Services.Interfaces;

public interface IMenuService
{
    Task<Result<MenuResponseDto>> CreateAsync(string performedByUserId, CreateMenuDto dto);
    Task<Result<MenuResponseDto>> UpdateAsync(string performedByUserId, Guid menuId, UpdateMenuDto dto);
    Task<Result<List<MenuResponseDto>>> BulkUpsertMenusBySystemAsync(string performedByUserId, Guid systemId, UpdateMenusBySystemDto dto);
    Task<Result<bool>> DeleteAsync(string performedByUserId, Guid menuId);
    Task<Result<PaginatedList<MenuResponseDto>>> GetAllAsync(int page, int size);
    Task<Result<MenuResponseDto>> GetByIdAsync(Guid menuId);
    Task<Result<List<MenuResponseDto>>> GetBySystemIdAsync(Guid systemId);
    Task<Result<List<MenuWithChildrenResponseDto>>> GetMenuHierarchyBySystemIdAsync(Guid systemId);
    Task<Result<List<MenuResponseDto>>> GetChildMenusAsync(Guid menuParentId);
}
