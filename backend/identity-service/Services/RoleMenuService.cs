using System;
using System.Linq;
using System.Threading.Tasks;
using identity_service.Dtos;
using identity_service.Dtos.RoleMenu;
using Microsoft.EntityFrameworkCore;
using identity_service.Models;
using identity_service.Repositories.Interfaces;
using identity_service.Services.Interfaces;

namespace identity_service.Services;

public class RoleMenuService : IRoleMenuService
{
    private readonly IRoleMenuRepository _roleMenuRepository;
    private readonly IMenuRepository _menuRepository;

    public RoleMenuService(IRoleMenuRepository roleMenuRepository, IMenuRepository menuRepository)
    {
        _roleMenuRepository = roleMenuRepository;
        _menuRepository = menuRepository;
    }

    public async Task<Result<PaginatedList<RoleMenuResponseDto>>> GetAllAsync(int page = 1, int size = 10)
    {
        try
        {
            if (page < 1) page = 1;
            if (size < 1) size = 10;

            var query = _roleMenuRepository.Query();
            var total = query.Count();
            var items = query.Skip((page - 1) * size).Take(size).ToList();

            var dtos = items.Select(rm => new RoleMenuResponseDto
            {
                Id = rm.Id,
                RoleId = rm.RoleId,
                MenuId = rm.MenuId,
                AccessLevel = rm.AccessLevel,
                UserCreate = rm.UserCreate,
                DateCreate = rm.DateCreate,
                UserUpdate = rm.UserUpdate,
                DateUpdate = rm.DateUpdate
            }).ToList();

            var paged = new PaginatedList<RoleMenuResponseDto>(dtos, total, page, size);
            return Result<PaginatedList<RoleMenuResponseDto>>.Success(paged);
        }
        catch (Exception ex)
        {
            return Result<PaginatedList<RoleMenuResponseDto>>.Failure($"Error retrieving role menus: {ex.Message}");
        }
    }

    public async Task<Result<RoleMenuResponseDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var item = await _roleMenuRepository.GetByIdAsync(id);
            if (item == null) return Result<RoleMenuResponseDto>.Failure("RoleMenu not found");

            var dto = new RoleMenuResponseDto
            {
                Id = item.Id,
                RoleId = item.RoleId,
                MenuId = item.MenuId,
                AccessLevel = item.AccessLevel,
                UserCreate = item.UserCreate,
                DateCreate = item.DateCreate,
                UserUpdate = item.UserUpdate,
                DateUpdate = item.DateUpdate
            };

            return Result<RoleMenuResponseDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<RoleMenuResponseDto>.Failure($"Error retrieving role menu: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateAccessLevelsForRoleAsync(string performedByUserId, string roleId, List<RoleMenuAccessUpdateDto> updates)
    {
        try
        {
            if (updates == null) return Result<bool>.Failure("No updates provided");

            foreach (var u in updates)
            {
                var existing = await _roleMenuRepository.Query().FirstOrDefaultAsync(rm => rm.RoleId == roleId && rm.MenuId == u.MenuId);

                if (existing != null)
                {
                    existing.AccessLevel = u.AccessLevel;
                    existing.UserUpdate = performedByUserId;
                    existing.DateUpdate = DateTimeOffset.UtcNow;
                    await _roleMenuRepository.UpdateAsync(existing);
                }
                else
                {
                    var entity = new RoleMenu
                    {
                        RoleId = roleId,
                        MenuId = u.MenuId,
                        AccessLevel = u.AccessLevel,
                        UserCreate = performedByUserId,
                        DateCreate = DateTimeOffset.UtcNow
                    };

                    await _roleMenuRepository.AddAsync(entity);
                }
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error updating access levels: {ex.Message}");
        }
    }

    public async Task<Result<List<MenuWithRoleMenuDto>>> GetMenusByRoleAsync(string roleId)
    {
        try
        {
            var roleMenus = _roleMenuRepository.Query().Where(rm => rm.RoleId == roleId).ToList();

            if (!roleMenus.Any())
                return Result<List<MenuWithRoleMenuDto>>.Success(new List<MenuWithRoleMenuDto>());

            var menuIds = roleMenus.Select(rm => rm.MenuId).Distinct().ToList();

            var menus = _menuRepository.Query().Where(m => menuIds.Contains(m.Id)).ToList();

            var combined = new List<MenuWithRoleMenuDto>();

            foreach (var rm in roleMenus)
            {
                var menu = menus.FirstOrDefault(m => m.Id == rm.MenuId);
                if (menu == null) continue;

                combined.Add(new MenuWithRoleMenuDto
                {
                    MenuId = menu.Id,
                    MenuLabel = menu.MenuLabel,
                    Module = menu.Module,
                    ModuleType = menu.ModuleType,
                    MenuType = menu.MenuType,
                    IconUrl = menu.IconUrl,
                    Level = menu.Level,
                    ParentMenuId = menu.ParentId,
                    OrderIndex = menu.OrderIndex,
                    RoleMenuId = rm.Id,
                    AccessLevel = rm.AccessLevel
                });
            }

            return Result<List<MenuWithRoleMenuDto>>.Success(combined);
        }
        catch (Exception ex)
        {
            return Result<List<MenuWithRoleMenuDto>>.Failure($"Error retrieving menus by role: {ex.Message}");
        }
    }

    public async Task<Result<RoleMenuResponseDto>> CreateAsync(string performedByUserId, RoleMenuCreateDto dto)
    {
        try
        {
            var entity = new RoleMenu
            {
                RoleId = dto.RoleId,
                MenuId = dto.MenuId,
                AccessLevel = dto.AccessLevel,
                UserCreate = performedByUserId,
                DateCreate = DateTime.UtcNow
            };

            var created = await _roleMenuRepository.AddAsync(entity);

            var response = new RoleMenuResponseDto
            {
                Id = created.Id,
                RoleId = created.RoleId,
                MenuId = created.MenuId,
                AccessLevel = created.AccessLevel,
                UserCreate = created.UserCreate,
                DateCreate = created.DateCreate
            };

            return Result<RoleMenuResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<RoleMenuResponseDto>.Failure($"Error creating role menu: {ex.Message}");
        }
    }

    public async Task<Result<RoleMenuResponseDto>> UpdateAsync(string performedByUserId, Guid id, RoleMenuUpdateDto dto)
    {
        try
        {
            var existing = await _roleMenuRepository.GetByIdAsync(id);
            if (existing == null) return Result<RoleMenuResponseDto>.Failure("RoleMenu not found");

            existing.RoleId = dto.RoleId;
            existing.MenuId = dto.MenuId;
            existing.AccessLevel = dto.AccessLevel;
            existing.UserUpdate = performedByUserId;
            existing.DateUpdate = DateTime.UtcNow;

            await _roleMenuRepository.UpdateAsync(existing);

            var response = new RoleMenuResponseDto
            {
                Id = existing.Id,
                RoleId = existing.RoleId,
                MenuId = existing.MenuId,
                AccessLevel = existing.AccessLevel,
                UserUpdate = existing.UserUpdate,
                DateUpdate = existing.DateUpdate
            };

            return Result<RoleMenuResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<RoleMenuResponseDto>.Failure($"Error updating role menu: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(string performedByUserId, Guid id)
    {
        try
        {
            var existing = await _roleMenuRepository.GetByIdAsync(id);
            if (existing == null) return Result<bool>.Failure("RoleMenu not found");

            var rows = await _roleMenuRepository.DeleteAsync(existing);
            return Result<bool>.Success(rows > 0);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error deleting role menu: {ex.Message}");
        }
    }
}
