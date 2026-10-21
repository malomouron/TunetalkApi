namespace TunetalkApi.Application.Identity;

public interface IUserAuthenticationService
{
    Task<IdentityOperationResult> RegisterAsync(
        string displayName,
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<IdentityOperationResult> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken);
}
