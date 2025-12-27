using System.Text.Json;
using identity_service.Dtos;
using identity_service.Dtos.ProviderConfiguration;
using identity_service.Models;
using identity_service.Repositories.Interfaces;
using identity_service.Services.Interfaces;

namespace identity_service.Services;

public class ProviderConfigurationService : IProviderConfigurationService
{
    private readonly IProviderConfigurationRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProviderConfigurationService> _logger;

    public ProviderConfigurationService(
        IProviderConfigurationRepository repository,
        IConfiguration configuration,
        ILogger<ProviderConfigurationService> logger)
    {
        _repository = repository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<(bool Enabled, string Server, int Port, string? AdminDn, string? AdminPassword, LdapSettingsDto Settings)?>> GetLdapConfigurationAsync()
    {
        try
        {
            // 1. Intentar obtener desde la base de datos
            var ldapProvider = await _repository.GetByTypeAsync("LDAP");

            if (ldapProvider != null && ldapProvider.Enabled)
            {
                // Parsear el EndpointUrl: ldap://localhost:389
                var uri = new Uri(ldapProvider.EndpointUrl!);
                var server = uri.Host;
                var port = uri.Port;

                // Deserializar Scopes como JSON a LdapSettingsDto
                LdapSettingsDto? settings = null;
                if (!string.IsNullOrWhiteSpace(ldapProvider.Scopes))
                {
                    settings = JsonSerializer.Deserialize<LdapSettingsDto>(ldapProvider.Scopes);
                }

                if (settings == null)
                {
                    _logger.LogWarning("LDAP provider found but Scopes is null or invalid JSON");
                    return Result<(bool Enabled, string Server, int Port, string? AdminDn, string? AdminPassword, LdapSettingsDto Settings)?>.Success(null);
                }

                var config = (
                    Enabled: ldapProvider.Enabled,
                    Server: server,
                    Port: port,
                    AdminDn: ldapProvider.ClientId,
                    AdminPassword: ldapProvider.ClientSecret,
                    Settings: settings
                );

                return Result<(bool Enabled, string Server, int Port, string? AdminDn, string? AdminPassword, LdapSettingsDto Settings)?>.Success(config);
            }

            // 2. Fallback: leer desde appsettings.json
            var ldapEnabled = bool.Parse(_configuration["LdapSettings:Enabled"] ?? "false");
            if (!ldapEnabled)
            {
                return Result<(bool Enabled, string Server, int Port, string? AdminDn, string? AdminPassword, LdapSettingsDto Settings)?>.Success(null);
            }

            var appsettingsConfig = new LdapSettingsDto
            {
                BaseDn = _configuration["LdapSettings:BaseDn"] ?? "",
                UserSearchBase = _configuration["LdapSettings:UserSearchBase"] ?? "",
                UseSsl = bool.Parse(_configuration["LdapSettings:UseSsl"] ?? "false"),
                UseStartTls = false,
                Timeout = 30
            };

            var fallbackConfig = (
                Enabled: true,
                Server: _configuration["LdapSettings:Server"] ?? "localhost",
                Port: int.Parse(_configuration["LdapSettings:Port"] ?? "389"),
                AdminDn: _configuration["LdapSettings:AdminDn"],
                AdminPassword: _configuration["LdapSettings:AdminPassword"],
                Settings: appsettingsConfig
            );

            return Result<(bool Enabled, string Server, int Port, string? AdminDn, string? AdminPassword, LdapSettingsDto Settings)?>.Success(fallbackConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving LDAP configuration");
            return Result<(bool Enabled, string Server, int Port, string? AdminDn, string? AdminPassword, LdapSettingsDto Settings)?>.Failure("Error retrieving LDAP configuration: " + ex.Message);
        }
    }

    public async Task<Result<List<ProviderConfigurationResponseDto>>> GetAllAsync()
    {
        try
        {
            var providers = await _repository.GetAllAsync();
            var result = providers.Select(MapToResponseDto).ToList();
            return Result<List<ProviderConfigurationResponseDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all provider configurations");
            return Result<List<ProviderConfigurationResponseDto>>.Failure("Error retrieving provider configurations: " + ex.Message);
        }
    }

    public async Task<Result<ProviderConfigurationResponseDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var provider = await _repository.GetByIdAsync(id);
            if (provider == null)
                return Result<ProviderConfigurationResponseDto>.Failure("Provider configuration not found");

            return Result<ProviderConfigurationResponseDto>.Success(MapToResponseDto(provider));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving provider configuration {Id}", id);
            return Result<ProviderConfigurationResponseDto>.Failure("Error retrieving provider configuration: " + ex.Message);
        }
    }

    public async Task<Result<ProviderConfigurationResponseDto>> CreateAsync(ProviderConfigurationCreateDto dto, string userId)
    {
        try
        {
            var entity = new ProviderConfiguration
            {
                ProviderName = dto.ProviderName,
                ProviderType = dto.ProviderType,
                ClientId = dto.ClientId,
                ClientSecret = dto.ClientSecret,
                EndpointUrl = dto.EndpointUrl,
                Scopes = dto.Scopes,
                Enabled = dto.Enabled,
                LastModified = DateTime.UtcNow,
                DateCreate = DateTime.UtcNow,
                UserCreate = userId
            };

            var created = await _repository.AddAsync(entity);
            return Result<ProviderConfigurationResponseDto>.Success(MapToResponseDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating provider configuration");
            return Result<ProviderConfigurationResponseDto>.Failure("Error creating provider configuration: " + ex.Message);
        }
    }

    public async Task<Result<ProviderConfigurationResponseDto>> UpdateAsync(Guid id, ProviderConfigurationUpdateDto dto, string userId)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return Result<ProviderConfigurationResponseDto>.Failure($"ProviderConfiguration with ID {id} not found");

            existing.ProviderName = dto.ProviderName;
            existing.ClientId = dto.ClientId;
            existing.ClientSecret = dto.ClientSecret;
            existing.EndpointUrl = dto.EndpointUrl;
            existing.Scopes = dto.Scopes;
            existing.Enabled = dto.Enabled;
            existing.LastModified = DateTime.UtcNow;
            existing.DateUpdate = DateTime.UtcNow;
            existing.UserUpdate = userId;

            await _repository.UpdateAsync(existing);
            return Result<ProviderConfigurationResponseDto>.Success(MapToResponseDto(existing));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating provider configuration {Id}", id);
            return Result<ProviderConfigurationResponseDto>.Failure("Error updating provider configuration: " + ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var result = await _repository.DeleteByIdAsync(id);
            if (result == 0)
                return Result<bool>.Failure("Provider configuration not found");

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting provider configuration {Id}", id);
            return Result<bool>.Failure("Error deleting provider configuration: " + ex.Message);
        }
    }

    private static ProviderConfigurationResponseDto MapToResponseDto(ProviderConfiguration entity)
    {
        return new ProviderConfigurationResponseDto
        {
            Id = entity.Id,
            ProviderName = entity.ProviderName,
            ProviderType = entity.ProviderType,
            ClientId = entity.ClientId,
            EndpointUrl = entity.EndpointUrl,
            Scopes = entity.Scopes,
            Enabled = entity.Enabled,
            LastModified = entity.LastModified,
            DateCreate = entity.DateCreate,
            UserCreate = entity.UserCreate
        };
    }
}
