using System.Collections.Concurrent;

namespace TunetalkApi.Api.Hubs;

public sealed class InMemoryChatMessageRateLimiter(TimeProvider timeProvider) : IChatMessageRateLimiter
{
    private static readonly TimeSpan Window = TimeSpan.FromSeconds(10);
    private const int MessageLimit = 5;
    private readonly ConcurrentDictionary<Guid, Queue<DateTimeOffset>> _sentAtByUser = new();

    public bool TryAcquire(Guid userId)
    {
        var sentAt = _sentAtByUser.GetOrAdd(userId, static _ => new Queue<DateTimeOffset>());
        lock (sentAt)
        {
            var cutoff = timeProvider.GetUtcNow() - Window;
            while (sentAt.TryPeek(out var timestamp) && timestamp <= cutoff)
            {
                sentAt.Dequeue();
            }

            if (sentAt.Count >= MessageLimit)
            {
                return false;
            }

            sentAt.Enqueue(timeProvider.GetUtcNow());
            return true;
        }
    }
}
