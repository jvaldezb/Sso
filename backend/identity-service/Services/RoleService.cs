using System;
using System.Security.Claims;
using System.Text.Json;
using AutoMapper;
using FluentValidation;
using identity_service.Data;
using identity_service.Dtos;
using identity_service.Dtos.Role;
using identity_service.Models;
using identity_service.Repositories.Interfaces;
using identity_service.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace identity_service.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;
    private readonly IMenuRepository _menuRepository;
    private readonly IRoleClaimEncoderService _roleClaimEncoderService;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateRoleDto> _createValidator;
    private readonly IValidator<UpdateRoleDto> _updateValidator;

    public RoleService(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        AppDbContext context,
        IMenuRepository menuRepository,
        IMapper mapper,
        IValidator<CreateRoleDto> createValidator,
        IValidator<UpdateRoleDto> updateValidator,
        IRoleClaimEncoderService roleClaimEncoderService)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
        _menuRepository = menuRepository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _roleClaimEncoderService = roleClaimEncoderService;
    }

    #region Role Management

    public async Task<Result<RoleDto>> CreateRoleAsync(string performedByUserId, CreateRoleDto dto)
    {
        try
        {
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return Result<RoleDto>.Failure(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

            var roleExists = await _roleManager.RoleExistsAsync(dto.Name);
            if (roleExists)
                return Result<RoleDto>.Failure($"Role '{dto.Name}' already exists");

            var role = _mapper.Map<ApplicationRole>(dto);
            role.UserCreate = performedByUserId;
            role.DateCreate = DateTime.UtcNow;

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
                return Result<RoleDto>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

            await RecordRoleAuditAsync("CREATE", performedByUserId, new { RoleName = dto.Name, SystemId = dto.SystemId });

            var roleDto = _mapper.Map<RoleDto>(role);
            return Result<RoleDto>.Success(roleDto);
        }
        catch (Exception ex)
        {
            return Result<RoleDto>.Failure($"Error creating role: {ex.Message}");
        }
    }

    public async Task<Result<RoleDto>> UpdateRoleAsync(string performedByUserId, string roleId, UpdateRoleDto dto)
    {
        try
        {
            var validation = await _updateValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return Result<RoleDto>.Failure(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Result<RoleDto>.Failure("Role not found");

            var roleWithSameName = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Name && r.Id != roleId);
            if (roleWithSameName != null)
                return Result<RoleDto>.Failure($"A role with name '{dto.Name}' already exists");

            _mapper.Map(dto, role);
            role.UserUpdate = performedByUserId;
            role.DateUpdate = DateTime.UtcNow;

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
                return Result<RoleDto>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

            await RecordRoleAuditAsync("UPDATE", performedByUserId, new { RoleId = roleId, NewName = dto.Name, SystemId = dto.SystemId });

            var roleDto = _mapper.Map<RoleDto>(role);
            return Result<RoleDto>.Success(roleDto);
        }
        catch (Exception ex)
        {
            return Result<RoleDto>.Failure($"Error updating role: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteRoleAsync(string performedByUserId, string roleId)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Result<bool>.Failure("Role not found");

            // Check if role is assigned to any users
            var usersInRole = await _context.UserRoles.Where(ur => ur.RoleId == roleId).ToListAsync();
            if (usersInRole.Any())
                return Result<bool>.Failure("Cannot delete a role that is assigned to users");

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return Result<bool>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

            await RecordRoleAuditAsync("DELETE", performedByUserId, new { RoleId = roleId, RoleName = role.Name });

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error deleting role: {ex.Message}");
        }
    }

    public async Task<Result<PaginatedList<RoleDto>>> GetRolesAsync(int page, int size)
    {
        try
        {
            if (page < 1) page = 1;
            if (size < 1) size = 10;

            var totalCount = await _context.Roles.CountAsync();
            var roles = await _context.Roles
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            var roleDtos = _mapper.Map<List<RoleDto>>(roles);

            return Result<PaginatedList<RoleDto>>.Success(
                new PaginatedList<RoleDto>(roleDtos, totalCount, page, size));
        }
        catch (Exception ex)
        {
            return Result<PaginatedList<RoleDto>>.Failure($"Error retrieving roles: {ex.Message}");
        }
    }

    public async Task<Result<RoleDto>> GetRoleByIdAsync(string roleId)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Result<RoleDto>.Failure("Role not found");

                var roleDto = _mapper.Map<RoleDto>(role);
                return Result<RoleDto>.Success(roleDto);
        }
        catch (Exception ex)
        {
            return Result<RoleDto>.Failure($"Error retrieving role: {ex.Message}");
        }
    }

    public async Task<Result<bool>> SetRoleEnabledAsync(string performedByUserId, string roleId, bool enabled)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Result<bool>.Failure("Role not found");

            var performedByuser = await _userManager.FindByIdAsync(performedByUserId);
            if (performedByuser == null)
                return Result<bool>.Failure("El usuario no existe.");  

            role.IsEnabled = enabled;
            role.UserUpdate = performedByUserId;
            role.DateUpdate = DateTime.UtcNow;

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
                return Result<bool>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

            await RecordRoleAuditAsync(enabled ? "ENABLE" : "DISABLE", performedByUserId, new { RoleId = roleId, RoleName = role.Name, PerformedByName = performedByuser.UserName });

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error updating role enabled flag: {ex.Message}");
        }
    }

    #endregion

    #region User-Role Assignment

    public async Task<Result<bool>> AddRoleToUserAsync(string performedByUserId, string userId, string roleName)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<bool>.Failure("User not found");

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
                return Result<bool>.Failure($"Role '{roleName}' does not exist");

            var userAlreadyHasRole = await _userManager.IsInRoleAsync(user, roleName);
            if (userAlreadyHasRole)
                return Result<bool>.Failure($"User already has role '{roleName}'");

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
                return Result<bool>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

            await RecordRoleAuditAsync("ADD_ROLE_TO_USER", performedByUserId, new { UserId = userId, RoleName = roleName });

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error adding role to user: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RemoveRoleFromUserAsync(string performedByUserId, string userId, string roleName)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<bool>.Failure("User not found");

            var userHasRole = await _userManager.IsInRoleAsync(user, roleName);
            if (!userHasRole)
                return Result<bool>.Failure($"User does not have role '{roleName}'");

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
                return Result<bool>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

            await RecordRoleAuditAsync("REMOVE_ROLE_FROM_USER", performedByUserId, new { UserId = userId, RoleName = roleName });

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error removing role from user: {ex.Message}");
        }
    }

    public async Task<Result<List<RoleDto>>> GetUserRolesAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<List<RoleDto>>.Failure("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            
            var roleEntities = await _context.Roles
                    .Where(r => roles.Contains(r.Name!))
                .ToListAsync();

                var roleDtos = _mapper.Map<List<RoleDto>>(roleEntities);
 
            return Result<List<RoleDto>>.Success(roleDtos);
        }
        catch (Exception ex)
        {
            return Result<List<RoleDto>>.Failure($"Error retrieving user roles: {ex.Message}");
        }
    }

    #endregion

    #region Role Claims

    public async Task<Result<bool>> AddClaimToRoleAsync(string performedByUserId, string roleId, string claimType, string claimValue)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Result<bool>.Failure("Role not found");

            var claimExists = await _roleManager.GetClaimsAsync(role);
            if (claimExists.Any(c => c.Type == claimType && c.Value == claimValue))
                return Result<bool>.Failure($"Claim '{claimType}:{claimValue}' already exists for this role");

            var claim = new Claim(claimType, claimValue);
            var result = await _roleManager.AddClaimAsync(role, claim);
            if (!result.Succeeded)
                return Result<bool>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

            await RecordRoleAuditAsync("ADD_CLAIM", performedByUserId, new { RoleId = roleId, ClaimType = claimType, ClaimValue = claimValue });

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error adding claim to role: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RemoveClaimFromRoleAsync(string performedByUserId, string roleId, string claimType, string claimValue)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Result<bool>.Failure("Role not found");

            var claim = new Claim(claimType, claimValue);
            var result = await _roleManager.RemoveClaimAsync(role, claim);
            if (!result.Succeeded)
                return Result<bool>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

            await RecordRoleAuditAsync("REMOVE_CLAIM", performedByUserId, new { RoleId = roleId, ClaimType = claimType, ClaimValue = claimValue });

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error removing claim from role: {ex.Message}");
        }
    }

    public async Task<Result<List<RoleClaimDto>>> GetRoleClaimsAsync(string roleId)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Result<List<RoleClaimDto>>.Failure("Role not found");

            var claims = await _roleManager.GetClaimsAsync(role);
            var claimDtos = claims.Select(c => new RoleClaimDto
            {
                Type = c.Type,
                Value = c.Value
            }).ToList();

            return Result<List<RoleClaimDto>>.Success(claimDtos);
        }
        catch (Exception ex)
        {
            return Result<List<RoleClaimDto>>.Failure($"Error retrieving role claims: {ex.Message}");
        }
    }

    #endregion

    #region Menu Integration & Access

    public async Task<Result<bool>> UserHasAccessToMenuAsync(string userId, Guid menuId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<bool>.Failure("User not found");

            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Any())
                return Result<bool>.Success(false);

            var roleEntities = await _context.Roles
                 .Where(r => userRoles.Contains(r.Name!))
                .ToListAsync();

            var menu = await _context.Menus.FirstOrDefaultAsync(m => m.Id == menuId);
            if (menu == null)
                return Result<bool>.Failure("Menu not found");

            // If menu doesn't require a specific claim, grant access to all users with roles
            if (string.IsNullOrEmpty(menu.RequiredClaimType))
                return Result<bool>.Success(true);

            // Check if any role has the required claim with sufficient value
            foreach (var role in roleEntities)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                var hasClaim = claims.Any(c => 
                    c.Type == menu.RequiredClaimType && 
                    int.TryParse(c.Value, out var claimValue) &&
                    claimValue >= menu.RequiredClaimMinValue);

                if (hasClaim)
                    return Result<bool>.Success(true);
            }

            return Result<bool>.Success(false);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error checking menu access: {ex.Message}");
        }
    }

    public async Task<Result<List<MenuDto>>> GetAllowedMenusAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<List<MenuDto>>.Failure("User not found");

            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Any())
                return Result<List<MenuDto>>.Success(new List<MenuDto>());

            var roleEntities = await _context.Roles
                 .Where(r => userRoles.Contains(r.Name!))
                .ToListAsync();

            var userClaims = new Dictionary<string, int>();
            foreach (var role in roleEntities)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in claims)
                {
                    if (int.TryParse(claim.Value, out var claimValue))
                    {
                        var key = claim.Type;
                        if (!userClaims.ContainsKey(key) || userClaims[key] < claimValue)
                            userClaims[key] = claimValue;
                    }
                }
            }

            var menus = await _context.Menus.ToListAsync();
            var allowedMenus = new List<MenuDto>();

            foreach (var menu in menus)
            {
                bool hasAccess = false;

                if (string.IsNullOrEmpty(menu.RequiredClaimType))
                {
                    // Menu accessible to all users with roles
                    hasAccess = true;
                }
                else if (userClaims.TryGetValue(menu.RequiredClaimType, out var claimValue) &&
                         claimValue >= menu.RequiredClaimMinValue)
                {
                    // User has required claim with sufficient value
                    hasAccess = true;
                }

                if (hasAccess)
                {
                    allowedMenus.Add(new MenuDto
                    {
                        Id = menu.Id,
                        MenuLabel = menu.MenuLabel,
                        RequiredClaimType = menu.RequiredClaimType,
                        RequiredClaimMinValue = menu.RequiredClaimMinValue,
                        SystemId = menu.SystemId,
                        Level = menu.Level,
                        ParentId = menu.ParentId,
                        OrderIndex = menu.OrderIndex
                    });
                }
            }

            return Result<List<MenuDto>>.Success(allowedMenus);
        }
        catch (Exception ex)
        {
            return Result<List<MenuDto>>.Failure($"Error retrieving allowed menus: {ex.Message}");
        }
    }

    public async Task<Result<List<MenuRoleRwxResponseDto>>> GetRoleMenusAsync(string roleId)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Result<List<MenuRoleRwxResponseDto>>.Failure("Role not found");

            var systemRegistry = await _context.SystemRegistries
                .AsNoTracking() 
                .FirstOrDefaultAsync(s => s.Id == role.SystemId);    
            if (systemRegistry == null)
                return Result<List<MenuRoleRwxResponseDto>>.Failure("System registry not found for the role");

            var claimType = $"Access:{systemRegistry.SystemCode}";

            var claims = await _roleManager.GetClaimsAsync(role);
            var menuClaims = claims.FirstOrDefault(c => c.Type == claimType);

            if (menuClaims == null)
                return Result<List<MenuRoleRwxResponseDto>>.Success(new List<MenuRoleRwxResponseDto>());

            // AHORA haz la consulta a la base de datos
            var menusByRole = await _context.Menus
                .AsNoTracking() 
                .Where(m => m.SystemId == systemRegistry.Id)
                .ToListAsync();            

            var menusDtosWithBitPosition = _mapper.Map<List<MenuRoleBitPositionDto>>(menusByRole.Where(m => m.BitPosition != null && !string.IsNullOrEmpty(m.Module)).ToList());

            // DECODIFICA PRIMERO antes de hacer otra consulta
            var decodedPermissions = await _roleClaimEncoderService.DecodeAsync(menusDtosWithBitPosition, menuClaims.Value);            

            var menusDtoByRole = _mapper.Map<List<MenuRoleRwxResponseDto>>(menusByRole);
            
            // Merge decoded permissions into full menu list
            foreach (var menu in menusDtoByRole)
            {
                var decodedMenu = decodedPermissions.FirstOrDefault(m => m.Id == menu.Id);
                if (decodedMenu != null)
                {
                    menu.RwxValue = decodedMenu.RwxValue;
                }
            }

            return Result<List<MenuRoleRwxResponseDto>>.Success(menusDtoByRole);
        }
        catch (Exception ex)
        {
            return Result<List<MenuRoleRwxResponseDto>>.Failure($"Error retrieving role menus: {ex.Message}");
        }
    }

    public async Task<Result<bool>> SetRoleMenusAsync(string performedByUserId, string roleId, List<MenuRoleRwxRequestDto> menus)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return Result<bool>.Failure("Role not found");

            var systemRegistry = await _context.SystemRegistries
                .AsNoTracking() 
                .FirstOrDefaultAsync(s => s.Id == role.SystemId);    

            if (systemRegistry == null)
                return Result<bool>.Failure("System registry not found for the role");

            var claimType = $"Access:{systemRegistry.SystemCode}";

            // ENCODE los permisos primero
            var menusDtos = _mapper.Map<List<MenuRoleRwxDto>>(menus);

            // 1. EXTRAER los IDs de los menús que queremos buscar
            var menuIds = menus.Select(m => m.Id).ToList();

            // obtener bit positions desde la base de datos
            var menusFromDb = await _context.Menus
                .AsNoTracking() 
                .Where(m => m.SystemId == systemRegistry.Id && menuIds.Contains(m.Id)) 
                .ToListAsync();
            
            foreach (var menuDto in menusDtos)
            {
                var menuInDb = menusFromDb.FirstOrDefault(m => m.Id == menuDto.Id);
                if (menuInDb != null)
                {
                    menuDto.BitPosition = menuInDb.BitPosition;
                }
            }

            var encodedValue = await _roleClaimEncoderService.EncodeAsync(menusDtos.Where(m => m.RwxValue.HasValue).ToList());

            // Ahora actualiza o crea el claim
            var existingClaims = await _roleManager.GetClaimsAsync(role);
            var menuClaim = existingClaims.FirstOrDefault(c => c.Type == claimType);

            IdentityResult result;
            if (menuClaim != null)
            {
                // Remove old claim and add new one
                var removeResult = await _roleManager.RemoveClaimAsync(role, menuClaim);
                if (!removeResult.Succeeded)
                    return Result<bool>.Failure(string.Join(", ", removeResult.Errors.Select(e => e.Description)));

                var newClaim = new Claim(claimType, encodedValue.ToString());
                result = await _roleManager.AddClaimAsync(role, newClaim);
            }
            else
            {
                // Add new claim
                var newClaim = new Claim(claimType, encodedValue.ToString());
                result = await _roleManager.AddClaimAsync(role, newClaim);
            }

            if (!result.Succeeded)
                return Result<bool>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

            await RecordRoleAuditAsync("SET_ROLE_MENUS", performedByUserId, new { RoleId = roleId, EncodedValue = encodedValue.ToString() });

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error setting role menus: {ex.Message}");
        }
    }

    #endregion

    #region Audit

    public async Task<Result<bool>> RecordRoleAuditAsync(string action, string performedByUserId, object details)
    {
        try
        {
            var log = new AuthAuditLog
            {
                UserId = performedByUserId,
                EventType = $"ROLE_{action}",
                EventDate = DateTime.UtcNow,
                Details = details != null
                    ? JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(details))
                    : null
            };

            _context.AuthAuditLogs.Add(log);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error recording audit: {ex.Message}");
        }
    }

    #endregion
}
