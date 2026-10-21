using Microsoft.EntityFrameworkCore;
using TunetalkApi.Application.Chat;
using TunetalkApi.Domain.Entities;

namespace TunetalkApi.Infrastructure.Persistence;

public sealed class EfChatMessageRepository(TunetalkDbContext dbContext) : IChatMessageRepository
{
    public async Task<IReadOnlyList<ChatMessage>> GetRecentAsync(
        string channelId,
        int take,
        CancellationToken cancellationToken)
    {
        var newestFirst = await dbContext.ChatMessages
            .AsNoTracking()
            .Where(message => message.ChannelId == channelId)
            .OrderByDescending(message => message.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        newestFirst.Reverse();
        return newestFirst;
    }

    public async Task AddAsync(ChatMessage message, CancellationToken cancellationToken)
    {
        dbContext.ChatMessages.Add(message);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
