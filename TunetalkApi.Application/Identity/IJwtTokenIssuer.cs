namespace TunetalkApi.Application.Identity;

public interface IJwtTokenIssuer
{
    AuthenticationResponse Create(string userId, string email, string displayName);
}
