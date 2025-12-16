using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using FluentAssertions;
using Moq;
using Xunit;
using identity_service.Models;
using identity_service.Repositories.Interfaces;
using identity_service.Services;
using identity_service.Dtos.RoleMenu;

namespace identity_service.UnitTests.Services;

public class RoleMenuServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        var repoMock = new Mock<IRoleMenuRepository>();
        var menuRepoMock = new Mock<IMenuRepository>();

        var items = new List<RoleMenu>();
        for (int i = 0; i < 25; i++)
        {
            items.Add(new RoleMenu
            {
                Id = Guid.NewGuid(),
                RoleId = Guid.NewGuid(),
                MenuId = Guid.NewGuid(),
                AccessLevel = i % 3,
                UserCreate = "u"
            });
        }

        repoMock.Setup(r => r.Query()).Returns(items.AsQueryable());

        var sut = new RoleMenuService(repoMock.Object, menuRepoMock.Object);

        var result = await sut.GetAllAsync(page: 2, size: 10);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(10);
        result.Data.TotalCount.Should().Be(25);
        result.Data.PageNumber.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsFailure_WhenMissing()
    {
        var repoMock = new Mock<IRoleMenuRepository>();
        var menuRepoMock = new Mock<IMenuRepository>();

        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((RoleMenu)null);

        var sut = new RoleMenuService(repoMock.Object, menuRepoMock.Object);

        var result = await sut.GetByIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage!.ToLowerInvariant().Should().Contain("not found");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenFound()
    {
        var repoMock = new Mock<IRoleMenuRepository>();
        var menuRepoMock = new Mock<IMenuRepository>();

        var entity = new RoleMenu
        {
            Id = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            MenuId = Guid.NewGuid(),
            AccessLevel = 7,
            UserCreate = "creator",
            DateCreate = DateTimeOffset.UtcNow
        };

        repoMock.Setup(r => r.GetByIdAsync(entity.Id)).ReturnsAsync(entity);

        var sut = new RoleMenuService(repoMock.Object, menuRepoMock.Object);

        var result = await sut.GetByIdAsync(entity.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(entity.Id);
        result.Data.AccessLevel.Should().Be(entity.AccessLevel);
    }

    

    [Fact]
    public async Task GetMenusByRoleAsync_ReturnsCombinedDtos()
    {
        var repoMock = new Mock<IRoleMenuRepository>();
        var menuRepoMock = new Mock<IMenuRepository>();

        var roleId = Guid.NewGuid();
        var menuId = Guid.NewGuid();

        var rm = new RoleMenu { Id = Guid.NewGuid(), RoleId = roleId, MenuId = menuId, AccessLevel = 5 };
        repoMock.Setup(r => r.Query()).Returns(new List<RoleMenu> { rm }.AsQueryable());

        var menu = new Menu { Id = menuId, MenuLabel = "L1", SystemId = Guid.NewGuid(), Level = 1, ParentId = null, OrderIndex = 0 };
        menuRepoMock.Setup(m => m.Query()).Returns(new List<Menu> { menu }.AsQueryable());

        var sut = new RoleMenuService(repoMock.Object, menuRepoMock.Object);

        var result = await sut.GetMenusByRoleAsync(roleId);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        var dto = result.Data[0];
        dto.MenuLabel.Should().Be(menu.MenuLabel);
        dto.AccessLevel.Should().Be(rm.AccessLevel);
        dto.RoleMenuId.Should().Be(rm.Id);
    }

    [Fact]
    public async Task CreateAsync_AddsAndReturnsDto()
    {
        var repoMock = new Mock<IRoleMenuRepository>();
        var menuRepoMock = new Mock<IMenuRepository>();

        var dto = new RoleMenuCreateDto { RoleId = Guid.NewGuid(), MenuId = Guid.NewGuid(), AccessLevel = 2 };

        repoMock.Setup(r => r.AddAsync(It.IsAny<RoleMenu>())).ReturnsAsync((RoleMenu e) =>
        {
            e.Id = Guid.NewGuid();
            return e;
        });

        var sut = new RoleMenuService(repoMock.Object, menuRepoMock.Object);

        var result = await sut.CreateAsync("creator", dto);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.RoleId.Should().Be(dto.RoleId);
        result.Data.MenuId.Should().Be(dto.MenuId);
        repoMock.Verify(r => r.AddAsync(It.IsAny<RoleMenu>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenMissing_AndTrue_WhenDeleted()
    {
        var repoMock = new Mock<IRoleMenuRepository>();
        var menuRepoMock = new Mock<IMenuRepository>();

        var id = Guid.NewGuid();

        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((RoleMenu)null);

        var sut = new RoleMenuService(repoMock.Object, menuRepoMock.Object);

        var notFound = await sut.DeleteAsync("u", id);
        notFound.IsSuccess.Should().BeFalse();

        var existing = new RoleMenu { Id = id };
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
        repoMock.Setup(r => r.DeleteAsync(existing)).ReturnsAsync(1);

        var deleted = await sut.DeleteAsync("u", id);
        deleted.IsSuccess.Should().BeTrue();
        deleted.Data.Should().BeTrue();
    }
}
