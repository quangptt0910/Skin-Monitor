namespace SkinMonitor.Data;

using Microsoft.EntityFrameworkCore;
using SkinMonitor.Models;
using SQLitePCL;

public class AppDbContext : DbContext
{
    public DbSet<Wound> Wounds { get; set; }
    public DbSet<WoundPhoto> WoundPhotos { get; set; }
    public DbSet<HealingStageLog> HealingLogs { get; set; }
    
    public AppDbContext()
    {
        SQLitePCL.Batteries_V2.Init();
        Database.EnsureCreated();
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(
            FileSystem.AppDataDirectory, 
            "woundtracker.db"
        );
        optionsBuilder.UseSqlite($"Filename={dbPath}");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Wound>()
            .HasMany(w => w.Photos)
            .WithOne(p => p.Wound)
            .HasForeignKey(p => p.WoundId);
            
        modelBuilder.Entity<Wound>()
            .HasMany(w => w.HealingLogs)
            .WithOne(h => h.Wound)
            .HasForeignKey(h => h.WoundId);
    }
}