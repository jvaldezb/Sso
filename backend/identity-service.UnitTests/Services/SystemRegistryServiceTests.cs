using System;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using FluentValidation.Results;
using Moq;
using Xunit;
using identity_service.UnitTests.Fixtures;
using identity_service.Services;
using identity_service.Repositories.Interfaces;
using identity_service.Dtos.SystemRegistry;
using identity_service.Models;
using identity_service.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace identity_service.UnitTests.Services;

public class SystemRegistryServiceTests
{
    private readonly ServiceTestFixture _fixture = new ServiceTestFixture();

    [Fact]
    public async Task CreateAsync_WhenValid_CreatesSystem()
    {
        // Arrange
        using var ctx = InMemoryDbFixture.CreateNewContext();

        var repoMock = new Mock<ISystemRegistryRepository>();

        var createValidatorMock = new Mock<FluentValidation.IValidator<CreateSystemRegistryDto>>();
        createValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateSystemRegistryDto>(), default))
            .ReturnsAsync(new ValidationResult());

        var updateValidatorMock = new Mock<FluentValidation.IValidator<UpdateSystemRegistryDto>>();

        var userManagerMock = _fixture.CreateUserManagerMock();
        userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1", UserName = "tester" });

        // make AddAsync simply add to context so GetAll can observe it if needed
        repoMock.Setup(r => r.AddAsync(It.IsAny<SystemRegistry>()))
            .ReturnsAsync((SystemRegistry sr) =>
            {
                ctx.SystemRegistries.Add(sr);
                ctx.SaveChanges();  
                return sr;
            });

        var service = new SystemRegistryService(
            repoMock.Object,
            ctx,
            _fixture.Mapper,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            userManagerMock.Object);

        var dto = new CreateSystemRegistryDto {
            SystemCode = "S001", 
            SystemName = "My System",
            BaseUrl = "https://example",
            Description = "desc",
            IconUrl = null, 
            Category = "cat", 
            ContactEmail = "a@b.com"
        };

        // Act
        var result = await service.CreateAsync("user-1", dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.SystemCode.Should().Be(dto.SystemCode);
        repoMock.Verify(r => r.AddAsync(It.IsAny<SystemRegistry>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenCodeExists_ReturnsFailure()
    {
        // Arrange
        using var ctx = InMemoryDbFixture.CreateNewContext();

        // seed existing system with same code
        ctx.SystemRegistries.Add(new SystemRegistry { Id = Guid.NewGuid(), SystemCode = "S001", SystemName = "Existing", BaseUrl = "https://www.existing.com" });
        await ctx.SaveChangesAsync();

        var repoMock = new Mock<ISystemRegistryRepository>();

        var createValidatorMock = new Mock<FluentValidation.IValidator<CreateSystemRegistryDto>>();
        createValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateSystemRegistryDto>(), default))
            .ReturnsAsync(new ValidationResult());

        var updateValidatorMock = new Mock<FluentValidation.IValidator<UpdateSystemRegistryDto>>();

        var userManagerMock = _fixture.CreateUserManagerMock();
        userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1", UserName = "tester" });

        var service = new SystemRegistryService(
            repoMock.Object,
            ctx,
            _fixture.Mapper,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            userManagerMock.Object);
        
        var dto = new CreateSystemRegistryDto {
            SystemCode = "S001", 
            SystemName = "New",
            BaseUrl = "https://example",
            Description = null,
            IconUrl = null, 
            Category = null, 
            ContactEmail = null
        };

        // Act
        var result = await service.CreateAsync("user-1", dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");
        repoMock.Verify(r => r.AddAsync(It.IsAny<SystemRegistry>()), Times.Never);
    }

    [Fact]
    public async Task SetEnabledAsync_WhenValid_SucceedsAndRecordsAudit()
    {
        // Arrange
        using var ctx = InMemoryDbFixture.CreateNewContext();

        var systemId = Guid.NewGuid();
        var system = new SystemRegistry { 
            Id = systemId, 
            SystemCode = "S002", 
            SystemName = "Sys2", 
            IsEnabled = false, 
            BaseUrl = "https://sys2.com" 
        };

        ctx.SystemRegistries.Add(system);
        await ctx.SaveChangesAsync();

        var repoMock = new Mock<ISystemRegistryRepository>();
        repoMock.Setup(r => r.GetByIdAsync(systemId)).ReturnsAsync(system);
        repoMock.Setup(r => r.UpdateAsync(It.IsAny<SystemRegistry>()))
            .ReturnsAsync(1);

        var createValidatorMock = new Mock<FluentValidation.IValidator<CreateSystemRegistryDto>>();
        var updateValidatorMock = new Mock<FluentValidation.IValidator<UpdateSystemRegistryDto>>();

        var userManagerMock = _fixture.CreateUserManagerMock();
        userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "admin", UserName = "adminuser" });

        var service = new SystemRegistryService(
            repoMock.Object,
            ctx,
            _fixture.Mapper,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            userManagerMock.Object);

        // Act
        var result = await service.SetEnabledAsync("admin", systemId, true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // audit log should be saved by RecordSystemRegistryAuditAsync
        var log = await ctx.AuthAuditLogs.FirstOrDefaultAsync();
        log.Should().NotBeNull();
        log!.EventType.Should().Contain("SYSTEM_REGISTRY_ENABLE");
        repoMock.Verify(r => r.UpdateAsync(It.Is<SystemRegistry>(s => s.IsEnabled == true)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenValid_UpdatesSystem()
    {
        // Arrange
        using var ctx = InMemoryDbFixture.CreateNewContext();

        var systemId = Guid.NewGuid();
        var system = new SystemRegistry
        {
            Id = systemId,
            SystemCode = "S003",
            SystemName = "Original",
            BaseUrl = "https://original.com",
            IsEnabled = true
        };
        ctx.SystemRegistries.Add(system);
        await ctx.SaveChangesAsync();

        var repoMock = new Mock<ISystemRegistryRepository>();
        repoMock.Setup(r => r.GetByIdAsync(systemId)).ReturnsAsync(system);
        repoMock.Setup(r => r.UpdateAsync(It.IsAny<SystemRegistry>()))
            .ReturnsAsync(1);

        var createValidatorMock = new Mock<FluentValidation.IValidator<CreateSystemRegistryDto>>();
        var updateValidatorMock = new Mock<FluentValidation.IValidator<UpdateSystemRegistryDto>>();
        updateValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateSystemRegistryDto>(), default))
            .ReturnsAsync(new ValidationResult());

        var userManagerMock = _fixture.CreateUserManagerMock();
        userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Id = "user-1", UserName = "editor" });

        var service = new SystemRegistryService(
            repoMock.Object,
            ctx,
            _fixture.Mapper,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            userManagerMock.Object);

        var updateDto = new UpdateSystemRegistryDto(
            SystemCode: "S003",
            SystemName: "Updated Name",
            Description: "Updated description",
            BaseUrl: "https://updated.com",
            IconUrl: "https://icon.com/icon.png",
            Category: "new-category",
            ContactEmail: "updated@b.com",
            ApiKey: null
        );

        // Act
        var result = await service.UpdateAsync("user-1", systemId, updateDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.SystemName.Should().Be(updateDto.SystemName);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<SystemRegistry>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenSystemNotFound_ReturnsFailure()
    {
        // Arrange
        using var ctx = InMemoryDbFixture.CreateNewContext();

        var repoMock = new Mock<ISystemRegistryRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((SystemRegistry)null);

        var createValidatorMock = new Mock<FluentValidation.IValidator<CreateSystemRegistryDto>>();
        var updateValidatorMock = new Mock<FluentValidation.IValidator<UpdateSystemRegistryDto>>();
        updateValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateSystemRegistryDto>(), default))
            .ReturnsAsync(new ValidationResult());

        var userManagerMock = _fixture.CreateUserManagerMock();

        var service = new SystemRegistryService(
            repoMock.Object,
            ctx,
            _fixture.Mapper,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            userManagerMock.Object);

        var updateDto = new UpdateSystemRegistryDto(
            SystemCode: "S003",
            SystemName: "New",
            Description: null,
            BaseUrl: "https://example",
            IconUrl: null,
            Category: null,
            ContactEmail: null,
            ApiKey: null
        );

        // Act
        var result = await service.UpdateAsync("user-1", Guid.NewGuid(), updateDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsSystem()
    {
        // Arrange
        using var ctx = InMemoryDbFixture.CreateNewContext();

        var systemId = Guid.NewGuid();
        var system = new SystemRegistry
        {
            Id = systemId,
            SystemCode = "S004",
            SystemName = "Retrieve Test",
            BaseUrl = "https://retrieve.com",
            IsEnabled = true
        };
        ctx.SystemRegistries.Add(system);
        await ctx.SaveChangesAsync();

        var repoMock = new Mock<ISystemRegistryRepository>();
        repoMock.Setup(r => r.GetByIdAsync(systemId)).ReturnsAsync(system);

        var createValidatorMock = new Mock<FluentValidation.IValidator<CreateSystemRegistryDto>>();
        var updateValidatorMock = new Mock<FluentValidation.IValidator<UpdateSystemRegistryDto>>();

        var service = new SystemRegistryService(
            repoMock.Object,
            ctx,
            _fixture.Mapper,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            _fixture.CreateUserManagerMock().Object);

        // Act
        var result = await service.GetByIdAsync(systemId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.SystemCode.Should().Be("S004");
        result.Data.SystemName.Should().Be("Retrieve Test");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsFailure()
    {
        // Arrange
        using var ctx = InMemoryDbFixture.CreateNewContext();

        var repoMock = new Mock<ISystemRegistryRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((SystemRegistry)null);

        var createValidatorMock = new Mock<FluentValidation.IValidator<CreateSystemRegistryDto>>();
        var updateValidatorMock = new Mock<FluentValidation.IValidator<UpdateSystemRegistryDto>>();

        var service = new SystemRegistryService(
            repoMock.Object,
            ctx,
            _fixture.Mapper,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            _fixture.CreateUserManagerMock().Object);

        // Act
        var result = await service.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task GetByCodeAsync_WhenExists_ReturnsSystem()
    {
        // Arrange
        using var ctx = InMemoryDbFixture.CreateNewContext();

        var system = new SystemRegistry
        {
            Id = Guid.NewGuid(),
            SystemCode = "SEARCH_CODE",
            SystemName = "Search Test",
            BaseUrl = "https://search.com"
        };
        ctx.SystemRegistries.Add(system);
        await ctx.SaveChangesAsync();

        var repoMock = new Mock<ISystemRegistryRepository>();
        var createValidatorMock = new Mock<FluentValidation.IValidator<CreateSystemRegistryDto>>();
        var updateValidatorMock = new Mock<FluentValidation.IValidator<UpdateSystemRegistryDto>>();

        var service = new SystemRegistryService(
            repoMock.Object,
            ctx,
            _fixture.Mapper,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            _fixture.CreateUserManagerMock().Object);

        // Act
        var result = await service.GetByCodeAsync("SEARCH_CODE");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.SystemCode.Should().Be("SEARCH_CODE");
        result.Data.SystemName.Should().Be("Search Test");
    }

    [Fact]
    public async Task GetByCodeAsync_WhenNotFound_ReturnsFailure()
    {
        // Arrange
        using var ctx = InMemoryDbFixture.CreateNewContext();

        var repoMock = new Mock<ISystemRegistryRepository>();
        var createValidatorMock = new Mock<FluentValidation.IValidator<CreateSystemRegistryDto>>();
        var updateValidatorMock = new Mock<FluentValidation.IValidator<UpdateSystemRegistryDto>>();

        var service = new SystemRegistryService(
            repoMock.Object,
            ctx,
            _fixture.Mapper,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            _fixture.CreateUserManagerMock().Object);

        // Act
        var result = await service.GetByCodeAsync("NONEXISTENT");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_DeletesSystem()
    {
        // Arrange
        using var ctx = InMemoryDbFixture.CreateNewContext();

        var systemId = Guid.NewGuid();
        var system = new SystemRegistry
        {
            Id = systemId,
            SystemCode = "S005",
            SystemName = "Delete Test",
            BaseUrl = "https://delete.com"
        };
        ctx.SystemRegistries.Add(system);
        await ctx.SaveChangesAsync();

        var repoMock = new Mock<ISystemRegistryRepository>();
        repoMock.Setup(r => r.GetByIdAsync(systemId)).ReturnsAsync(system);
        repoMock.Setup(r => r.DeleteAsync(It.IsAny<SystemRegistry>()))
            .ReturnsAsync(1);

        var createValidatorMock = new Mock<FluentValidation.IValidator<CreateSystemRegistryDto>>();
        var updateValidatorMock = new Mock<FluentValidation.IValidator<UpdateSystemRegistryDto>>();

        var service = new SystemRegistryService(
            repoMock.Object,
            ctx,
            _fixture.Mapper,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            _fixture.CreateUserManagerMock().Object);

        // Act
        var result = await service.DeleteAsync("user-1", systemId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        repoMock.Verify(r => r.DeleteAsync(It.IsAny<SystemRegistry>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFailure()
    {
        // Arrange
        using var ctx = InMemoryDbFixture.CreateNewContext();

        var repoMock = new Mock<ISystemRegistryRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((SystemRegistry)null);

        var createValidatorMock = new Mock<FluentValidation.IValidator<CreateSystemRegistryDto>>();
        var updateValidatorMock = new Mock<FluentValidation.IValidator<UpdateSystemRegistryDto>>();

        var service = new SystemRegistryService(
            repoMock.Object,
            ctx,
            _fixture.Mapper,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            _fixture.CreateUserManagerMock().Object);

        // Act
        var result = await service.DeleteAsync("user-1", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedList()
    {
        // Arrange
        using var ctx = InMemoryDbFixture.CreateNewContext();

        for (int i = 1; i <= 15; i++)
        {
            ctx.SystemRegistries.Add(new SystemRegistry
            {
                Id = Guid.NewGuid(),
                SystemCode = $"S{i:D3}",
                SystemName = $"System {i}",
                BaseUrl = $"https://sys{i}.com"
            });
        }
        await ctx.SaveChangesAsync();

        var repoMock = new Mock<ISystemRegistryRepository>();
        var createValidatorMock = new Mock<FluentValidation.IValidator<CreateSystemRegistryDto>>();
        var updateValidatorMock = new Mock<FluentValidation.IValidator<UpdateSystemRegistryDto>>();

        var service = new SystemRegistryService(
            repoMock.Object,
            ctx,
            _fixture.Mapper,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            _fixture.CreateUserManagerMock().Object);

        // Act
        var result = await service.GetAllAsync(page: 1, size: 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(10);
        result.Data.TotalCount.Should().Be(15);
        result.Data.PageNumber.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ReturnsSecondPage()
    {
        // Arrange
        using var ctx = InMemoryDbFixture.CreateNewContext();

        for (int i = 1; i <= 25; i++)
        {
            ctx.SystemRegistries.Add(new SystemRegistry
            {
                Id = Guid.NewGuid(),
                SystemCode = $"S{i:D3}",
                SystemName = $"System {i}",
                BaseUrl = $"https://sys{i}.com"
            });
        }
        await ctx.SaveChangesAsync();

        var repoMock = new Mock<ISystemRegistryRepository>();
        var createValidatorMock = new Mock<FluentValidation.IValidator<CreateSystemRegistryDto>>();
        var updateValidatorMock = new Mock<FluentValidation.IValidator<UpdateSystemRegistryDto>>();

        var service = new SystemRegistryService(
            repoMock.Object,
            ctx,
            _fixture.Mapper,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            _fixture.CreateUserManagerMock().Object);

        // Act
        var result = await service.GetAllAsync(page: 2, size: 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Items.Should().HaveCount(10);
        result.Data.TotalCount.Should().Be(25);
        result.Data.PageNumber.Should().Be(2);
    }
}
