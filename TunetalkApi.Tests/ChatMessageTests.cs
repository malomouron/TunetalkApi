using TunetalkApi.Domain.Entities;

namespace TunetalkApi.Tests;

public sealed class ChatMessageTests
{
    [Fact]
    public void Create_trims_user_text_and_preserves_channel_identity()
    {
        var userId = Guid.NewGuid();
        var createdAt = DateTimeOffset.Parse("2026-10-21T07:00:00Z");

        var message = ChatMessage.Create(
            "7f3c7b1e-81a8-4b13-bc2a-1437a11c7a25",
            userId,
            "  Lou  ",
            "  Salut !  ",
            createdAt);

        Assert.Equal(userId, message.UserId);
        Assert.Equal("Lou", message.DisplayName);
        Assert.Equal("Salut !", message.Content);
        Assert.Equal(createdAt, message.CreatedAt);
    }

    [Fact]
    public void Create_rejects_empty_or_oversized_messages()
    {
        var userId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() =>
            ChatMessage.Create("station", userId, "Lou", "  ", createdAt));
        Assert.Throws<ArgumentException>(() =>
            ChatMessage.Create("station", userId, "Lou", new string('a', 501), createdAt));
    }
}
