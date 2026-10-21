namespace TunetalkApi.Domain.Entities;

public sealed class ChatMessage
{
    private ChatMessage()
    {
    }

    private ChatMessage(
        string channelId,
        Guid userId,
        string displayName,
        string content,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        ChannelId = channelId;
        UserId = userId;
        DisplayName = displayName;
        Content = content;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string ChannelId { get; private set; } = string.Empty;

    public Guid UserId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public static ChatMessage Create(
        string channelId,
        Guid userId,
        string displayName,
        string content,
        DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelId);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        var normalizedContent = content.Trim();
        var normalizedDisplayName = displayName.Trim();

        if (channelId.Length > 100)
        {
            throw new ArgumentException("Channel ID cannot exceed 100 characters.", nameof(channelId));
        }

        if (normalizedDisplayName.Length > 40)
        {
            throw new ArgumentException("Display name cannot exceed 40 characters.", nameof(displayName));
        }

        if (normalizedContent.Length > 500)
        {
            throw new ArgumentException("Message cannot exceed 500 characters.", nameof(content));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("A valid user ID is required.", nameof(userId));
        }

        return new ChatMessage(channelId.Trim(), userId, normalizedDisplayName, normalizedContent, createdAt);
    }
}
