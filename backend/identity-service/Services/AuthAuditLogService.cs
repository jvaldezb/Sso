using System;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using identity_service.Dtos;
using identity_service.Dtos.AuthAuditLog;
using identity_service.Models;
using identity_service.Repositories.Interfaces;
using identity_service.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace identity_service.Services;

public class AuthAuditLogService : IAuthAuditLogService
{
    private readonly IAuthAuditLogRepository _authAuditLogRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<AuthAuditLogForCreateDto> _createValidator;

    public AuthAuditLogService(IAuthAuditLogRepository authAuditLogRepository, IMapper mapper, IValidator<AuthAuditLogForCreateDto> createValidator)
    {
        _authAuditLogRepository = authAuditLogRepository;
        _mapper = mapper;
        _createValidator = createValidator;
    }

    public async Task<Result<AuthAuditLogDto>> CreateAsync(AuthAuditLogForCreateDto authAuditLogForCreateDto)
    {
        var validationResult = await _createValidator.ValidateAsync(authAuditLogForCreateDto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors);
            return Result<AuthAuditLogDto>.Failure($"Validation failed: {errors}");
        }

        var authAuditLog = _mapper.Map<AuthAuditLog>(authAuditLogForCreateDto);
        await _authAuditLogRepository.AddAsync(authAuditLog);

        return Result<AuthAuditLogDto>.Success(_mapper.Map<AuthAuditLogDto>(authAuditLog));
    }

    public async Task<Result<AuthAuditLogDto>> DeleteAsync(Guid id)
    {
        var authAuditLog = await _authAuditLogRepository.GetByIdAsync(id);
        if (authAuditLog == null)
        {
            return Result<AuthAuditLogDto>.Failure("AuthAuditLog not found.");
        }

        await _authAuditLogRepository.DeleteAsync(authAuditLog);
        return Result<AuthAuditLogDto>.Success(_mapper.Map<AuthAuditLogDto>(authAuditLog));
    }

    public async Task<PaginatedList<AuthAuditLogDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _authAuditLogRepository.Query()
        .OrderByDescending(x => x.Id);

        var count = await query.CountAsync();

        var items = await query
            .ProjectTo<AuthAuditLogDto>(_mapper.ConfigurationProvider)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedList<AuthAuditLogDto>(items, count, pageNumber, pageSize);       
    }

    public async Task<Result<AuthAuditLogDto>> GetByIdAsync(Guid id)
    {
        var authAuditLog = await _authAuditLogRepository.GetByIdAsync(id);
        if (authAuditLog == null)
        {
            return Result<AuthAuditLogDto>.Failure("AuthAuditLog not found.");
        }
        return Result<AuthAuditLogDto>.Success(_mapper.Map<AuthAuditLogDto>(authAuditLog));
    }
}
