// Services/PhotoService.cs
using Microsoft.Maui.Media;
using SkiaSharp;

namespace SkinMonitor.Services;

public interface IPhotoService
{
    Task<string?> CapturePhotoAsync();
    Task<string?> SelectPhotoAsync();
    Task<string> SavePhotoAsync(Stream photoStream, string fileName);
    Task<string> CreateThumbnailAsync(string originalPath);
    Task DeletePhotoAsync(string photoPath);
    Task<bool> ValidatePhotoAsync(string photoPath);
}

public class PhotoService : IPhotoService
{
    private readonly string _photosDirectory;

    public PhotoService()
    {
        _photosDirectory = Path.Combine(FileSystem.AppDataDirectory, "Photos");
        Directory.CreateDirectory(_photosDirectory);
    }

    public async Task<string?> CapturePhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Capture Wound Photo"
            });

            if (photo != null)
            {
                var fileName = $"wound_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                return await SavePhotoFromResultAsync(photo, fileName);
            }

            return null;
        }
        catch (Exception ex)
        {
            // Log exception
            System.Diagnostics.Debug.WriteLine($"Error capturing photo: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> SelectPhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select Wound Photo"
            });

            if (photo != null)
            {
                var fileName = $"wound_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                return await SavePhotoFromResultAsync(photo, fileName);
            }

            return null;
        }
        catch (Exception ex)
        {
            // Log exception
            System.Diagnostics.Debug.WriteLine($"Error selecting photo: {ex.Message}");
            return null;
        }
    }

    private async Task<string> SavePhotoFromResultAsync(FileResult photo, string fileName)
    {
        using var stream = await photo.OpenReadAsync();
        return await SavePhotoAsync(stream, fileName);
    }

    public async Task<string> SavePhotoAsync(Stream photoStream, string fileName)
    {
        var filePath = Path.Combine(_photosDirectory, fileName);
        
        using var fileStream = File.Create(filePath);
        await photoStream.CopyToAsync(fileStream);
        
        return filePath;
    }

    public async Task<string> CreateThumbnailAsync(string originalPath)
    {
        var thumbnailPath = Path.Combine(
            _photosDirectory, 
            $"thumb_{Path.GetFileName(originalPath)}"
        );

        using var original = SKBitmap.Decode(originalPath);
        var thumbnailSize = 200;
        var aspectRatio = (float)original.Width / original.Height;
        
        int thumbnailWidth, thumbnailHeight;
        if (aspectRatio > 1)
        {
            thumbnailWidth = thumbnailSize;
            thumbnailHeight = (int)(thumbnailSize / aspectRatio);
        }
        else
        {
            thumbnailWidth = (int)(thumbnailSize * aspectRatio);
            thumbnailHeight = thumbnailSize;
        }

        using var thumbnail = original.Resize(new SKImageInfo(thumbnailWidth, thumbnailHeight), SKFilterQuality.High);
        using var image = SKImage.FromBitmap(thumbnail);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 80);
        
        await File.WriteAllBytesAsync(thumbnailPath, data.ToArray());
        
        return thumbnailPath;
    }

    public async Task DeletePhotoAsync(string photoPath)
    {
        try
        {
            if (File.Exists(photoPath))
            {
                File.Delete(photoPath);
                
                // Also delete thumbnail if exists
                var thumbnailPath = Path.Combine(
                    Path.GetDirectoryName(photoPath)!, 
                    $"thumb_{Path.GetFileName(photoPath)}"
                );
                
                if (File.Exists(thumbnailPath))
                {
                    File.Delete(thumbnailPath);
                }
            }
        }
        catch (Exception ex)
        {
            // Log exception
            System.Diagnostics.Debug.WriteLine($"Error deleting photo: {ex.Message}");
        }
    }

    public async Task<bool> ValidatePhotoAsync(string photoPath)
    {
        try
        {
            if (!File.Exists(photoPath))
                return false;

            using var bitmap = SKBitmap.Decode(photoPath);
            return bitmap != null && bitmap.Width > 0 && bitmap.Height > 0;
        }
        catch
        {
            return false;
        }
    }
}