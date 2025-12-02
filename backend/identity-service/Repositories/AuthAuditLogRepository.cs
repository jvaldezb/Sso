using System;
using identity_service.Data;
using identity_service.Models;
using identity_service.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace identity_service.Repositories;

public class AuthAuditLogRepository : BaseRepository<AuthAuditLog>, IAuthAuditLogRepository
{
    public AuthAuditLogRepository(AppDbContext dbContext) : base(dbContext)
    {    
    }
}
