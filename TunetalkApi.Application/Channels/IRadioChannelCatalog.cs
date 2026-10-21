namespace TunetalkApi.Application.Channels;

public interface IRadioChannelCatalog
{
    Task<IReadOnlyList<RadioChannel>> GetByGenreAsync(string genreId, CancellationToken cancellationToken);

    Task<RadioChannel?> GetByIdAsync(string channelId, CancellationToken cancellationToken);
}
