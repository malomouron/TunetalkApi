namespace TunetalkApi.Application.Chat;

public sealed record ChatMessageDto(
    string Id,
    string ChannelId,
    string DisplayName,
    string Content,
    DateTimeOffset CreatedAt);
