namespace TunetalkApi.Application.Identity;

public sealed record AuthenticationResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string Email,
    string DisplayName);
