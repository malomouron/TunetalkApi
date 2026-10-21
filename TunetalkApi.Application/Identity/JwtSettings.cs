namespace TunetalkApi.Application.Identity;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "Tunetalk";

    public string Audience { get; init; } = "Tunetalk.Mobile";

    public string SigningKey { get; init; } = string.Empty;

    public int AccessTokenMinutes { get; init; } = 60;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience))
        {
            throw new InvalidOperationException("JWT issuer and audience must be configured.");
        }

        if (System.Text.Encoding.UTF8.GetByteCount(SigningKey) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey must contain at least 32 UTF-8 bytes. Configure it with user-secrets or an environment variable.");
        }

        if (AccessTokenMinutes is < 5 or > 1440)
        {
            throw new InvalidOperationException("Jwt:AccessTokenMinutes must be between 5 and 1440.");
        }
    }
}
