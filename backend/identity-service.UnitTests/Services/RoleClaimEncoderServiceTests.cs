using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using identity_service.UnitTests.Fixtures;
using identity_service.Services;
using identity_service.Models;
using identity_service.Dtos.Role;
using AutoMapper;

namespace identity_service.UnitTests.Services;

public class RoleClaimEncoderServiceTests
{
    private readonly ServiceTestFixture _fixture = new ServiceTestFixture();

    [Fact]
    public async Task EncodeAsync_EncodesPermissionsCorrectly()
    {
        var mapper = _fixture.Mapper;
        var service = new RoleClaimEncoderService(mapper);

        var perms = new List<MenuRoleRwxDto>
        {
            new MenuRoleRwxDto { Id = Guid.NewGuid(), Module = "ModA", BitPosition = 0, RwxValue = 1 },
            new MenuRoleRwxDto { Id = Guid.NewGuid(), Module = "ModB", BitPosition = 1, RwxValue = 3 },
            new MenuRoleRwxDto { Id = Guid.NewGuid(), Module = "ModC", BitPosition = 2, RwxValue = 7 }
        };

        var result = await service.EncodeAsync(perms);

        var expected = new BigInteger((1) | (3 << 3) | (7 << 6));
        result.Should().Be(expected);
    }

    [Fact]
    public async Task EncodeAsync_MasksValues()
    {
        var mapper = _fixture.Mapper;
        var service = new RoleClaimEncoderService(mapper);

        var perms = new List<MenuRoleRwxDto>
        {
            new MenuRoleRwxDto { Id = Guid.NewGuid(), Module = "ModA", BitPosition = 0, RwxValue = 15 }
        };

        var result = await service.EncodeAsync(perms);
        var expected = new BigInteger(7); // masked to 7 in position 0
        result.Should().Be(expected);
    }

    [Fact]
    public async Task DecodeAsync_ParsesValueAndExtractsPermissions()
    {
        var mapper = _fixture.Mapper;
        var service = new RoleClaimEncoderService(mapper);

        var menus = new List<MenuRoleBitPositionDto>
        {
            new MenuRoleBitPositionDto { Id = Guid.NewGuid(), Module = "ModA", BitPosition = 0 },
            new MenuRoleBitPositionDto { Id = Guid.NewGuid(), Module = "ModB", BitPosition = 1 },
            new MenuRoleBitPositionDto { Id = Guid.NewGuid(), Module = "ModC", BitPosition = 2 }
        };

        var value = new BigInteger((2) | (5 << 3) | (6 << 6));
        var decoded = await service.DecodeAsync(menus, value.ToString());

        decoded.Should().HaveCount(3);
        decoded.Find(x => x.Module == "ModA")!.RwxValue.Should().Be(2);
        decoded.Find(x => x.Module == "ModB")!.RwxValue.Should().Be(5);
        decoded.Find(x => x.Module == "ModC")!.RwxValue.Should().Be(6);
    }

    [Fact]
    public async Task DecodeAsync_NonNumericString_ReturnsZeroValuesForModules()
    {
        var mapper = _fixture.Mapper;
        var service = new RoleClaimEncoderService(mapper);

        var menus = new List<MenuRoleBitPositionDto>
        {
            new MenuRoleBitPositionDto { Id = Guid.NewGuid(), Module = "ModA", BitPosition = 0 },
            new MenuRoleBitPositionDto { Id = Guid.NewGuid(), Module = "ModB", BitPosition = 1 }
        };

        var decoded = await service.DecodeAsync(menus, "not-a-number");

        decoded.Find(x => x.Module == "ModA")!.RwxValue.Should().Be(0);
        decoded.Find(x => x.Module == "ModB")!.RwxValue.Should().Be(0);
    }

    [Fact]
    public async Task DecodeAsync_EmptyString_ReturnsEmptyList()
    {
        var mapper = _fixture.Mapper;
        var service = new RoleClaimEncoderService(mapper);

        var decoded = await service.DecodeAsync(new List<MenuRoleBitPositionDto>(), string.Empty);

        decoded.Should().BeEmpty();
    }
}
