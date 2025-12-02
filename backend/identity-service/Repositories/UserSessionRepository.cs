using System;
using identity_service.Data;
using identity_service.Models;
using identity_service.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace identity_service.Repositories;

public class UserSessionRepository : BaseRepository<UserSession>, IUserSessionRepository
{
    public UserSessionRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(string userId)
    {
        return await Query()
            .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.IssuedAt)
            .ToListAsync();
    }
}
