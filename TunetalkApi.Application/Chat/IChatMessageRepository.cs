using TunetalkApi.Domain.Entities;

namespace TunetalkApi.Application.Chat;

public interface IChatMessageRepository
{
    Task<IReadOnlyList<ChatMessage>> GetRecentAsync(
        string channelId,
        int take,
        CancellationToken cancellationToken);

    Task AddAsync(ChatMessage message, CancellationToken cancellationToken);
}
