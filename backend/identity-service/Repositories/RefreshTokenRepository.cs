using System;
using identity_service.Data;
using identity_service.Models;
using identity_service.Repositories.Interfaces;

namespace identity_service.Repositories;

public class RefreshTokenRepository: BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
