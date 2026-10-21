using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TunetalkApi.Application.Channels;

namespace TunetalkApi.Infrastructure.Channels;

public sealed class RadioBrowserChannelCatalog(
    HttpClient httpClient,
    RadioBrowserEndpointProvider endpointProvider,
    IMemoryCache cache,
    ILogger<RadioBrowserChannelCatalog> logger) : IRadioChannelCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<RadioChannel>> GetByGenreAsync(
        string genreId,
        CancellationToken cancellationToken)
    {
        var genre = ChannelGenres.Find(genreId)
            ?? throw new ArgumentException("Genre is not supported.", nameof(genreId));
        var cacheKey = $"radio-browser:genre:{genre.Id}";
        if (cache.TryGetValue(cacheKey, out IReadOnlyList<RadioChannel>? cached) && cached is not null)
        {
            return cached;
        }

        var path = $"json/stations/bytag/{Uri.EscapeDataString(genre.Id)}?limit=24&hidebroken=true&order=clickcount&reverse=true";
        var stations = await GetAsync<List<RadioBrowserStation>>(path, cancellationToken);
        var channels = stations
            .Where(station => station.LastCheckOk == true
                && !string.IsNullOrWhiteSpace(station.StationUuid)
                && !string.IsNullOrWhiteSpace(station.Name))
            .Select(station => ToChannel(station, genre))
            .ToArray();

        cache.Set(cacheKey, channels, TimeSpan.FromMinutes(5));
        foreach (var channel in channels)
        {
            cache.Set($"radio-browser:station:{channel.Id}", channel, TimeSpan.FromMinutes(5));
        }

        return channels;
    }

    public async Task<RadioChannel?> GetByIdAsync(string channelId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(channelId, out _))
        {
            return null;
        }

        var cacheKey = $"radio-browser:station:{channelId}";
        if (cache.TryGetValue(cacheKey, out RadioChannel? cached) && cached is not null)
        {
            return cached;
        }

        var path = $"json/stations/byuuid/{Uri.EscapeDataString(channelId)}";
        var stations = await GetAsync<List<RadioBrowserStation>>(path, cancellationToken);
        var station = stations.FirstOrDefault(item =>
            item.LastCheckOk == true
            && string.Equals(item.StationUuid, channelId, StringComparison.OrdinalIgnoreCase));

        if (station is null)
        {
            return null;
        }

        var firstTag = station.Tags?
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
        var genre = ChannelGenres.Find(firstTag)
            ?? new ChannelGenre(firstTag ?? "radio", firstTag ?? "Radio");
        var channel = ToChannel(station, genre);
        cache.Set(cacheKey, channel, TimeSpan.FromMinutes(5));
        return channel;
    }

    private async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        Exception? lastFailure = null;
        var endpoints = await endpointProvider.GetEndpointsAsync(cancellationToken);

        foreach (var endpoint in endpoints.Take(4))
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(endpoint, path));
                request.Headers.UserAgent.ParseAdd("Tunetalk/1.0");
                using var response = await httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    lastFailure = new HttpRequestException(
                        $"The radio directory returned {(int)response.StatusCode}.",
                        null,
                        response.StatusCode);
                    logger.LogWarning(
                        "Radio directory endpoint {Endpoint} returned {StatusCode}.",
                        endpoint.Host,
                        (int)response.StatusCode);
                    continue;
                }

                var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
                if (result is not null)
                {
                    return result;
                }

                lastFailure = new JsonException("The radio directory returned an empty response.");
            }
            catch (HttpRequestException exception)
            {
                lastFailure = exception;
                logger.LogWarning(exception, "Radio directory endpoint {Endpoint} could not be reached.", endpoint.Host);
            }
            catch (JsonException exception)
            {
                lastFailure = exception;
                logger.LogWarning(exception, "Radio directory endpoint {Endpoint} returned invalid JSON.", endpoint.Host);
            }
            catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
            {
                lastFailure = exception;
                logger.LogWarning(exception, "Radio directory endpoint {Endpoint} timed out.", endpoint.Host);
            }
        }

        throw new CatalogUnavailableException(
            "The external radio directory is temporarily unavailable. Please try again.",
            lastFailure);
    }

    private static RadioChannel ToChannel(RadioBrowserStation station, ChannelGenre genre)
    {
        var tags = station.Tags?
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Take(3);
        var description = tags is null ? null : string.Join(" · ", tags);

        return new RadioChannel(
            station.StationUuid,
            station.Name.Trim(),
            genre.Name,
            string.IsNullOrWhiteSpace(description) ? null : description,
            GetHttpsUri(station.UrlResolved),
            GetHttpsUri(station.Favicon));
    }

    private static string? GetHttpsUri(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps
            ? uri.AbsoluteUri
            : null;

    private sealed record RadioBrowserStation(
        [property: JsonPropertyName("stationuuid")] string StationUuid,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("url_resolved")] string? UrlResolved,
        [property: JsonPropertyName("favicon")] string? Favicon,
        [property: JsonPropertyName("tags")] string? Tags,
        [property: JsonPropertyName("lastcheckok")] bool? LastCheckOk);
}
