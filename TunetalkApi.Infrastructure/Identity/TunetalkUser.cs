using Microsoft.AspNetCore.Identity;

namespace TunetalkApi.Infrastructure.Identity;

public sealed class TunetalkUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
}
