namespace TunetalkApi.Api.Hubs;

public sealed record ChannelPresence(string ChannelId, int ConnectionCount);

public sealed record ChannelJoinResult(string? PreviousChannelId, int ConnectionCount);

public interface IChannelPresenceTracker
{
    ChannelJoinResult Join(string connectionId, string channelId);

    ChannelPresence? Leave(string connectionId, string? expectedChannelId = null);

    bool IsInChannel(string connectionId, string channelId);

    int Count(string channelId);
}
