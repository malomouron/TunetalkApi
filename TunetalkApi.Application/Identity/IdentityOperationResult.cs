namespace TunetalkApi.Application.Identity;

public sealed record IdentityOperationResult(
    AuthenticationResponse? Authentication,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public bool Succeeded => Authentication is not null;

    public static IdentityOperationResult Success(AuthenticationResponse authentication) =>
        new(authentication, new Dictionary<string, string[]>());

    public static IdentityOperationResult Failure(IReadOnlyDictionary<string, string[]> errors) =>
        new(null, errors);
}
