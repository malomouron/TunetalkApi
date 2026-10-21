namespace TunetalkApi.Application.Channels;

public sealed record ChannelGenre(string Id, string Name);

public static class ChannelGenres
{
    private static readonly IReadOnlyList<ChannelGenre> Genres =
    [
        new("pop", "Pop"),
        new("rock", "Rock"),
        new("jazz", "Jazz"),
        new("classical", "Classique"),
        new("electronic", "Électronique"),
        new("hip-hop", "Hip-hop"),
        new("ambient", "Ambient")
    ];

    public static IReadOnlyList<ChannelGenre> All => Genres;

    public static ChannelGenre? Find(string? id) =>
        Genres.FirstOrDefault(genre => string.Equals(genre.Id, id, StringComparison.OrdinalIgnoreCase));
}
