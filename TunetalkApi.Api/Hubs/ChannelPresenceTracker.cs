namespace TunetalkApi.Api.Hubs;

public sealed class ChannelPresenceTracker : IChannelPresenceTracker
{
    private readonly object _sync = new();
    private readonly Dictionary<string, string> _channelByConnection = new(StringComparer.Ordinal);
    private readonly Dictionary<string, HashSet<string>> _connectionsByChannel = new(StringComparer.Ordinal);

    public ChannelJoinResult Join(string connectionId, string channelId)
    {
        lock (_sync)
        {
            _channelByConnection.TryGetValue(connectionId, out var previousChannelId);
            if (string.Equals(previousChannelId, channelId, StringComparison.Ordinal))
            {
                return new ChannelJoinResult(previousChannelId, _connectionsByChannel[channelId].Count);
            }

            RemoveConnectionFromChannel(connectionId, previousChannelId);
            _channelByConnection[connectionId] = channelId;

            if (!_connectionsByChannel.TryGetValue(channelId, out var connections))
            {
                connections = new HashSet<string>(StringComparer.Ordinal);
                _connectionsByChannel.Add(channelId, connections);
            }

            connections.Add(connectionId);
            return new ChannelJoinResult(previousChannelId, connections.Count);
        }
    }

    public ChannelPresence? Leave(string connectionId, string? expectedChannelId = null)
    {
        lock (_sync)
        {
            if (!_channelByConnection.TryGetValue(connectionId, out var channelId)
                || (expectedChannelId is not null
                    && !string.Equals(channelId, expectedChannelId, StringComparison.Ordinal)))
            {
                return null;
            }

            _channelByConnection.Remove(connectionId);
            var remaining = RemoveConnectionFromChannel(connectionId, channelId);
            return new ChannelPresence(channelId, remaining);
        }
    }

    public bool IsInChannel(string connectionId, string channelId)
    {
        lock (_sync)
        {
            return _channelByConnection.TryGetValue(connectionId, out var currentChannelId)
                && string.Equals(currentChannelId, channelId, StringComparison.Ordinal);
        }
    }

    public int Count(string channelId)
    {
        lock (_sync)
        {
            return _connectionsByChannel.TryGetValue(channelId, out var connections)
                ? connections.Count
                : 0;
        }
    }

    private int RemoveConnectionFromChannel(string connectionId, string? channelId)
    {
        if (channelId is null || !_connectionsByChannel.TryGetValue(channelId, out var connections))
        {
            return 0;
        }

        connections.Remove(connectionId);
        if (connections.Count == 0)
        {
            _connectionsByChannel.Remove(channelId);
        }

        return connections.Count;
    }
}
