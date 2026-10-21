namespace TunetalkApi.Application.Channels;

public sealed record RadioChannel(
    string Id,
    string Name,
    string Genre,
    string? Description,
    string? StreamUrl,
    string? ArtworkUrl);
