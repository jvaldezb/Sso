using System;
using identity_service.Data;
using identity_service.Models;
using identity_service.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace identity_service.Repositories;

public class ProviderConfigurationRepository : BaseRepository<ProviderConfiguration>, IProviderConfigurationRepository
{
    private readonly AppDbContext _dbContext;

    public ProviderConfigurationRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProviderConfiguration?> GetByTypeAsync(string providerType)
    {
        return await _dbContext.ProviderConfigurations
            .Where(pc => pc.ProviderType == providerType && pc.Enabled)
            .OrderByDescending(pc => pc.LastModified)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ProviderConfiguration>> GetByTypeListAsync(string providerType)
    {
        return await _dbContext.ProviderConfigurations
            .Where(pc => pc.ProviderType == providerType)
            .OrderByDescending(pc => pc.LastModified)
            .ToListAsync();
    }

    public async Task<int> DeleteByIdAsync(Guid id)
    {
        var entity = await _dbContext.ProviderConfigurations.FindAsync(id);
        if (entity == null)
            return 0;

        _dbContext.ProviderConfigurations.Remove(entity);
        return await _dbContext.SaveChangesAsync();
    }
}
