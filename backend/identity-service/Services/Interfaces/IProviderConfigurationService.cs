using identity_service.Dtos;
using identity_service.Dtos.ProviderConfiguration;

namespace identity_service.Services.Interfaces;

public interface IProviderConfigurationService
{
    /// <summary>
    /// Obtiene la configuración LDAP habilitada desde la base de datos.
    /// Si no existe, retorna la configuración desde appsettings.json como fallback.
    /// </summary>
    Task<Result<(bool Enabled, string Server, int Port, string? AdminDn, string? AdminPassword, LdapSettingsDto Settings)?>> GetLdapConfigurationAsync();
    
    /// <summary>
    /// Obtiene todas las configuraciones de providers
    /// </summary>
    Task<Result<List<ProviderConfigurationResponseDto>>> GetAllAsync();
    
    /// <summary>
    /// Obtiene una configuración por ID
    /// </summary>
    Task<Result<ProviderConfigurationResponseDto>> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Crea una nueva configuración de provider
    /// </summary>
    Task<Result<ProviderConfigurationResponseDto>> CreateAsync(ProviderConfigurationCreateDto dto, string userId);
    
    /// <summary>
    /// Actualiza una configuración existente
    /// </summary>
    Task<Result<ProviderConfigurationResponseDto>> UpdateAsync(Guid id, ProviderConfigurationUpdateDto dto, string userId);
    
    /// <summary>
    /// Elimina una configuración
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);
}
