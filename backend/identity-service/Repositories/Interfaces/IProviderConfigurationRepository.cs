using System;
using identity_service.Models;

namespace identity_service.Repositories.Interfaces;

public interface IProviderConfigurationRepository: IBaseRepository<ProviderConfiguration>
{
    Task<ProviderConfiguration?> GetByTypeAsync(string providerType);
    Task<List<ProviderConfiguration>> GetByTypeListAsync(string providerType);
    Task<int> DeleteByIdAsync(Guid id);
}
