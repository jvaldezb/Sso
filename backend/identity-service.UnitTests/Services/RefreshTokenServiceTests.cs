using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using identity_service.Models;
using identity_service.UnitTests.Fixtures;
using identity_service.Services;
using Xunit;

namespace identity_service.UnitTests.Services;

public class RefreshTokenServiceTests
{
    private readonly ServiceTestFixture _fixture = new ServiceTestFixture();

    [Fact]
    public void GenerateRefreshToken_ReturnsTokenWithExpectedProperties()
    {
        using var ctx = InMemoryDbFixture.CreateNewContext();
        var userManagerMock = _fixture.CreateUserManagerMock();

        var sut = new RefreshTokenService(ctx, userManagerMock.Object);

        var ip = "127.0.0.1";
        var device = "unit-test-device";

        var token = sut.GenerateRefreshToken(ip, device);

        token.Should().NotBeNull();
        token.Token.Should().NotBeNullOrWhiteSpace();
        token.CreatedAt.Should().BeBefore(token.ExpiresAt);
        token.ExpiresAt.Should().BeCloseTo(token.CreatedAt.AddDays(30), precision: TimeSpan.FromSeconds(5));
        token.IpAddress.Should().Be(ip);
        token.DeviceInfo.Should().Be(device);
        token.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task SaveRefreshTokenAsync_PersistsTokenWithUserInfo()
    {
        using var ctx = InMemoryDbFixture.CreateNewContext();
        var userManagerMock = _fixture.CreateUserManagerMock();

        var sut = new RefreshTokenService(ctx, userManagerMock.Object);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "save_test_user"
        };

        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var token = sut.GenerateRefreshToken("1.2.3.4", "device-x");

        await sut.SaveRefreshTokenAsync(user, token);

        var persisted = await ctx.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token.Token);

        persisted.Should().NotBeNull();
        persisted!.UserId.Should().Be(user.Id);
        persisted.UserCreate.Should().Be(user.UserName);
        persisted.DateCreate.Should().NotBe(default(DateTime));
    }

    [Fact]
    public async Task RotateRefreshTokenAsync_SuccessfullyRotatesToken()
    {
        using var ctx = InMemoryDbFixture.CreateNewContext();
        var userManagerMock = _fixture.CreateUserManagerMock();

        var sut = new RefreshTokenService(ctx, userManagerMock.Object);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "rotate_user"
        };
        ctx.Users.Add(user);

        var oldToken = new RefreshToken
        {
            Token = "old-token-123",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            IpAddress = "8.8.8.8",
            DeviceInfo = "old-device",
            UserId = user.Id,
            User = user
        };

        ctx.RefreshTokens.Add(oldToken);
        await ctx.SaveChangesAsync();

        var (success, returnedUser, newToken) = await sut.RotateRefreshTokenAsync(oldToken.Token, "9.9.9.9", "new-device");

        success.Should().BeTrue();
        returnedUser.Should().NotBeNull();
        returnedUser!.Id.Should().Be(user.Id);

        newToken.Should().NotBeNull();
        newToken!.Token.Should().NotBeNullOrWhiteSpace();
        newToken.UserId.Should().Be(user.Id);
        newToken.DeviceInfo.Should().Be("new-device");
        newToken.IpAddress.Should().Be("9.9.9.9");

        var reloadedOld = await ctx.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(t => t.Id == oldToken.Id);
        reloadedOld.Should().NotBeNull();
        reloadedOld!.IsRevoked.Should().BeTrue();
        reloadedOld.ReplacedByToken.Should().Be(newToken.Token);

        var persistedNew = await ctx.RefreshTokens.FirstOrDefaultAsync(t => t.Token == newToken.Token);
        persistedNew.Should().NotBeNull();
    }

    [Fact]
    public async Task RotateRefreshTokenAsync_ReturnsFalse_WhenTokenMissing()
    {
        using var ctx = InMemoryDbFixture.CreateNewContext();
        var userManagerMock = _fixture.CreateUserManagerMock();
        var sut = new RefreshTokenService(ctx, userManagerMock.Object);

        var (success, user, newToken) = await sut.RotateRefreshTokenAsync("non-existent-token", null, null);

        success.Should().BeFalse();
        user.Should().BeNull();
        newToken.Should().BeNull();
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_RevokeAndIsRefreshTokenValidReflectsChange()
    {
        using var ctx = InMemoryDbFixture.CreateNewContext();
        var userManagerMock = _fixture.CreateUserManagerMock();
        var sut = new RefreshTokenService(ctx, userManagerMock.Object);

        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "revoke_user" };
        ctx.Users.Add(user);

        var token = new RefreshToken
        {
            Token = "revokable",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            UserId = user.Id,
            User = user
        };

        ctx.RefreshTokens.Add(token);
        await ctx.SaveChangesAsync();

        var revoked = await sut.RevokeRefreshTokenAsync(token.Token);
        revoked.Should().BeTrue();

        var isValid = await sut.IsRefreshTokenValidAsync(token.Token);
        isValid.Should().BeFalse();

        var persisted = await ctx.RefreshTokens.FirstAsync(t => t.Token == token.Token);
        persisted.IsRevoked.Should().BeTrue();
        persisted.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeAllTokensForUserAsync_RevokesAllActiveTokens()
    {
        using var ctx = InMemoryDbFixture.CreateNewContext();
        var userManagerMock = _fixture.CreateUserManagerMock();
        var sut = new RefreshTokenService(ctx, userManagerMock.Object);

        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "global_logout_user" };
        ctx.Users.Add(user);

        var t1 = new RefreshToken
        {
            Token = "t1",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            IsRevoked = false,
            UserId = user.Id,
            User = user
        };
        var t2 = new RefreshToken
        {
            Token = "t2",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            IsRevoked = false,
            UserId = user.Id,
            User = user
        };

        ctx.RefreshTokens.AddRange(t1, t2);
        await ctx.SaveChangesAsync();

        await sut.RevokeAllTokensForUserAsync(user.Id);

        var tokens = await ctx.RefreshTokens.Where(t => t.UserId == user.Id).ToListAsync();
        tokens.Should().NotBeNull();
        tokens.Should().OnlyContain(t => t.IsRevoked);
    }
}
