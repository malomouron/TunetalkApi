using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TunetalkApi.Application.Channels;

namespace TunetalkApi.Infrastructure.Channels;

public sealed class RadioBrowserEndpointProvider(
    IMemoryCache cache,
    ILogger<RadioBrowserEndpointProvider> logger)
{
    private const string CacheKey = "radio-browser:endpoints";

    public async Task<IReadOnlyList<Uri>> GetEndpointsAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(CacheKey, out IReadOnlyList<Uri>? cached) && cached is not null)
        {
            return cached;
        }

        IPAddress[] addresses;
        try
        {
            addresses = await Dns.GetHostAddressesAsync("all.api.radio-browser.info", cancellationToken);
        }
        catch (SocketException exception)
        {
            throw new CatalogUnavailableException("The radio directory server list could not be resolved.", exception);
        }

        var endpoints = new HashSet<Uri>();
        foreach (var address in addresses.OrderBy(_ => Random.Shared.Next()))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var host = (await Dns.GetHostEntryAsync(address).WaitAsync(cancellationToken)).HostName.TrimEnd('.');
                if (!host.EndsWith(".api.radio-browser.info", StringComparison.OrdinalIgnoreCase)
                    || host.StartsWith("all.", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                endpoints.Add(new UriBuilder(Uri.UriSchemeHttps, host).Uri);
            }
            catch (SocketException exception)
            {
                logger.LogDebug(exception, "Reverse DNS lookup failed for a radio directory server.");
            }
        }

        if (endpoints.Count == 0)
        {
            throw new CatalogUnavailableException("The radio directory did not return any usable server addresses.");
        }

        var result = endpoints.ToArray();
        cache.Set(CacheKey, result, TimeSpan.FromMinutes(15));
        return result;
    }
}
