using TunetalkApi.Domain.Entities;

namespace TunetalkApi.Application.Chat;

public sealed class ChatMessageService(
    IChatMessageRepository repository,
    TimeProvider timeProvider)
{
    public async Task<IReadOnlyList<ChatMessageDto>> GetRecentAsync(
        string channelId,
        CancellationToken cancellationToken)
    {
        var messages = await repository.GetRecentAsync(channelId, 50, cancellationToken);

        return messages
            .Select(ToDto)
            .ToArray();
    }

    public async Task<ChatMessageDto> SendAsync(
        string channelId,
        Guid userId,
        string displayName,
        string content,
        CancellationToken cancellationToken)
    {
        var message = ChatMessage.Create(channelId, userId, displayName, content, timeProvider.GetUtcNow());
        await repository.AddAsync(message, cancellationToken);

        return ToDto(message);
    }

    private static ChatMessageDto ToDto(ChatMessage message) =>
        new(
            message.Id.ToString(),
            message.ChannelId,
            message.DisplayName,
            message.Content,
            message.CreatedAt);
}
