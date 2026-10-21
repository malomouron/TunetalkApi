using TunetalkApi.Application.Channels;

namespace TunetalkApi.Tests;

public sealed class ChannelGenresTests
{
    [Theory]
    [InlineData("pop")]
    [InlineData("rock")]
    [InlineData("jazz")]
    [InlineData("classical")]
    [InlineData("electronic")]
    [InlineData("hip-hop")]
    [InlineData("ambient")]
    public void Find_returns_only_supported_genres(string genreId)
    {
        Assert.NotNull(ChannelGenres.Find(genreId));
        Assert.NotNull(ChannelGenres.Find(genreId.ToUpperInvariant()));
    }

    [Fact]
    public void Find_rejects_unknown_genres()
    {
        Assert.Null(ChannelGenres.Find("user-created"));
    }
}
