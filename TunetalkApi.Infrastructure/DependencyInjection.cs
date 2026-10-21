using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TunetalkApi.Application.Chat;
using TunetalkApi.Application.Channels;
using TunetalkApi.Application.Identity;
using TunetalkApi.Infrastructure.Channels;
using TunetalkApi.Infrastructure.Identity;
using TunetalkApi.Infrastructure.Persistence;

namespace TunetalkApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");
        }

        services.AddDbContext<TunetalkDbContext>(options => options.UseSqlite(connectionString));
        services.AddIdentityCore<TunetalkUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<TunetalkDbContext>()
            .AddSignInManager();

        services.AddMemoryCache();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<RadioBrowserEndpointProvider>();
        services.AddHttpClient<IRadioChannelCatalog, RadioBrowserChannelCatalog>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        services.AddScoped<IChatMessageRepository, EfChatMessageRepository>();
        services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
        services.AddSingleton<IJwtTokenIssuer, JwtTokenIssuer>();

        return services;
    }
}
