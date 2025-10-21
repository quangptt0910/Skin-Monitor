using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using SkinMonitor.Models;
using SkinMonitor.Services;

namespace SkinMonitor.ViewModels;

public partial class AnalysisPageViewModel : ObservableObject
{
    private readonly IWoundRepository _woundRepository;
    private readonly IAIAnalysisService _aiAnalysisService;
    private NavigationViewModel _navigationViewModel;

    [ObservableProperty]
    private ObservableCollection<Wound> woundsWithPhotos = new();

    [ObservableProperty]
    private Wound? selectedWound;

    [ObservableProperty]
    private WoundAnalysisResult? analysisResult;

    [ObservableProperty]
    private bool isAnalyzing;

    [ObservableProperty]
    private bool hasAnalysisResult;

    [ObservableProperty]
    private string statusMessage = "Select a wound to analyze";

    public NavigationViewModel NavigationViewModel => _navigationViewModel;

    public IAsyncRelayCommand LoadWoundsCommand { get; }
    public IAsyncRelayCommand<Wound> SelectWoundCommand { get; }
    public IAsyncRelayCommand AnalyzeWoundCommand { get; }

    public AnalysisPageViewModel(IWoundRepository woundRepository, IAIAnalysisService aiAnalysisService, NavigationViewModel navigationViewModel)
    {
        _woundRepository = woundRepository ?? throw new ArgumentNullException(nameof(woundRepository));
        _aiAnalysisService = aiAnalysisService ?? throw new ArgumentNullException(nameof(aiAnalysisService));
        _navigationViewModel = navigationViewModel ?? throw new ArgumentNullException(nameof(navigationViewModel));

        LoadWoundsCommand = new AsyncRelayCommand(LoadWoundsAsync);
        SelectWoundCommand = new AsyncRelayCommand<Wound>(SelectWoundAsync);
        AnalyzeWoundCommand = new AsyncRelayCommand(AnalyzeSelectedWoundAsync, () => SelectedWound != null && !IsAnalyzing);

        _navigationViewModel.UpdateCurrentPage("Analysis");
    }

    private async Task LoadWoundsAsync()
    {
        try
        {
            StatusMessage = "Loading wounds...";
            var wounds = await _woundRepository.GetAllWoundsAsync();
            
            // Only show wounds that have photos
            var woundsWithPhotos = wounds.Where(w => w.Photos?.Any() == true).ToList();
            
            WoundsWithPhotos.Clear();
            foreach (var wound in woundsWithPhotos)
            {
                WoundsWithPhotos.Add(wound);
            }

            StatusMessage = WoundsWithPhotos.Any() 
                ? "Select a wound to analyze" 
                : "No wounds with photos available";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading wounds: {ex.Message}";
        }
    }

    private async Task SelectWoundAsync(Wound wound)
    {
        try
        {
            SelectedWound = wound;
            AnalysisResult = null;
            HasAnalysisResult = false;
            
            if (wound != null)
            {
                StatusMessage = $"Selected: {wound.Name} - Ready to analyze";
                AnalyzeWoundCommand.NotifyCanExecuteChanged();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error selecting wound: {ex.Message}";
        }
    }

    private async Task AnalyzeSelectedWoundAsync()
    {
        if (SelectedWound?.Photos?.Any() != true)
        {
            StatusMessage = "Selected wound has no photos to analyze";
            return;
        }

        try
        {
            IsAnalyzing = true;
            StatusMessage = "Running AI analysis...";

            // Get the latest photo for analysis
            var latestPhoto = SelectedWound.Photos
                .OrderByDescending(p => p.DateTaken)
                .FirstOrDefault();

            if (latestPhoto?.PhotoPath == null)
            {
                StatusMessage = "No valid photo path found";
                return;
            }

            // Run AI analysis
            var result = await _aiAnalysisService.AnalyzeWoundAsync(
                SelectedWound.Id, 
                SelectedWound.Photos.ToList(), 
                latestPhoto.PhotoPath);

            // Save analysis results to the photo record
            latestPhoto.WoundAreaCm2 = result.WoundAreaCm2;
            latestPhoto.InfectionRiskScore = result.InfectionRisk.RiskScore;
            latestPhoto.HealingProgressScore = result.ConfidenceScore;
            latestPhoto.AIClassification = result.Classification.PrimaryType.ToString();
            latestPhoto.AIDetectedIssues = string.Join(", ", result.InfectionRisk.RiskFactors);

            // Update photo in database
            await _woundRepository.UpdateWoundAsync(SelectedWound);

            AnalysisResult = result;
            HasAnalysisResult = true;
            StatusMessage = "Analysis completed successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Analysis failed: {ex.Message}";
            AnalysisResult = null;
            HasAnalysisResult = false;
        }
        finally
        {
            IsAnalyzing = false;
            AnalyzeWoundCommand.NotifyCanExecuteChanged();
        }
    }

    public async Task InitializeAsync()
    {
        await LoadWoundsAsync();
    }
}
