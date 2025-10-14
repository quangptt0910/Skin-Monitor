using Microsoft.EntityFrameworkCore;
using SkinMonitor.Data;


namespace SkinMonitor.Services;

public interface IDatabaseService
{
    Task InitializeAsync();
    Task<WoundDbContext> GetDbContextAsync();
    Task EnsureDatabaseCreatedAsync();
}

public class DatabaseService : IDatabaseService
{
    public async Task InitializeAsync()
    {
        using var context = new WoundDbContext();
        await context.Database.EnsureCreatedAsync();
        
        // Run migrations if needed
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            await context.Database.MigrateAsync();
        }
    }

    public async Task<WoundDbContext> GetDbContextAsync()
    {
        var context = new WoundDbContext();
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    public async Task EnsureDatabaseCreatedAsync()
    {
        using var context = new WoundDbContext();
        await context.Database.EnsureCreatedAsync();
    }
}