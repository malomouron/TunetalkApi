namespace TunetalkApi.Api.Hubs;

public interface IChatMessageRateLimiter
{
    bool TryAcquire(Guid userId);
}
