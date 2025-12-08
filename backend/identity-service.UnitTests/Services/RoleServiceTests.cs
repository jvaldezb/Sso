using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using identity_service.UnitTests.Fixtures;
using identity_service.Services;
using identity_service.Models;
using identity_service.Data;
using identity_service.Dtos.Role;
using identity_service.Repositories.Interfaces;
using identity_service.Services.Interfaces;

namespace identity_service.UnitTests.Services;

public class RoleServiceTests
{
    private readonly ServiceTestFixture _fixture = new ServiceTestFixture();

    [Fact]
    public async Task GetRoleMenusAsync_NoMenuClaim_ReturnsEmpty()
    {
        using var ctx = InMemoryDbFixture.CreateNewContext();

        var systemId = Guid.NewGuid();
        var system = new SystemRegistry
        {
            Id = systemId,
            SystemCode = "SYS_A",
            SystemName = "Sys A",
            BaseUrl = "https://example"
        };
        ctx.SystemRegistries.Add(system);
        await ctx.SaveChangesAsync();

        var roleId = "role-1";
        var role = new ApplicationRole { Id = roleId, SystemId = systemId };

        var roleManagerMock = _fixture.CreateRoleManagerMock();
        roleManagerMock.Setup(r => r.FindByIdAsync(roleId)).ReturnsAsync(role);
        roleManagerMock.Setup(r => r.GetClaimsAsync(role)).ReturnsAsync(new List<Claim>());

        var userManagerMock = _fixture.CreateUserManagerMock();

        var menuRepoMock = new Mock<IMenuRepository>();
        var createValidatorMock = new Mock<IValidator<identity_service.Dtos.Role.CreateRoleDto>>();
        var updateValidatorMock = new Mock<IValidator<identity_service.Dtos.Role.UpdateRoleDto>>();
        var roleClaimEncoderMock = new Mock<IRoleClaimEncoderService>();

        var sut = new RoleService(
            roleManagerMock.Object,
            userManagerMock.Object,
            ctx,
            menuRepoMock.Object,
            _fixture.Mapper,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            roleClaimEncoderMock.Object);

        var result = await sut.GetRoleMenusAsync(roleId);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRoleMenusAsync_WithClaim_ReturnsDecodedValues()
    {
        using var ctx = InMemoryDbFixture.CreateNewContext();

        var systemId = Guid.NewGuid();
        var system = new SystemRegistry
        {
            Id = systemId,
            SystemCode = "SYS_B",
            SystemName = "Sys B",
            BaseUrl = "https://example"
        };
        ctx.SystemRegistries.Add(system);

        var menu1Id = Guid.NewGuid();
        var menu2Id = Guid.NewGuid();

        ctx.Menus.Add(new Menu
        {
            Id = menu1Id,
            SystemId = systemId,
            MenuLabel = "Menu 1",
            Module = "M1",
            BitPosition = 0,
            RequiredClaimType = null,
            RequiredClaimMinValue = 0,
            Level = 1,
            OrderIndex = 0
        });

        ctx.Menus.Add(new Menu
        {
            Id = menu2Id,
            SystemId = systemId,
            MenuLabel = "Menu 2",
            Module = "",
            BitPosition = null,
            RequiredClaimType = null,
            RequiredClaimMinValue = 0,
            Level = 1,
            OrderIndex = 1
        });

        await ctx.SaveChangesAsync();

        var roleId = "role-2";
        var role = new ApplicationRole { Id = roleId, SystemId = systemId };

        var roleManagerMock = _fixture.CreateRoleManagerMock();
        roleManagerMock.Setup(r => r.FindByIdAsync(roleId)).ReturnsAsync(role);

        var claimType = $"Access:{system.SystemCode}";
        var fakeClaim = new Claim(claimType, "fake-encoded-value");
        roleManagerMock.Setup(r => r.GetClaimsAsync(role)).ReturnsAsync(new List<Claim> { fakeClaim });

        var userManagerMock = _fixture.CreateUserManagerMock();
        var menuRepoMock = new Mock<IMenuRepository>();
        var createValidatorMock = new Mock<IValidator<identity_service.Dtos.Role.CreateRoleDto>>();
        var updateValidatorMock = new Mock<IValidator<identity_service.Dtos.Role.UpdateRoleDto>>();

        var decoded = new List<MenuRoleRwxDto>
        {
            new MenuRoleRwxDto { Id = menu1Id, Module = "M1", BitPosition = 0, RwxValue = 5 }
        };

        var roleClaimEncoderMock = new Mock<IRoleClaimEncoderService>();
        roleClaimEncoderMock
            .Setup(r => r.DecodeAsync(It.IsAny<List<identity_service.Dtos.Role.MenuRoleBitPositionDto>>(), fakeClaim.Value))
            .ReturnsAsync(decoded);

        var sut = new RoleService(
            roleManagerMock.Object,
            userManagerMock.Object,
            ctx,
            menuRepoMock.Object,
            _fixture.Mapper,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            roleClaimEncoderMock.Object);

        var result = await sut.GetRoleMenusAsync(roleId);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        var menu1 = result.Data.FirstOrDefault(m => m.Id == menu1Id);
        menu1.Should().NotBeNull();
        menu1!.RwxValue.Should().Be(5);

        var menu2 = result.Data.FirstOrDefault(m => m.Id == menu2Id);
        menu2.Should().NotBeNull();
        menu2!.RwxValue.Should().BeNull();
    }

    [Fact]
    public async Task SetRoleMenusAsync_WhenCalled_Encodes_AddsClaimAndRecordsAudit()
    {
        using var ctx = InMemoryDbFixture.CreateNewContext();

        var systemId = Guid.NewGuid();
        var system = new SystemRegistry
        {
            Id = systemId,
            SystemCode = "SYS_C",
            SystemName = "Sys C",
            BaseUrl = "https://example"
        };
        ctx.SystemRegistries.Add(system);

        var menuId = Guid.NewGuid();
        ctx.Menus.Add(new Menu
        {
            Id = menuId,
            SystemId = systemId,
            MenuLabel = "Menu X",
            Module = "MX",
            BitPosition = 0
        });

        await ctx.SaveChangesAsync();

        var roleId = "role-3";
        var role = new ApplicationRole { Id = roleId, SystemId = systemId };

        var roleManagerMock = _fixture.CreateRoleManagerMock();
        roleManagerMock.Setup(r => r.FindByIdAsync(roleId)).ReturnsAsync(role);
        roleManagerMock.Setup(r => r.GetClaimsAsync(role)).ReturnsAsync(new List<Claim>());
        roleManagerMock.Setup(r => r.AddClaimAsync(role, It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);

        var userManagerMock = _fixture.CreateUserManagerMock();
        var menuRepoMock = new Mock<IMenuRepository>();
        var createValidatorMock = new Mock<IValidator<identity_service.Dtos.Role.CreateRoleDto>>();
        var updateValidatorMock = new Mock<IValidator<identity_service.Dtos.Role.UpdateRoleDto>>();

        var encodedValue = new BigInteger(12345);
        var roleClaimEncoderMock = new Mock<IRoleClaimEncoderService>();
        roleClaimEncoderMock
            .Setup(r => r.EncodeAsync(It.IsAny<List<MenuRoleRwxDto>>()))
            .ReturnsAsync(encodedValue);

        var sut = new RoleService(
            roleManagerMock.Object,
            userManagerMock.Object,
            ctx,
            menuRepoMock.Object,
            _fixture.Mapper,
            createValidatorMock.Object,
            updateValidatorMock.Object,
            roleClaimEncoderMock.Object);

        var request = new List<MenuRoleRwxRequestDto>
        {
            new MenuRoleRwxRequestDto { Id = menuId, RwxValue = 3 }
        };

        var result = await sut.SetRoleMenusAsync("user-100", roleId, request);

        result.IsSuccess.Should().BeTrue();

        var expectedClaimType = $"Access:{system.SystemCode}";
        roleManagerMock.Verify(r => r.AddClaimAsync(
            role,
            It.Is<Claim>(c => c.Type == expectedClaimType && c.Value == encodedValue.ToString())
        ), Times.Once);

        var log = await ctx.AuthAuditLogs.FirstOrDefaultAsync();
        log.Should().NotBeNull();
        log!.EventType.Should().Contain("ROLE_SET_ROLE_MENUS");
    }
}
