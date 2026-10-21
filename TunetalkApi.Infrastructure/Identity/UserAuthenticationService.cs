using Microsoft.AspNetCore.Identity;
using TunetalkApi.Application.Identity;

namespace TunetalkApi.Infrastructure.Identity;

public sealed class UserAuthenticationService(
    UserManager<TunetalkUser> userManager,
    SignInManager<TunetalkUser> signInManager,
    IJwtTokenIssuer tokenIssuer) : IUserAuthenticationService
{
    public async Task<IdentityOperationResult> RegisterAsync(
        string displayName,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedEmail = email.Trim();
        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            return IdentityOperationResult.Failure(new Dictionary<string, string[]>
            {
                ["email"] = ["An account with this email already exists."]
            });
        }

        var user = new TunetalkUser
        {
            Id = Guid.NewGuid(),
            UserName = normalizedEmail,
            Email = normalizedEmail,
            DisplayName = displayName.Trim()
        };
        var creation = await userManager.CreateAsync(user, password);
        if (!creation.Succeeded)
        {
            var errors = creation.Errors
                .GroupBy(error => error.Code)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.Description).ToArray());

            return IdentityOperationResult.Failure(errors);
        }

        return IdentityOperationResult.Success(
            tokenIssuer.Create(user.Id.ToString(), user.Email!, user.DisplayName));
    }

    public async Task<IdentityOperationResult> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user is null)
        {
            return InvalidCredentials();
        }

        var signIn = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!signIn.Succeeded)
        {
            return InvalidCredentials();
        }

        return IdentityOperationResult.Success(
            tokenIssuer.Create(user.Id.ToString(), user.Email!, user.DisplayName));
    }

    private static IdentityOperationResult InvalidCredentials() =>
        IdentityOperationResult.Failure(new Dictionary<string, string[]>
        {
            ["credentials"] = ["Email or password is incorrect."]
        });
}
