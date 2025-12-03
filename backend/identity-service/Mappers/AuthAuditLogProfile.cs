using System;
using AutoMapper;
using identity_service.Dtos.AuthAuditLog;
using identity_service.Models;

namespace identity_service.Mappers;

public class AuthAuditLogProfile: Profile
{
    public AuthAuditLogProfile()
    {
        CreateMap<AuthAuditLog, AuthAuditLogDto>().ReverseMap();
        CreateMap<AuthAuditLogForCreateDto, AuthAuditLog>();
    }

}
