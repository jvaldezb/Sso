using System;
using identity_service.Dtos;
using identity_service.Dtos.AuthAuditLog;

namespace identity_service.Services.Interfaces;

public interface IAuthAuditLogService
{
    Task<PaginatedList<AuthAuditLogDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<Result<AuthAuditLogDto>> GetByIdAsync(Guid id);
    Task<Result<AuthAuditLogDto>> CreateAsync(AuthAuditLogForCreateDto authAuditLogForCreateDto);    
    Task<Result<AuthAuditLogDto>> DeleteAsync(Guid id);
}
