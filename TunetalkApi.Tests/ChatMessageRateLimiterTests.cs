using TunetalkApi.Api.Hubs;

namespace TunetalkApi.Tests;

public sealed class ChatMessageRateLimiterTests
{
    [Fact]
    public void Allows_five_messages_per_user_in_a_ten_second_window()
    {
        var limiter = new InMemoryChatMessageRateLimiter(TimeProvider.System);
        var userId = Guid.NewGuid();

        for (var index = 0; index < 5; index++)
        {
            Assert.True(limiter.TryAcquire(userId));
        }

        Assert.False(limiter.TryAcquire(userId));
        Assert.True(limiter.TryAcquire(Guid.NewGuid()));
    }
}
