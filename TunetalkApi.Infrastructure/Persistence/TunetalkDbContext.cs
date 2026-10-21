using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TunetalkApi.Domain.Entities;
using TunetalkApi.Infrastructure.Identity;

namespace TunetalkApi.Infrastructure.Persistence;

public sealed class TunetalkDbContext(
    DbContextOptions<TunetalkDbContext> options)
    : IdentityDbContext<TunetalkUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TunetalkUser>()
            .Property(user => user.DisplayName)
            .HasMaxLength(40)
            .IsRequired();

        builder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(message => message.Id);
            entity.Property(message => message.ChannelId).HasMaxLength(100).IsRequired();
            entity.Property(message => message.DisplayName).HasMaxLength(40).IsRequired();
            entity.Property(message => message.Content).HasMaxLength(500).IsRequired();
            entity.Property(message => message.CreatedAt)
                .HasConversion(new ValueConverter<DateTimeOffset, long>(
                    value => value.ToUnixTimeMilliseconds(),
                    value => DateTimeOffset.FromUnixTimeMilliseconds(value)));
            entity.HasIndex(message => new { message.ChannelId, message.CreatedAt });
        });
    }
}
