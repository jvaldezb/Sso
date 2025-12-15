using System;
using AutoMapper;
using Moq;
using Microsoft.AspNetCore.Identity;
using identity_service.Models;
using identity_service.Services.Interfaces;

namespace identity_service.UnitTests.Fixtures;

public class ServiceTestFixture
{
    public IMapper Mapper { get; }

    public ServiceTestFixture()
    {
        var config = new MapperConfiguration(cfg =>
        {
            // Load all profiles from the identity_service Mappers assembly
            cfg.AddMaps(typeof(identity_service.Mappers.UserProfile).Assembly);
        });

        Mapper = config.CreateMapper();
    }

    public Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
    }

    public Mock<RoleManager<ApplicationRole>> CreateRoleManagerMock()
    {
        var store = new Mock<IRoleStore<ApplicationRole>>();
        return new Mock<RoleManager<ApplicationRole>>(store.Object, null, null, null, null);
    }

    public Mock<SignInManager<ApplicationUser>> CreateSignInManagerMock(Mock<UserManager<ApplicationUser>> userManagerMock)
    {
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        var options = new Mock<Microsoft.Extensions.Options.IOptions<IdentityOptions>>();
        options.Setup(o => o.Value).Returns(new IdentityOptions());
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<SignInManager<ApplicationUser>>>();
        var schemes = new Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
        var confirmation = new Mock<IUserConfirmation<ApplicationUser>>();

        return new Mock<SignInManager<ApplicationUser>>(
            userManagerMock.Object,
            contextAccessor.Object,
            claimsFactory.Object,
            options.Object,
            logger.Object,
            schemes.Object,
            confirmation.Object);
    }

    public Mock<IEmailService> CreateEmailServiceMock()
    {
        return new Mock<IEmailService>();
    }

    public Mock<IRefreshTokenService> CreateRefreshTokenServiceMock()
    {
        return new Mock<IRefreshTokenService>();
    }
}
