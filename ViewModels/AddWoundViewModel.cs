using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using SkinMonitor.Models;
using SkinMonitor.Services;

namespace SkinMonitor.ViewModels;

public partial class AddWoundViewModel : BaseViewModel
{
    private readonly IWoundRepository _woundRepository;
    private readonly IAIAnalysisService _aiAnalysisService;

    // Backing store for wound types enum list
    private readonly List<WoundType> _woundTypes = new(Enum.GetValues<WoundType>());

    // Properties with source generators
    public List<WoundType> WoundTypes => _woundTypes;

    [ObservableProperty]
    private WoundType _selectedWoundType;

    [ObservableProperty]
    private string _aISuggestionSummary = string.Empty;

    [ObservableProperty]
    private string _woundName = string.Empty;

    [ObservableProperty]
    private string _bodyLocation = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private string? _photoPath;

    [ObservableProperty]
    private bool _isPhotoPreviewVisible;
    
    
    // Commands
    public IAsyncRelayCommand PickPhotoCommand { get; }
    public IAsyncRelayCommand CapturePhotoCommand { get; }
    public IAsyncRelayCommand SaveWoundCommand { get; }
    public IAsyncRelayCommand CancelCommand { get; }
    public IAsyncRelayCommand SuggestWoundTypeCommand { get; }
    
    

    public AddWoundViewModel(IWoundRepository woundRepository, IAIAnalysisService aiAnalysisService)
    {
        _woundRepository = woundRepository;
        _aiAnalysisService = aiAnalysisService;
        _selectedWoundType = WoundType.Unknown;
        
        // Initialize commands with method bindings
        PickPhotoCommand = new AsyncRelayCommand(PickPhotoAsync);
        CapturePhotoCommand = new AsyncRelayCommand(CapturePhotoAsync);
        SaveWoundCommand = new AsyncRelayCommand(SaveWoundAsync);
        CancelCommand = new AsyncRelayCommand(CancelAsync);
        SuggestWoundTypeCommand = new AsyncRelayCommand(SuggestWoundTypeAsync);
    }
    
    partial void OnPhotoPathChanged(string? oldValue, string? newValue)
    {
        IsPhotoPreviewVisible = !string.IsNullOrEmpty(newValue);
    }

    private async Task PickPhotoAsync()
    {
        try
        {
            var photoResult = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Pick a wound photo"
            });

            if (photoResult != null) PhotoPath = photoResult.FullPath;
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

    private async Task SuggestWoundTypeAsync()
    {
        if (string.IsNullOrEmpty(PhotoPath))
        {
            AISuggestionSummary = "Please pick or capture a photo first.";
            return;
        }

        AISuggestionSummary = "Analyzing photo…";

        try
        {
            var classification = await _aiAnalysisService.ClassifyWoundTypeAsync(PhotoPath);

            if (classification == null || classification.PrimaryType == WoundType.Unknown)
            {
                AISuggestionSummary = "AI could not classify this wound.";
                return;
            }

            SelectedWoundType = classification.PrimaryType;
            AISuggestionSummary = $"AI suggests: {classification.PrimaryType} ({classification.Confidence:P1})";
        }
        catch (Exception ex)
        {
            AISuggestionSummary = $"AI analysis failed: {ex.Message}";
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
        using var original = SkiaSharp.SKBitmap.Decode(originalImagePath);

        if (original == null)
            return string.Empty;

        var scale = Math.Min(100f / original.Width, 100f / original.Height);
        int width = (int)(original.Width * scale);
        int height = (int)(original.Height * scale);

        using var resized = original.Resize(new SkiaSharp.SKImageInfo(width, height), SkiaSharp.SKFilterQuality.High);

        if (resized == null)
            return string.Empty;

        using var image = SkiaSharp.SKImage.FromBitmap(resized);
        using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Jpeg, 80);

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
