// Services/WoundRepository.cs
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using SkinMonitor.Models;
using SkinMonitor.Data;

namespace SkinMonitor.Services;

public interface IWoundRepository
{
    Task<List<Wound>> GetAllWoundsAsync();
    Task<List<Wound>> GetActiveWoundsAsync();
    Task<Wound?> GetWoundByIdAsync(int id);
    Task<Wound> AddWoundAsync(Wound wound);
    Task<Wound> UpdateWoundAsync(Wound wound);
    Task DeleteWoundAsync(int id);
    Task<WoundPhoto> AddPhotoAsync(WoundPhoto photo);
    Task<List<WoundPhoto>> GetWoundPhotosAsync(int woundId);
    Task<HealingStageLog> AddHealingLogAsync(HealingStageLog log);
    Task<List<HealingStageLog>> GetHealingLogsAsync(int woundId);
}

public class WoundRepository : IWoundRepository
{
    private readonly IDatabaseService _databaseService;

    public WoundRepository(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<List<Wound>> GetAllWoundsAsync()
    {
        using var context = await _databaseService.GetDbContextAsync();
        return await context.Wounds
            .Include(w => w.Photos)
            .Include(w => w.HealingLogs)
            .OrderByDescending(w => w.DateCreated)
            .ToListAsync();
    }

    public async Task<List<Wound>> GetActiveWoundsAsync()
    {
        using var context = await _databaseService.GetDbContextAsync();
        return await context.Wounds
            .Include(w => w.Photos)
            .Include(w => w.HealingLogs)
            .Where(w => w.IsActive)
            .OrderByDescending(w => w.DateCreated)
            .ToListAsync();
    }

    public async Task<Wound?> GetWoundByIdAsync(int id)
    {
        using var context = await _databaseService.GetDbContextAsync();
        return await context.Wounds
            .Include(w => w.Photos)
            .Include(w => w.HealingLogs)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<Wound> AddWoundAsync(Wound wound)
    {
        using var context = await _databaseService.GetDbContextAsync();
        context.Wounds.Add(wound);
        await context.SaveChangesAsync();
        return wound;
    }

    public async Task<Wound> UpdateWoundAsync(Wound wound)
    {
        using var context = await _databaseService.GetDbContextAsync();
        context.Wounds.Update(wound);
        await context.SaveChangesAsync();
        return wound;
    }

    public async Task DeleteWoundAsync(int id)
    {
        using var context = await _databaseService.GetDbContextAsync();
        var wound = await context.Wounds.FindAsync(id);
        if (wound != null)
        {
            context.Wounds.Remove(wound);
            await context.SaveChangesAsync();
        }
    }

    public async Task<WoundPhoto> AddPhotoAsync(WoundPhoto photo)
    {
        using var context = await _databaseService.GetDbContextAsync();
        context.WoundPhotos.Add(photo);
        await context.SaveChangesAsync();
        return photo;
    }

    public async Task<List<WoundPhoto>> GetWoundPhotosAsync(int woundId)
    {
        using var context = await _databaseService.GetDbContextAsync();
        return await context.WoundPhotos
            .Where(p => p.WoundId == woundId)
            .OrderByDescending(p => p.DateTaken)
            .ToListAsync();
    }

    public async Task<HealingStageLog> AddHealingLogAsync(HealingStageLog log)
    {
        using var context = await _databaseService.GetDbContextAsync();
        context.HealingLogs.Add(log);
        await context.SaveChangesAsync();
        return log;
    }

    public async Task<List<HealingStageLog>> GetHealingLogsAsync(int woundId)
    {
        using var context = await _databaseService.GetDbContextAsync();
        return await context.HealingLogs
            .Where(l => l.WoundId == woundId)
            .OrderByDescending(l => l.Date)
            .ToListAsync();
    }
}
