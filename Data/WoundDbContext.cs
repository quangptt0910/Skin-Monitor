// Data/WoundDbContext.cs
using Microsoft.EntityFrameworkCore;
using SkinMonitor.Models;

namespace SkinMonitor.Data;

public class WoundDbContext : DbContext
{
    public DbSet<Wound> Wounds { get; set; }
    public DbSet<WoundPhoto> WoundPhotos { get; set; }
    public DbSet<HealingStageLog> HealingLogs { get; set; }

    public WoundDbContext() { }

    public WoundDbContext(DbContextOptions<WoundDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "wounds.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Wound entity
        modelBuilder.Entity<Wound>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.BodyLocation).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            
            entity.HasMany(e => e.Photos)
                .WithOne(e => e.Wound)
                .HasForeignKey(e => e.WoundId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.HealingLogs)
                .WithOne(e => e.Wound)
                .HasForeignKey(e => e.WoundId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure WoundPhoto entity
        modelBuilder.Entity<WoundPhoto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PhotoPath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ThumbnailPath).HasMaxLength(500);
            entity.Property(e => e.ReferenceObjectUsed).HasMaxLength(50);
            entity.Property(e => e.AIClassification).HasMaxLength(100);
            entity.Property(e => e.AIDetectedIssues).HasMaxLength(2000);
            entity.Property(e => e.Notes).HasMaxLength(1000);
        });

        // Configure HealingStageLog entity
        modelBuilder.Entity<HealingStageLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DrainageType).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(1000);
        });

        // Seed data for development
        // modelBuilder.Entity<Wound>().HasData(
        //     new Wound
        //     {
        //         Id = 1,
        //         Name = "Sample Surgical Incision",
        //         BodyLocation = "Right Abdomen",
        //         Type = WoundType.Surgical,
        //         DateCreated = DateTime.Now.AddDays(-7),
        //         IsActive = true,
        //         Notes = "Post-operative wound from appendectomy"
        //     }
        // );
    }
}
