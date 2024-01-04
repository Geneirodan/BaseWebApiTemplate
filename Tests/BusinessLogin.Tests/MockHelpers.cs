using BusinessLogic.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace BusinessLogin.Tests;

public class MockHelpers
{
    public static Mock<UserManager<TUser>> TestUserManager<TUser>(IUserStore<TUser>? store = null) where TUser : class
    {
        store ??= new Mock<IUserStore<TUser>>().Object;

        var identityOptions = new IdentityOptions { Lockout = { AllowedForNewUsers = false } };
        var options = new Mock<IOptions<IdentityOptions>>();
        options.Setup(o => o.Value).Returns(identityOptions);

        var validator = new Mock<IUserValidator<TUser>>();

        var userManager = new Mock<UserManager<TUser>>(store,
            options.Object,
            new PasswordHasher<TUser>(),
            new List<IUserValidator<TUser>> { validator.Object },
            new List<PasswordValidator<TUser>> { new() },
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            new Mock<ILogger<UserManager<TUser>>>().Object);

        validator
            .Setup(v => v.ValidateAsync(userManager.Object, It.IsAny<TUser>()))
            .Returns(Task.FromResult(IdentityResult.Success))
            .Verifiable();

        return userManager;
    }

    public static Mock<SignInManager<TUser>> TestSignInManager<TUser>() where TUser : IdentityUser =>
        new(TestUserManager<TUser>().Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<TUser>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<ILogger<SignInManager<TUser>>>().Object,
            new Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>().Object,
            new Mock<IUserConfirmation<TUser>>().Object);

    public static Mock<RoleManager<TRole>> TestRoleManager<TRole>() where TRole : class =>
        new(new Mock<IRoleStore<TRole>>().Object, null!, null!, null!, null!);

    public static Mock<IOptions<AdminOptions>> TestAdminOptions()
    {
        var adminOptions = new AdminOptions
        {
            Email = "admin@mail.com",
            Password = "1String1"
        };
        var options = new Mock<IOptions<AdminOptions>>();
        options.Setup(o => o.Value).Returns(adminOptions);
        return options;
    }
    
    public static Mock<IOptions<IdentityOptions>> TestIdentityOptions()
    {
        var identityOptions = new IdentityOptions()
        {
            Password =
            {
                RequireDigit = true,
                RequiredLength = 6,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonAlphanumeric = true
            }
        };
        var options = new Mock<IOptions<IdentityOptions>>();
        options.Setup(o => o.Value).Returns(identityOptions);
        return options;
    }
}
