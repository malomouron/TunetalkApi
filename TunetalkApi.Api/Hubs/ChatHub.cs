using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TunetalkApi.Application.Channels;
using TunetalkApi.Application.Chat;

namespace TunetalkApi.Api.Hubs;

[Authorize]
public sealed class ChatHub(
    ChatMessageService chatMessageService,
    IRadioChannelCatalog channelCatalog,
    IChannelPresenceTracker presenceTracker,
    IChatMessageRateLimiter chatMessageRateLimiter,
    ILogger<ChatHub> logger) : Hub
{
    public async Task JoinChannel(string channelId)
    {
        if (!Guid.TryParse(channelId, out _))
        {
            throw new HubException("A radio station UUID is required.");
        }

        var channel = await channelCatalog.GetByIdAsync(channelId, Context.ConnectionAborted);
        if (channel is null)
        {
            throw new HubException("This radio channel is not available.");
        }

        var join = presenceTracker.Join(Context.ConnectionId, channelId);
        if (join.PreviousChannelId is not null
            && !string.Equals(join.PreviousChannelId, channelId, StringComparison.Ordinal))
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                GroupName(join.PreviousChannelId),
                Context.ConnectionAborted);
            await Clients.Group(GroupName(join.PreviousChannelId))
                .SendAsync(
                    "PresenceUpdated",
                    join.PreviousChannelId,
                    presenceTracker.Count(join.PreviousChannelId),
                    Context.ConnectionAborted);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(channelId), Context.ConnectionAborted);
        await Clients.Group(GroupName(channelId))
            .SendAsync("PresenceUpdated", channelId, join.ConnectionCount, Context.ConnectionAborted);
    }

    public async Task LeaveChannel(string channelId)
    {
        if (!Guid.TryParse(channelId, out _))
        {
            throw new HubException("A radio station UUID is required.");
        }

        var presence = presenceTracker.Leave(Context.ConnectionId, channelId);
        if (presence is null)
        {
            return;
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(channelId), Context.ConnectionAborted);
        await Clients.Group(GroupName(channelId))
            .SendAsync("PresenceUpdated", channelId, presence.ConnectionCount, Context.ConnectionAborted);
    }

    public async Task SendMessage(string channelId, string content)
    {
        if (!Guid.TryParse(channelId, out _))
        {
            throw new HubException("A radio station UUID is required.");
        }

        if (!presenceTracker.IsInChannel(Context.ConnectionId, channelId))
        {
            throw new HubException("Join the channel before sending a message.");
        }

        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var displayName = Context.User?.FindFirstValue(ClaimTypes.Name);
        if (!Guid.TryParse(userId, out var parsedUserId) || string.IsNullOrWhiteSpace(displayName))
        {
            throw new HubException("The authenticated user profile is unavailable.");
        }

        if (!chatMessageRateLimiter.TryAcquire(parsedUserId))
        {
            throw new HubException("Vous envoyez des messages trop rapidement. Réessayez dans quelques secondes.");
        }

        ChatMessageDto message;
        try
        {
            message = await chatMessageService.SendAsync(
                channelId,
                parsedUserId,
                displayName,
                content,
                Context.ConnectionAborted);
        }
        catch (ArgumentException exception)
        {
            throw new HubException(exception.Message);
        }

        await Clients.Group(GroupName(channelId))
            .SendAsync(
                "ReceiveMessage",
                message.Id,
                message.ChannelId,
                message.DisplayName,
                message.Content,
                message.CreatedAt.ToString("O", CultureInfo.InvariantCulture),
                Context.ConnectionAborted);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var presence = presenceTracker.Leave(Context.ConnectionId);
        if (presence is not null)
        {
            try
            {
                await Clients.Group(GroupName(presence.ChannelId))
                    .SendAsync(
                        "PresenceUpdated",
                        presence.ChannelId,
                        presence.ConnectionCount,
                        CancellationToken.None);
            }
            catch (Exception sendException) when (sendException is IOException or OperationCanceledException)
            {
                logger.LogDebug(sendException, "Unable to broadcast a final listener count.");
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    private static string GroupName(string channelId) => $"channel:{channelId}";
}
