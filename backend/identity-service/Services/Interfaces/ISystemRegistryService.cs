using System;
using identity_service.Dtos;
using identity_service.Dtos.SystemRegistry;

namespace identity_service.Services.Interfaces;

public interface ISystemRegistryService
{
    Task<Result<SystemRegistryDto>> CreateAsync(string performedByUserId, CreateSystemRegistryDto dto);
    Task<Result<SystemRegistryDto>> UpdateAsync(string performedByUserId, Guid id, UpdateSystemRegistryDto dto);
    Task<Result<bool>> DeleteAsync(string performedByUserId, Guid id);
    Task<Result<PaginatedList<SystemRegistryDto>>> GetAllAsync(int page, int size);
    Task<Result<SystemRegistryDto>> GetByIdAsync(Guid id);
    Task<Result<SystemRegistryDto>> GetByCodeAsync(string systemCode);
    Task<PaginatedList<SystemRegistryDto>> GetByCodesAsync(IEnumerable<string> systemCodes);
    Task<Result<bool>> SetEnabledAsync(string performedByUserId, Guid id, bool enabled);
}
