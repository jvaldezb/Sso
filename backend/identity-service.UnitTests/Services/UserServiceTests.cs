using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using identity_service.Models;
using identity_service.UnitTests.Fixtures;
using identity_service.Data;
using identity_service.Repositories.Interfaces;
using identity_service.Services;
using identity_service.Services.Interfaces;
using identity_service.Dtos.User;
using Xunit;

namespace identity_service.UnitTests.Services;

public class UserServiceTests
{
    private readonly ServiceTestFixture _fixture = new ServiceTestFixture();

    [Fact]
    public async Task RegisterAsync_WhenUserManagerCreatesUser_ReturnsSuccess()
    {
        // Arrange
        using var ctx = InMemoryDbFixture.CreateNewContext();

        var userManagerMock = _fixture.CreateUserManagerMock();
        userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(m => m.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);

        var testRoleId = Guid.NewGuid();
        var testRole = new ApplicationRole { Id = testRoleId.ToString(), Name = "TestRole" };
        
        var roleManagerMock = _fixture.CreateRoleManagerMock();
        roleManagerMock.Setup(m => m.FindByIdAsync(testRoleId.ToString()))
            .ReturnsAsync(testRole);

        var signInManagerMock = _fixture.CreateSignInManagerMock(userManagerMock);
        var emailServiceMock = _fixture.CreateEmailServiceMock();
        var refreshTokenServiceMock = _fixture.CreateRefreshTokenServiceMock();
        var tokenGeneratorMock = new Mock<ITokenGenerator>();

        var userSessionRepoMock = new Mock<IUserSessionRepository>();

        var configurationMock = new Mock<IConfiguration>();

        var sut = new UserService(
            userManagerMock.Object,
            configurationMock.Object,
            ctx,
            userSessionRepoMock.Object,
            emailServiceMock.Object,
            signInManagerMock.Object,
            roleManagerMock.Object,
            _fixture.Mapper,
            refreshTokenServiceMock.Object,
            tokenGeneratorMock.Object);

        var dto = new UserForCreateDto
        {
            FullName = "Test Use r",
            UserName = "testuser",
            Email = "test@example.com",
            Password = "P@ssw0rd!",
            DocumentType = "ID",
            DocumentNumber = "12345678",
            RoleIds = new List<Guid> { testRoleId }
        };

        // Act
        var result = await sut.registerAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.UserName.Should().Be(dto.UserName);
    }
}
