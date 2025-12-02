using System;
using identity_service.Data;
using identity_service.Models;
using identity_service.Repositories.Interfaces;

namespace identity_service.Repositories;

public class ProviderConfigurationRepository : BaseRepository<ProviderConfiguration>, IProviderConfigurationRepository
{
    public ProviderConfigurationRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
