using System;
using System.Text.Json;
using AutoMapper;
using FluentValidation;
using identity_service.Data;
using identity_service.Dtos;
using identity_service.Dtos.SystemRegistry;
using identity_service.Models;
using identity_service.Repositories.Interfaces;
using identity_service.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace identity_service.Services;

public class SystemRegistryService : ISystemRegistryService
{
    private readonly ISystemRegistryRepository _repository;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateSystemRegistryDto> _createValidator;
    private readonly IValidator<UpdateSystemRegistryDto> _updateValidator;    
    private readonly UserManager<ApplicationUser> _userManager;

    public SystemRegistryService(
        ISystemRegistryRepository repository,
        AppDbContext context,
        IMapper mapper,
        IValidator<CreateSystemRegistryDto> createValidator,
        IValidator<UpdateSystemRegistryDto> updateValidator,
        UserManager<ApplicationUser> userManager)
    {
        _repository = repository;
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _userManager = userManager;        
    }

    public async Task<Result<SystemRegistryDto>> CreateAsync(string performedByUserId, CreateSystemRegistryDto dto)
    {
        try
        {
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return Result<SystemRegistryDto>.Failure(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

            // Check if system code already exists
            var existingSystem = await _context.SystemRegistries
                .FirstOrDefaultAsync(s => s.SystemCode == dto.SystemCode);
            if (existingSystem != null)
                return Result<SystemRegistryDto>.Failure($"A system with code '{dto.SystemCode}' already exists.");

            var user = await _userManager.FindByIdAsync(performedByUserId);
            if (user == null)
                return Result<SystemRegistryDto>.Failure("El usuario no existe.");

            var system = _mapper.Map<SystemRegistry>(dto);
            system.UserCreate = performedByUserId;
            system.DateCreate = DateTime.UtcNow;   
            

            await _repository.AddAsync(system);

            var systemDto = _mapper.Map<SystemRegistryDto>(system);
            return Result<SystemRegistryDto>.Success(systemDto);
        }
        catch (Exception ex)
        {
            return Result<SystemRegistryDto>.Failure($"Error creating system registry: {ex.Message}");
        }
    }

    public async Task<Result<SystemRegistryDto>> UpdateAsync(string performedByUserId, Guid id, UpdateSystemRegistryDto dto)
    {
        try
        {
            var validation = await _updateValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return Result<SystemRegistryDto>.Failure(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

            var system = await _repository.GetByIdAsync(id);
            if (system == null)
                return Result<SystemRegistryDto>.Failure("System registry not found.");

            // Check if another system has the same code
            var existingSystem = await _context.SystemRegistries
                .FirstOrDefaultAsync(s => s.SystemCode == dto.SystemCode && s.Id != id);
            if (existingSystem != null)
                return Result<SystemRegistryDto>.Failure($"A system with code '{dto.SystemCode}' already exists.");

            var user = await _userManager.FindByIdAsync(performedByUserId);
            if (user == null)
                return Result<SystemRegistryDto>.Failure("El usuario no existe.");    

            _mapper.Map(dto, system);
            system.UserUpdate = performedByUserId;
            system.DateUpdate = DateTime.UtcNow;

            await _repository.UpdateAsync(system);

            var systemDto = _mapper.Map<SystemRegistryDto>(system);
            return Result<SystemRegistryDto>.Success(systemDto);
        }
        catch (Exception ex)
        {
            return Result<SystemRegistryDto>.Failure($"Error updating system registry: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(string performedByUserId, Guid id)
    {
        try
        {
            var system = await _repository.GetByIdAsync(id);
            if (system == null)
                return Result<bool>.Failure("System registry not found.");

            await _repository.DeleteAsync(system);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error deleting system registry: {ex.Message}");
        }
    }

    public async Task<Result<PaginatedList<SystemRegistryDto>>> GetAllAsync(int page, int size)
    {
        try
        {
            if (page < 1) page = 1;
            if (size < 1) size = 10;

            var totalCount = await _context.SystemRegistries.CountAsync();
            var systems = await _context.SystemRegistries
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            var systemDtos = _mapper.Map<List<SystemRegistryDto>>(systems);

            return Result<PaginatedList<SystemRegistryDto>>.Success(
                new PaginatedList<SystemRegistryDto>(systemDtos, totalCount, page, size));
        }
        catch (Exception ex)
        {
            return Result<PaginatedList<SystemRegistryDto>>.Failure($"Error retrieving system registries: {ex.Message}");
        }
    }

    public async Task<Result<SystemRegistryDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var system = await _repository.GetByIdAsync(id);
            if (system == null)
                return Result<SystemRegistryDto>.Failure("System registry not found.");

            var systemDto = _mapper.Map<SystemRegistryDto>(system);
            return Result<SystemRegistryDto>.Success(systemDto);
        }
        catch (Exception ex)
        {
            return Result<SystemRegistryDto>.Failure($"Error retrieving system registry: {ex.Message}");
        }
    }

    public async Task<Result<SystemRegistryDto>> GetByCodeAsync(string systemCode)
    {
        try
        {
            var system = await _context.SystemRegistries
                .FirstOrDefaultAsync(s => s.SystemCode == systemCode);
            
            if (system == null)
                return Result<SystemRegistryDto>.Failure("System registry not found.");

            var systemDto = _mapper.Map<SystemRegistryDto>(system);
            return Result<SystemRegistryDto>.Success(systemDto);
        }
        catch (Exception ex)
        {
            return Result<SystemRegistryDto>.Failure($"Error retrieving system registry: {ex.Message}");
        }
    }

    public async Task<PaginatedList<SystemRegistryDto>> GetByCodesAsync(IEnumerable<string> systemCodes)
    {
        try
        {
            if (systemCodes == null || !systemCodes.Any())
                return new PaginatedList<SystemRegistryDto>(new List<SystemRegistryDto>(), 0, 1, 1);

            var codes = systemCodes.ToList();
            var systems = await _context.SystemRegistries
                .Where(s => codes.Contains(s.SystemCode))
                .ToListAsync();

            var systemDtos = _mapper.Map<List<SystemRegistryDto>>(systems);

            var totalCount = systemDtos.Count;
            var page = 1;
            var size = totalCount > 0 ? totalCount : 1;

            return new PaginatedList<SystemRegistryDto>(systemDtos, totalCount, page, size);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Result<bool>> SetEnabledAsync(string performedByUserId, Guid id, bool enabled)
    {
        try
        {
            var system = await _repository.GetByIdAsync(id);
            if (system == null)
                return Result<bool>.Failure("Sistema no encontrado.");

            var performedByUser = await _userManager.FindByIdAsync(performedByUserId);
            if (performedByUser == null)
                return Result<bool>.Failure("El usuario no existe.");    

            system.IsEnabled = enabled;
            system.UserUpdate = performedByUserId;
            system.DateUpdate = DateTime.UtcNow;

            await _repository.UpdateAsync(system);

            await RecordSystemRegistryAuditAsync(enabled ? "ENABLE" : "DISABLE", performedByUserId, 
                new { 
                    SystemRegistryId = id, 
                    SystemCode = system.SystemCode, 
                    PerformedByName = performedByUser.UserName 
                });

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error updating system registry enabled flag: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateApiKeyAsync(string performedByUserId, Guid id, string apiKey)    
    {        
        try
        {
            // Validate ApiKey
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Length < 32)
                return Result<bool>.Failure("El ApiKey no es válido. Debe tener al menos 32 caracteres.");

            var system = _context.SystemRegistries.FirstOrDefault(s => s.Id == id);
            if (system == null)
                return Result<bool>.Failure("System registry not found.");

            system.ApiKey = apiKey;
            system.UserUpdate = performedByUserId;
            system.DateUpdate = DateTime.UtcNow;

            _context.SystemRegistries.Update(system);
            _context.SaveChanges();

            await RecordSystemRegistryAuditAsync("UPDATE_API_KEY", performedByUserId,
                new
                {
                    SystemRegistryId = id,
                    SystemCode = system.SystemCode,
                    NewApiKey = apiKey
                });

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error updating API key: {ex.Message}");
        }
    }

    public async Task<Result<List<SystemRegistryWithMenusDto>>> GetAllWithMenusAsync()
    {
        try
        {
            var systems = await _context.SystemRegistries
                .Include(s => s.Menus)
                .Where(s => s.IsEnabled)
                .ToListAsync();

            var systemDtos = systems.Select(system => new SystemRegistryWithMenusDto
            {
                Id = system.Id,
                SystemCode = system.SystemCode,
                SystemName = system.SystemName,
                Description = system.Description,
                BaseUrl = system.BaseUrl,
                IconUrl = system.IconUrl,
                IsEnabled = system.IsEnabled,
                Category = system.Category,
                ContactEmail = system.ContactEmail,
                IsCentralAdmin = system.IsCentralAdmin ?? false,
                ApiKey = system.ApiKey,
                LastSync = system.LastSync,
                Menus = system.Menus?.Select(m => new MenuDetailsDto
                {
                    Id = m.Id,
                    ParentId = m.ParentId,
                    SystemId = m.SystemId,
                    MenuLabel = m.MenuLabel,
                    Description = m.Description,
                    Level = m.Level,
                    Module = m.Module,
                    ModuleType = m.ModuleType,
                    MenuType = m.MenuType,
                    IconUrl = m.IconUrl,
                    AccessScope = m.AccessScope,
                    OrderIndex = m.OrderIndex,                    
                    Url = m.Url
                }).OrderBy(m => m.OrderIndex).ToList() ?? new List<MenuDetailsDto>()
            }).ToList();

            return Result<List<SystemRegistryWithMenusDto>>.Success(systemDtos);
        }
        catch (Exception ex)
        {
            return Result<List<SystemRegistryWithMenusDto>>.Failure($"Error retrieving system registries with menus: {ex.Message}");
        }
    }

    #region Audit

    public async Task<Result<bool>> RecordSystemRegistryAuditAsync(string action, string performedByUserId, object details)
    {
        try
        {
            var log = new AuthAuditLog
            {
                UserId = performedByUserId,
                EventType = $"SYSTEM_REGISTRY_{action}",
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
