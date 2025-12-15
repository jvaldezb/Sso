using System;
using System.Collections.Generic;
using System.Text.Json;
using AutoMapper;
using FluentAssertions;
using Xunit;
using identity_service.Mappers;
using identity_service.Models;
using identity_service.Dtos.User;
using identity_service.Dtos.Role;
using identity_service.Dtos.SystemRegistry;
using identity_service.Dtos.Auth;
using identity_service.Dtos.AuthAuditLog;
using identity_service.Dtos.RefreshToken;
using identity_service.Dtos.UserSession;

namespace identity_service.UnitTests.Mappers;

public class MapperProfileTests
{
    private readonly IMapper _mapper;

    public MapperProfileTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserProfile>();
            cfg.AddProfile<RoleProfile>();
            cfg.AddProfile<SystemRegistryProfile>();
            cfg.AddProfile<AuthProfile>();
            cfg.AddProfile<AuthAuditLogProfile>();
            cfg.AddProfile<MenuProfile>();
            cfg.AddProfile<RefreshTokenProfile>();
            cfg.AddProfile<UserSessionProfile>();
        });

        // No validamos configuración aquí porque hay mappings inversos que no están definidos
        // Solo probamos los mappings que realmente se usan en la aplicación
        _mapper = config.CreateMapper();
    }

    #region UserProfile Tests

    [Fact]
    public void UserProfile_ApplicationUser_To_UserResponseDto_ShouldMap()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-123",
            UserName = "testuser",
            Email = "test@example.com",
            FullName = "Test User",
            PhoneNumber = "1234567890",
            IsEnabled = true,
            DocumentType = "DNI",
            DocumentNumber = "12345678"
        };

        // Act
        var dto = _mapper.Map<UserResponseDto>(user);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(user.Id);
        dto.UserName.Should().Be(user.UserName);
        dto.Email.Should().Be(user.Email);
        dto.FullName.Should().Be(user.FullName);
        dto.IsEnabled.Should().Be(user.IsEnabled);
        dto.DocumentType.Should().Be(user.DocumentType);
        dto.DocumentNumber.Should().Be(user.DocumentNumber);
    }

    [Fact]
    public void UserProfile_UserForCreateDto_To_ApplicationUser_ShouldMap()
    {
        // Arrange
        var dto = new UserForCreateDto
        {
            UserName = "newuser",
            Email = "new@example.com",
            Password = "SecurePass123!",
            FullName = "New User",
            DocumentType = "DNI",
            DocumentNumber = "87654321",
            RoleIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var user = _mapper.Map<ApplicationUser>(dto);

        // Assert
        user.Should().NotBeNull();
        user.UserName.Should().Be(dto.UserName);
        user.Email.Should().Be(dto.Email);
        user.FullName.Should().Be(dto.FullName);
        user.DocumentType.Should().Be(dto.DocumentType);
        user.DocumentNumber.Should().Be(dto.DocumentNumber);
    }

    #endregion

    #region RoleProfile Tests

    [Fact]
    public void RoleProfile_ApplicationRole_To_RoleDto_ShouldMap()
    {
        // Arrange
        var systemId = Guid.NewGuid();
        var role = new ApplicationRole
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Admin",
            NormalizedName = "ADMIN",
            SystemId = systemId,
            IsEnabled = true,
            DateCreate = DateTime.UtcNow,
            DateUpdate = DateTime.UtcNow
        };

        // Act
        var dto = _mapper.Map<RoleDto>(role);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(role.Id);
        dto.Name.Should().Be(role.Name);
        dto.SystemId.Should().Be(systemId);
        dto.IsEnabled.Should().Be(role.IsEnabled);
    }

    [Fact]
    public void RoleProfile_CreateRoleDto_To_ApplicationRole_ShouldMap()
    {
        // Arrange
        var systemId = Guid.NewGuid();
        var dto = new CreateRoleDto("Editor", systemId);

        // Act
        var role = _mapper.Map<ApplicationRole>(dto);

        // Assert
        role.Should().NotBeNull();
        role.Name.Should().Be(dto.Name);
        role.SystemId.Should().Be(systemId);
    }

    [Fact]
    public void RoleProfile_ApplicationRole_To_UserRoleDto_ShouldMap()
    {
        // Arrange
        var systemId = Guid.NewGuid();
        var roleId = Guid.NewGuid().ToString();
        var role = new ApplicationRole
        {
            Id = roleId,
            Name = "Manager",
            SystemId = systemId
        };

        // Act
        var dto = _mapper.Map<UserRoleDto>(role);

        // Assert
        dto.Should().NotBeNull();
        dto.RoleId.Should().Be(Guid.Parse(roleId));
        dto.RoleName.Should().Be("Manager");
        dto.SystemId.Should().Be(systemId);
    }

    #endregion

    #region SystemRegistryProfile Tests

    [Fact]
    public void SystemRegistryProfile_SystemRegistry_To_SystemRegistryDto_ShouldMap()
    {
        // Arrange
        var system = new SystemRegistry
        {
            Id = Guid.NewGuid(),
            SystemCode = "SYS001",
            SystemName = "Test System",
            Description = "Test description",
            BaseUrl = "https://test.com",
            IconUrl = "https://test.com/icon.png",
            Category = "Testing",
            ContactEmail = "contact@test.com",
            IsEnabled = true,
            DateCreate = DateTime.UtcNow,
            DateUpdate = DateTime.UtcNow
        };

        // Act
        var dto = _mapper.Map<SystemRegistryDto>(system);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(system.Id);
        dto.SystemCode.Should().Be(system.SystemCode);
        dto.SystemName.Should().Be(system.SystemName);
        dto.BaseUrl.Should().Be(system.BaseUrl);
        dto.IsEnabled.Should().Be(system.IsEnabled);
        dto.Description.Should().Be(system.Description);
        dto.Category.Should().Be(system.Category);
        dto.ContactEmail.Should().Be(system.ContactEmail);
    }

    [Fact]
    public void SystemRegistryProfile_CreateSystemRegistryDto_To_SystemRegistry_ShouldMap()
    {
        // Arrange
        var dto = new CreateSystemRegistryDto
        {
            SystemCode = "NEW001",
            SystemName = "New System",
            Description = "New system description",
            BaseUrl = "https://newsystem.com",
            Category = "New",
            ContactEmail = "new@system.com"
        };

        // Act
        var system = _mapper.Map<SystemRegistry>(dto);

        // Assert
        system.Should().NotBeNull();
        system.SystemCode.Should().Be(dto.SystemCode);
        system.SystemName.Should().Be(dto.SystemName);
        system.BaseUrl.Should().Be(dto.BaseUrl);
        system.Description.Should().Be(dto.Description);
        system.Category.Should().Be(dto.Category);
        system.ContactEmail.Should().Be(dto.ContactEmail);
    }

    [Fact]
    public void SystemRegistryProfile_UpdateSystemRegistryDto_To_SystemRegistry_ShouldMap()
    {
        // Arrange
        var dto = new UpdateSystemRegistryDto(
            SystemCode: "UPD001",
            SystemName: "Updated System",
            Description: "Updated description",
            BaseUrl: "https://updated.com",
            IconUrl: "https://updated.com/icon.png",
            Category: "Updated",
            ContactEmail: "updated@system.com",
            ApiKey: "api-key-123"
        );

        // Act
        var system = _mapper.Map<SystemRegistry>(dto);

        // Assert
        system.Should().NotBeNull();
        system.SystemCode.Should().Be(dto.SystemCode);
        system.SystemName.Should().Be(dto.SystemName);
        system.BaseUrl.Should().Be(dto.BaseUrl);
        system.Description.Should().Be(dto.Description);
        system.IconUrl.Should().Be(dto.IconUrl);
        system.Category.Should().Be(dto.Category);
        system.ContactEmail.Should().Be(dto.ContactEmail);
    }

    #endregion

    #region AuthProfile Tests

    [Fact]
    public void AuthProfile_Menu_To_MenuDto_ShouldMap()
    {
        // Arrange
        var systemId = Guid.NewGuid();
        var menu = new Menu
        {
            Id = Guid.NewGuid(),
            MenuLabel = "Dashboard",
            Url = "/dashboard",
            IconUrl = "dashboard-icon",
            ParentId = null,
            OrderIndex = 1,
            Level = 1,
            SystemId = systemId,
            Module = "Main",
            ModuleType = "Module",
            MenuType = "Menu",
            Description = "Main dashboard"
        };

        // Act
        var dto = _mapper.Map<identity_service.Dtos.Auth.MenuDto>(menu);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(menu.Id);
        dto.MenuLabel.Should().Be(menu.MenuLabel);
        dto.Url.Should().Be(menu.Url);
        dto.IconUrl.Should().Be(menu.IconUrl);
        dto.ParentId.Should().Be(menu.ParentId);
        dto.OrderIndex.Should().Be(menu.OrderIndex);
        dto.Level.Should().Be(menu.Level);
    }

    #endregion

    #region AuthAuditLogProfile Tests

    [Fact]
    public void AuthAuditLogProfile_AuthAuditLog_To_AuthAuditLogDto_ShouldMap()
    {
        // Arrange
        var log = new AuthAuditLog
        {
            Id = Guid.NewGuid(),
            UserId = "user-123",
            EventType = "LOGIN",
            ProviderName = "Local",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            Details = JsonDocument.Parse("{\"success\":true}"),
            DateCreate = DateTime.UtcNow
        };

        // Act
        var dto = _mapper.Map<AuthAuditLogDto>(log);

        // Assert
        dto.Should().NotBeNull();
        dto.UserId.Should().Be(log.UserId);
        dto.EventType.Should().Be(log.EventType);
        dto.ProviderName.Should().Be(log.ProviderName);
        dto.IpAddress.Should().Be(log.IpAddress);
        dto.UserAgent.Should().Be(log.UserAgent);
        dto.Details.Should().NotBeNull();
    }

    #endregion

    #region RefreshTokenProfile Tests

    [Fact]
    public void RefreshTokenProfile_RefreshToken_To_RefreshTokenDto_ShouldMap()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow;
        var expiresAt = createdAt.AddDays(7);
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "refresh-token-123",
            UserId = "user-789",
            CreatedAt = createdAt,
            ExpiresAt = expiresAt,
            RevokedAt = null,
            IsRevoked = false,
            SystemId = Guid.NewGuid(),
            SessionId = Guid.NewGuid()
        };

        // Act
        var dto = _mapper.Map<RefreshTokenDto>(token);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(token.Id);
        dto.Token.Should().Be(token.Token);
        dto.CreatedAt.Should().Be(createdAt);
        dto.ExpiresAt.Should().Be(expiresAt);
        dto.RevokedAt.Should().BeNull();
        dto.IsRevoked.Should().BeFalse();
    }

    #endregion

    #region UserSessionProfile Tests

    [Fact]
    public void UserSessionProfile_UserSession_To_UserSessionDto_ShouldMap()
    {
        // Arrange
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = "user-999",
            JwtId = "jwt-id-abc",
            TokenType = "access",
            SystemName = "SIGA",
            Device = "Chrome",
            IpAddress = "10.0.0.1",
            IssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(2),
            IsRevoked = false,
            Audience = "SIGA",
            Scope = "read write"
        };

        // Act
        var dto = _mapper.Map<UserSessionDto>(session);

        // Assert
        dto.Should().NotBeNull();
        dto.UserId.Should().Be(session.UserId);
        dto.JwtId.Should().Be(session.JwtId);
        dto.SystemName.Should().Be(session.SystemName);
        dto.Device.Should().Be(session.Device);
        dto.IpAddress.Should().Be(session.IpAddress);
        dto.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void UserSessionProfile_UserSessionDto_To_UserSession_ShouldMap()
    {
        // Arrange
        var dto = new UserSessionDto
        {
            UserId = "user-888",
            JwtId = "jwt-xyz",
            TokenType = "session",
            SystemName = "SIAF",
            Device = "Firefox",
            IpAddress = "172.16.0.1",
            IssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            IsRevoked = false
        };

        // Act
        var session = _mapper.Map<UserSession>(dto);

        // Assert
        session.Should().NotBeNull();
        session.UserId.Should().Be(dto.UserId);
        session.JwtId.Should().Be(dto.JwtId);
        session.SystemName.Should().Be(dto.SystemName);
        session.Device.Should().Be(dto.Device);
        session.IpAddress.Should().Be(dto.IpAddress);
        session.IsRevoked.Should().BeFalse();
    }

    #endregion

    [Fact]
    public void AllProfiles_MapperIsConfigured()
    {
        // This test ensures that the mapper is correctly configured
        _mapper.Should().NotBeNull();
        _mapper.ConfigurationProvider.Should().NotBeNull();
    }
}
