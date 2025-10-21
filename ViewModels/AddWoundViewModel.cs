using System;
using System.Windows.Input;
using System.Threading.Tasks;
using SkinMonitor.Models;
using SkinMonitor.Services;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SkinMonitor.ViewModels;


public class AddWoundViewModel : BaseViewModel
{
    private readonly IWoundRepository _woundRepository;
    
    private string _woundName = string.Empty;
    

    public string WoundName
    {
        get => _woundName;
        set => SetProperty(ref _woundName, value);
    }

    private string _bodyLocation = string.Empty;
    public string BodyLocation
    {
        get => _bodyLocation;
        set => SetProperty(ref _bodyLocation, value);
    }
    
    private readonly List<WoundType> _woundTypes = new List<WoundType>
    {
        WoundType.Surgical,
        WoundType.Burn,
        WoundType.Diabetic,
        WoundType.Traumatic,
        WoundType.Pressure,
        WoundType.Other
    };
    
    public List<WoundType> WoundTypes
    {
        get => _woundTypes;
        // Usually set only once so no setter needed or implement setter if you want notifications
    }
    
    private WoundType _selectedWoundType;
    public WoundType SelectedWoundType
    {
        get => _selectedWoundType;
        set => SetProperty(ref _selectedWoundType, value);
    }

    private string _notes = string.Empty;
    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    private string? _photoPath;
    public string? PhotoPath
    {
        get => _photoPath;
        set => SetProperty(ref _photoPath, value);
    }

    public ICommand PickPhotoCommand { get; }
    public ICommand CapturePhotoCommand { get; }
    public ICommand SaveWoundCommand { get; }
    public ICommand CancelCommand { get; }

    public AddWoundViewModel(IWoundRepository woundRepository)
    {
        _woundRepository = woundRepository;

        PickPhotoCommand = new Command(async () => await PickPhotoAsync());
        CapturePhotoCommand = new Command(async () => await CapturePhotoAsync());
        SaveWoundCommand = new Command(async () => await SaveWoundAsync());
        CancelCommand = new Command(async () => await CancelAsync());
    }

    private async Task PickPhotoAsync()
    {
        try
        {
            var photoResult = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Pick a wound photo"
            });

            if (photoResult != null)
            {
                var filePath = photoResult.FullPath;
                PhotoPath = filePath;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Error", $"Photo picking failed: {ex.Message}", "OK");
        }
    }

    private async Task CapturePhotoAsync()
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo != null)
                {
                    var fileName = $"wound_{DateTime.UtcNow:yyyyMMddHHmmss}.jpg";
                    var localFolder = Path.Combine(FileSystem.AppDataDirectory, "Photos");
                    Directory.CreateDirectory(localFolder);
                    var localFilePath = Path.Combine(localFolder, fileName);

                    using var sourceStream = await photo.OpenReadAsync();
                    using var localFileStream = File.Create(localFilePath);
                    await sourceStream.CopyToAsync(localFileStream);

                    PhotoPath = localFilePath;
                }
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Unsupported", "Camera capture is not supported on this device.", "OK");
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Error", $"Photo capture failed: {ex.Message}", "OK");
        }
    }

    private async Task SaveWoundAsync()
    {
        if (string.IsNullOrWhiteSpace(WoundName))
        {
            await App.Current.MainPage.DisplayAlert("Validation Error", "Please enter a wound name.", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(BodyLocation))
        {
            await App.Current.MainPage.DisplayAlert("Validation Error", "Please enter a body location.", "OK");
            return;
        }
        if (SelectedWoundType == default)
        {
            await App.Current.MainPage.DisplayAlert("Validation Error", "Please select a wound type.", "OK");
            return;
        }

        try
        {
            var wound = new Wound
            {
                Name = WoundName.Trim(),
                BodyLocation = BodyLocation.Trim(),
                Type = SelectedWoundType,
                DateCreated = DateTime.UtcNow,
                IsActive = true,
                Notes = Notes?.Trim()
            };

            var savedWound = await _woundRepository.AddWoundAsync(wound);

            if (!string.IsNullOrEmpty(PhotoPath))
            {
                var thumbnailPath = await GenerateThumbnailAsync(PhotoPath);

                var woundPhoto = new WoundPhoto
                {
                    WoundId = savedWound.Id,
                    PhotoPath = PhotoPath,
                    ThumbnailPath = thumbnailPath,
                    DateTaken = DateTime.UtcNow
                };

                await _woundRepository.AddPhotoAsync(woundPhoto);
            }

            await App.Current.MainPage.DisplayAlert("Success", "Wound saved successfully!", "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Error", $"Failed to save wound: {ex.Message}", "OK");
        }
    }

    private async Task<string> GenerateThumbnailAsync(string originalImagePath)
    {
        // Load the original image using SkiaSharp
        using var original = SkiaSharp.SKBitmap.Decode(originalImagePath);

        if (original == null)
            return string.Empty;

        // Resize to thumbnail size, e.g., 100x100
        var scale = Math.Min(100f / original.Width, 100f / original.Height);

        int width = (int)(original.Width * scale);
        int height = (int)(original.Height * scale);

        using var resized = original.Resize(new SkiaSharp.SKImageInfo(width, height), SkiaSharp.SKFilterQuality.High);

        if (resized == null)
            return string.Empty;

        using var image = SkiaSharp.SKImage.FromBitmap(resized);
        using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Jpeg, 80);

        // Save thumbnail file
        var thumbDir = Path.Combine(FileSystem.AppDataDirectory, "Thumbnails");
        Directory.CreateDirectory(thumbDir);
        var thumbPath = Path.Combine(thumbDir, Path.GetFileName(originalImagePath));

        using var stream = File.OpenWrite(thumbPath);
        data.SaveTo(stream);

        return thumbPath;
    }

    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}