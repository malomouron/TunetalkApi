using TunetalkApi.Api.Hubs;

namespace TunetalkApi.Tests;

public sealed class ChannelPresenceTrackerTests
{
    [Fact]
    public void Join_counts_connections_and_switching_channels_clears_previous_presence()
    {
        var tracker = new ChannelPresenceTracker();

        Assert.Equal(1, tracker.Join("connection-1", "channel-a").ConnectionCount);
        Assert.Equal(2, tracker.Join("connection-2", "channel-a").ConnectionCount);

        var move = tracker.Join("connection-1", "channel-b");

        Assert.Equal("channel-a", move.PreviousChannelId);
        Assert.Equal(1, tracker.Count("channel-a"));
        Assert.Equal(1, tracker.Count("channel-b"));
        Assert.False(tracker.IsInChannel("connection-1", "channel-a"));
        Assert.True(tracker.IsInChannel("connection-1", "channel-b"));
    }

    [Fact]
    public void Leave_ignores_a_different_channel_and_removes_the_current_connection()
    {
        var tracker = new ChannelPresenceTracker();
        tracker.Join("connection-1", "channel-a");

        Assert.Null(tracker.Leave("connection-1", "channel-b"));
        Assert.Equal(1, tracker.Count("channel-a"));

        var presence = tracker.Leave("connection-1");

        Assert.Equal("channel-a", presence?.ChannelId);
        Assert.Equal(0, presence?.ConnectionCount);
        Assert.Equal(0, tracker.Count("channel-a"));
    }
}
