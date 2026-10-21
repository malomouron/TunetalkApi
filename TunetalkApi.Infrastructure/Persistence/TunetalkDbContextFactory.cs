using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TunetalkApi.Infrastructure.Persistence;

public sealed class TunetalkDbContextFactory : IDesignTimeDbContextFactory<TunetalkDbContext>
{
    public TunetalkDbContext CreateDbContext(string[] args)
    {
        var connectionString = args.Length > 0 ? args[0] : "Data Source=tunetalk.db";
        var options = new DbContextOptionsBuilder<TunetalkDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new TunetalkDbContext(options);
    }
}
