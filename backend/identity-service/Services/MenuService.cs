using System;
using AutoMapper;
using identity_service.Data;
using identity_service.Dtos;
using identity_service.Dtos.Menu;
using identity_service.Models;
using identity_service.Repositories.Interfaces;
using identity_service.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace identity_service.Services;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepository;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public MenuService(
        IMenuRepository repository,
        AppDbContext context,
        IMapper mapper,
        UserManager<ApplicationUser> userManager)
    {
        _menuRepository = repository;
        _context = context;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<Result<MenuResponseDto>> CreateAsync(string performedByUserId, CreateMenuDto dto)
    {
        try
        {
            // Validate system exists
            var systemExists = await _context.SystemRegistries.AnyAsync(s => s.Id == dto.SystemId);
            if (!systemExists)
                return Result<MenuResponseDto>.Failure("Sistema no encontrado");

            // Validate parent menu if specified
            if (dto.ParentId.HasValue)
            {
                var parentExists = await _context.Menus.AnyAsync(m => m.Id == dto.ParentId.Value);
                if (!parentExists)
                    return Result<MenuResponseDto>.Failure("Menú padre no encontrado");
            }

            var user = await _userManager.FindByIdAsync(performedByUserId);
            if (user == null)
                return Result<MenuResponseDto>.Failure("Usuario no encontrado");

            var menu = _mapper.Map<Menu>(dto);
            menu.UserCreate = user.UserName;
            menu.DateCreate = DateTimeOffset.UtcNow;

            await _menuRepository.AddAsync(menu);            

            var result = _mapper.Map<MenuResponseDto>(menu);
            return Result<MenuResponseDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<MenuResponseDto>.Failure($"Error al crear el menú: {ex.Message}");
        }
    }

    public async Task<Result<MenuResponseDto>> UpdateAsync(string performedByUserId, Guid id, UpdateMenuDto dto)
    {
        try
        {
            var menu = await _menuRepository.GetByIdAsync(id);
            if (menu == null)
                return Result<MenuResponseDto>.Failure("Menú no encontrado");

            // Validate system if specified
            if (dto.SystemId.HasValue)
            {
                var systemExists = await _context.SystemRegistries.AnyAsync(s => s.Id == dto.SystemId.Value);
                if (!systemExists)
                    return Result<MenuResponseDto>.Failure("Sistema no encontrado");
            }

            // Validate parent menu if specified
            if (dto.ParentId.HasValue)
            {
                var parentExists = await _context.Menus.AnyAsync(m => m.Id == dto.ParentId.Value);
                if (!parentExists)
                    return Result<MenuResponseDto>.Failure("Menú padre no encontrado");

                // Prevent circular reference
                if (dto.ParentId.Value == id)
                    return Result<MenuResponseDto>.Failure("Un menú no puede ser su propio padre");
            }

            var user = await _userManager.FindByIdAsync(performedByUserId);
            if (user == null)
                return Result<MenuResponseDto>.Failure("Usuario no encontrado");

            // Update only specified fields
            if (dto.ParentId.HasValue) menu.ParentId = dto.ParentId;
            if (dto.SystemId.HasValue) menu.SystemId = dto.SystemId.Value;
            if (!string.IsNullOrWhiteSpace(dto.MenuLabel)) menu.MenuLabel = dto.MenuLabel;
            if (dto.Description != null) menu.Description = dto.Description;
            if (dto.Level.HasValue) menu.Level = dto.Level.Value;
            if (dto.Module != null) menu.Module = dto.Module;
            if (dto.ModuleType != null) menu.ModuleType = dto.ModuleType;
            if (dto.MenuType != null) menu.MenuType = dto.MenuType;            
            if (dto.IconUrl != null) menu.IconUrl = dto.IconUrl;
            if (dto.OrderIndex.HasValue) menu.OrderIndex = dto.OrderIndex.Value;
            if (dto.BitPosition.HasValue) menu.BitPosition = dto.BitPosition;
            if (dto.Url != null) menu.Url = dto.Url;
            if (dto.IsEnabled.HasValue) menu.IsEnabled = dto.IsEnabled.Value;

            menu.UserUpdate = user.UserName;
            menu.DateUpdate = DateTimeOffset.UtcNow;

            await _menuRepository.UpdateAsync(menu);            

            var result = _mapper.Map<MenuResponseDto>(menu);
            return Result<MenuResponseDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<MenuResponseDto>.Failure($"Error al actualizar el menú: {ex.Message}");
        }
    }

    public async Task<Result<List<MenuResponseDto>>> BulkUpsertMenusBySystemAsync(string performedByUserId, Guid systemId, UpdateMenusBySystemDto dto)
    {
        try
        {
            // Validate system exists
            var systemExists = await _context.SystemRegistries.AnyAsync(s => s.Id == systemId);
            if (!systemExists)
                return Result<List<MenuResponseDto>>.Failure("Sistema no encontrado");

            var user = await _userManager.FindByIdAsync(performedByUserId);
            if (user == null)
                return Result<List<MenuResponseDto>>.Failure("Usuario no encontrado");

            if (dto.Menus == null || !dto.Menus.Any())
                return Result<List<MenuResponseDto>>.Failure("La lista de menús no puede estar vacía");

            // Validate that each item has either MenuId or Module
            var invalidItems = dto.Menus.Where(m => 
                (!m.Id.HasValue || m.Id == Guid.Empty) && 
                string.IsNullOrWhiteSpace(m.Module)).ToList();
            
            if (invalidItems.Any())
                return Result<List<MenuResponseDto>>.Failure("Cada menú debe tener MenuId o Module para poder identificarlo");

            // Collect all IDs and modules for batch loading
            var providedIds = dto.Menus
                .Where(m => m.Id.HasValue && m.Id != Guid.Empty)
                .Select(m => m.Id!.Value)
                .ToList();

            var providedModules = dto.Menus
                .Where(m => !string.IsNullOrWhiteSpace(m.Module))
                .Select(m => m.Module!)
                .ToList();

            // Load existing menus by ID or Module in one query
            var existingMenus = await _context.Menus
                .Where(m => m.SystemId == systemId && 
                           (providedIds.Contains(m.Id) || providedModules.Contains(m.Module)))
                .ToListAsync();

            // Check for conflicts: IDs that exist but belong to different system
            var menusWithProvidedIds = await _context.Menus
                .Where(m => providedIds.Contains(m.Id))
                .ToListAsync();

            var conflictingIds = menusWithProvidedIds
                .Where(m => m.SystemId != systemId)
                .Select(m => m.Id)
                .ToList();

            if (conflictingIds.Any())
                return Result<List<MenuResponseDto>>.Failure(
                    $"Los siguientes MenuId existen pero pertenecen a otro sistema: {string.Join(", ", conflictingIds)}");

            // Build lookup dictionaries
            var existingById = existingMenus.Where(m => providedIds.Contains(m.Id))
                .ToDictionary(m => m.Id, m => m);
            var existingByModule = existingMenus.Where(m => !string.IsNullOrWhiteSpace(m.Module))
                .ToDictionary(m => m.Module, m => m);

            var updatedMenus = new List<Menu>();
            var createdMenus = new List<Menu>();

            foreach (var menuDto in dto.Menus)
            {
                Menu? menuToUpdate = null;

                // Priority 1: Look up by Id if provided
                if (menuDto.Id.HasValue && menuDto.Id != Guid.Empty)
                {
                    existingById.TryGetValue(menuDto.Id.Value, out menuToUpdate);
                }
                // Priority 2: Look up by Module if Id not found or not provided
                else if (!string.IsNullOrWhiteSpace(menuDto.Module))
                {
                    existingByModule.TryGetValue(menuDto.Module, out menuToUpdate);
                }

                // Validate parent menu if specified
                if (menuDto.ParentId.HasValue)
                {
                    // Check if parent will be created/updated in this batch
                    var parentInBatch = dto.Menus.Any(m => 
                        (m.Id.HasValue && m.Id == menuDto.ParentId) || 
                        (!string.IsNullOrWhiteSpace(m.Module) && existingByModule.ContainsKey(m.Module) && existingByModule[m.Module].Id == menuDto.ParentId));

                    if (!parentInBatch)
                    {
                        // Parent must exist in DB for this system
                        var parentExists = await _context.Menus
                            .AnyAsync(m => m.Id == menuDto.ParentId.Value && m.SystemId == systemId);
                        if (!parentExists)
                            return Result<List<MenuResponseDto>>.Failure(
                                $"Menú padre {menuDto.ParentId} no encontrado en el sistema");
                    }

                    // Prevent self-reference
                    if (menuToUpdate != null && menuDto.ParentId.Value == menuToUpdate.Id)
                        return Result<List<MenuResponseDto>>.Failure(
                            $"El menú no puede ser su propio padre");
                }

                if (menuToUpdate != null)
                {
                    // Update existing
                    if (menuDto.ParentId.HasValue) menuToUpdate.ParentId = menuDto.ParentId;
                    if (!string.IsNullOrWhiteSpace(menuDto.MenuLabel)) menuToUpdate.MenuLabel = menuDto.MenuLabel;
                    if (menuDto.Description != null) menuToUpdate.Description = menuDto.Description;
                    if (menuDto.Level.HasValue) menuToUpdate.Level = menuDto.Level.Value;
                    if (!string.IsNullOrWhiteSpace(menuDto.Module)) menuToUpdate.Module = menuDto.Module;
                    if (menuDto.ModuleType != null) menuToUpdate.ModuleType = menuDto.ModuleType;
                    if (menuDto.MenuType != null) menuToUpdate.MenuType = menuDto.MenuType;
                    if (menuDto.IconUrl != null) menuToUpdate.IconUrl = menuDto.IconUrl;
                    if (menuDto.OrderIndex.HasValue) menuToUpdate.OrderIndex = menuDto.OrderIndex.Value;
                    if (menuDto.Url != null) menuToUpdate.Url = menuDto.Url;
                    if (menuDto.IsEnabled.HasValue) menuToUpdate.IsEnabled = menuDto.IsEnabled.Value;

                    menuToUpdate.UserUpdate = user.UserName;
                    menuToUpdate.DateUpdate = DateTimeOffset.UtcNow;

                    updatedMenus.Add(menuToUpdate);
                }
                else
                {
                    // CREATE new menu
                    // Require Module for creation
                    if (string.IsNullOrWhiteSpace(menuDto.Module))
                        return Result<List<MenuResponseDto>>.Failure("Module es requerido para crear un nuevo menú");

                    var newMenu = new Menu
                    {
                        Id = (menuDto.Id.HasValue && menuDto.Id != Guid.Empty) 
                            ? menuDto.Id.Value 
                            : Guid.NewGuid(),
                        SystemId = systemId,
                        ParentId = menuDto.ParentId,
                        MenuLabel = menuDto.MenuLabel ?? string.Empty,
                        Description = menuDto.Description,
                        Level = menuDto.Level ?? 1,
                        Module = menuDto.Module,
                        ModuleType = menuDto.ModuleType,
                        MenuType = menuDto.MenuType,
                        RequiredClaimType = null,
                        RequiredClaimMinValue = 4,
                        IconUrl = menuDto.IconUrl,
                        AccessScope = null,
                        OrderIndex = menuDto.OrderIndex ?? 1,
                        Url = menuDto.Url,
                        UserCreate = user.UserName,
                        DateCreate = DateTimeOffset.UtcNow
                    };

                    await _context.Menus.AddAsync(newMenu);
                    createdMenus.Add(newMenu);

                    // Add to dictionary for subsequent parent lookups in same batch
                    existingByModule[newMenu.Module] = newMenu;
                    if (newMenu.Id != Guid.Empty)
                        existingById[newMenu.Id] = newMenu;
                }
            }

            // Save all changes in single transaction
            await _context.SaveChangesAsync();

            var allResultMenus = updatedMenus.Concat(createdMenus).ToList();
            var result = _mapper.Map<List<MenuResponseDto>>(allResultMenus);
            return Result<List<MenuResponseDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<List<MenuResponseDto>>.Failure($"Error al procesar los menús: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(string performedByUserId, Guid id)
    {
        try
        {
            var menu = await _menuRepository.GetByIdAsync(id);
            if (menu == null)
                return Result<bool>.Failure("Menú no encontrado");

            var user = await _userManager.FindByIdAsync(performedByUserId);
            if (user == null)
                return Result<bool>.Failure("Usuario no encontrado");

            // Check if menu has children
            var hasChildren = await _context.Menus.AnyAsync(m => m.ParentId == id);
            if (hasChildren)
                return Result<bool>.Failure("No se puede eliminar un menú con submenús. Elimine los submenús primero.");

            await _menuRepository.DeleteAsync(menu);
            

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error al eliminar el menú: {ex.Message}");
        }
    }

    public async Task<Result<PaginatedList<MenuResponseDto>>> GetAllAsync(int page, int size)
    {
        try
        {
            var query = _context.Menus
                .OrderBy(m => m.SystemId)
                .ThenBy(m => m.Level)
                .ThenBy(m => m.OrderIndex)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();                       

            var mappedItems = _mapper.Map<List<MenuResponseDto>>(items);

            // agregar system code y name
            var systemIds = items.Select(m => m.SystemId).Distinct().ToList();
            var systems = await _context.SystemRegistries
                .Where(s => systemIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s); 

            foreach (var menuDto in mappedItems)
            {
                if (systems.TryGetValue(menuDto.SystemId, out var system))
                {
                    menuDto.SystemCode = system.SystemCode;
                    menuDto.SystemName = system.SystemName;
                }
            }    

            var paginatedList = new PaginatedList<MenuResponseDto>(mappedItems, totalCount, page, size);

            return Result<PaginatedList<MenuResponseDto>>.Success(paginatedList);
        }
        catch (Exception ex)
        {
            return Result<PaginatedList<MenuResponseDto>>.Failure($"Error al obtener los menús: {ex.Message}");
        }
    }

    public async Task<Result<MenuResponseDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var menu = await _context.Menus
                .Include(m => m.ChildMenus)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menu == null)
                return Result<MenuResponseDto>.Failure("Menú no encontrado");

            var result = _mapper.Map<MenuResponseDto>(menu);
            return Result<MenuResponseDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<MenuResponseDto>.Failure($"Error al obtener el menú: {ex.Message}");
        }
    }

    public async Task<Result<List<MenuResponseDto>>> GetBySystemIdAsync(Guid systemId)
    {
        try
        {
            var menus = await _context.Menus
                .Where(m => m.SystemId == systemId)
                .OrderBy(m => m.Level)
                .ThenBy(m => m.OrderIndex)
                .ToListAsync();

            var result = _mapper.Map<List<MenuResponseDto>>(menus);
            return Result<List<MenuResponseDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<List<MenuResponseDto>>.Failure($"Error al obtener los menús del sistema: {ex.Message}");
        }
    }

    public async Task<Result<List<MenuWithChildrenResponseDto>>> GetMenuHierarchyBySystemIdAsync(Guid systemId)
    {
        try
        {
            // Get all menus for the system
            var allMenus = await _context.Menus
                .Where(m => m.SystemId == systemId)
                .OrderBy(m => m.Level)
                .ThenBy(m => m.OrderIndex)
                .ToListAsync();

            // Get root menus (no parent)
            var rootMenus = allMenus.Where(m => m.ParentId == null).ToList();
            
            var result = new List<MenuWithChildrenResponseDto>();
            foreach (var rootMenu in rootMenus)
            {
                var menuDto = _mapper.Map<MenuWithChildrenResponseDto>(rootMenu);
                menuDto.ChildMenus = BuildMenuHierarchy(rootMenu.Id, allMenus);
                result.Add(menuDto);
            }

            return Result<List<MenuWithChildrenResponseDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<List<MenuWithChildrenResponseDto>>.Failure($"Error al obtener la jerarquía de menús: {ex.Message}");
        }
    }

    public async Task<Result<List<MenuResponseDto>>> GetChildMenusAsync(Guid parentId)
    {
        try
        {
            var childMenus = await _context.Menus
                .Where(m => m.ParentId == parentId)
                .OrderBy(m => m.OrderIndex)
                .ToListAsync();

            var result = _mapper.Map<List<MenuResponseDto>>(childMenus);
            return Result<List<MenuResponseDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<List<MenuResponseDto>>.Failure($"Error al obtener los submenús: {ex.Message}");
        }
    }

    private List<MenuWithChildrenResponseDto> BuildMenuHierarchy(Guid parentId, List<Menu> allMenus)
    {
        var children = allMenus
            .Where(m => m.ParentId == parentId)
            .OrderBy(m => m.OrderIndex)
            .ToList();

        var result = new List<MenuWithChildrenResponseDto>();
        foreach (var child in children)
        {
            var childDto = _mapper.Map<MenuWithChildrenResponseDto>(child);
            childDto.ChildMenus = BuildMenuHierarchy(child.Id, allMenus);
            result.Add(childDto);
        }

        return result;
    }
}
