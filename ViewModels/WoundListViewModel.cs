using System.Collections.ObjectModel;
using System.Windows.Input;
using SkinMonitor.Models;
using SkinMonitor.Services;

namespace SkinMonitor.ViewModels;

public class WoundListViewModel : BaseViewModel
{
    private readonly IWoundRepository _woundRepository;
    private readonly IAIAnalysisService _aiAnalysisService; 

    public ObservableCollection<Wound> Wounds { get; } = new();
    public ObservableCollection<Wound> ActiveWounds { get; } = new();

    public ICommand LoadWoundsCommand { get; }
    public ICommand AddWoundCommand { get; }
    public ICommand SelectWoundCommand { get; }
    public ICommand RefreshCommand { get; }

    public ICommand AnalyzeWoundCommand { get; }


    private double _woundArea;
    public double WoundArea
    {
        get => _woundArea;
        set => SetProperty(ref _woundArea, value);
    }

    private InfectionRiskAssessment _infectionRisk;
    public InfectionRiskAssessment InfectionRisk
    {
        get => _infectionRisk;
        set => SetProperty(ref _infectionRisk, value);
    }

    private WoundClassification _woundClassification;
    public WoundClassification WoundClassification
    {
        get => _woundClassification;
        set => SetProperty(ref _woundClassification, value);
    }
    
    public WoundListViewModel(IWoundRepository woundRepository, IAIAnalysisService aiAnalysisService)
    {
        _woundRepository = woundRepository;
        _aiAnalysisService = aiAnalysisService;

        Title = "My Wounds";
        
        // Data Commands
        LoadWoundsCommand = new Command(async () => await LoadWounds());
        AddWoundCommand = new Command(async () => await AddWound());
        SelectWoundCommand = new Command<Wound>(async (wound) => await SelectWound(wound));
        RefreshCommand = new Command(async () => await RefreshWounds());
        
        AnalyzeWoundCommand = new Command<Wound>(async wound => await AnalyzeWound(wound));

    }
    
    private async Task AnalyzeWound(Wound wound)
    {
        if (wound == null)
        {
            await ShowError("Analysis Error", "No wound selected for analysis");
            return;
        }

        try
        {
            // Example usage of AIAnalysisService - assuming wound has a photo path
            var primaryPhoto = wound.Photos?.FirstOrDefault();
            if (primaryPhoto == null)
            {
                await ShowError("Analysis Error", "No wound photo available");
                return;
            }
            
            var photos = wound.Photos?.ToList() ?? new List<WoundPhoto>();
            var result = await _aiAnalysisService.AnalyzeWoundAsync(wound.Id, photos, primaryPhoto.PhotoPath);

            // Use the result to update UI, e.g.
            // WoundArea, InfectionRiskLevel, Classification, etc.
            // You can implement property bindings or events for this

            // For now, just output to debug
            WoundArea = result.WoundAreaCm2;
            InfectionRisk = result.InfectionRisk;
            WoundClassification = result.Classification;
            
            System.Diagnostics.Debug.WriteLine($"AI Analysis Result for Wound {wound.Name}: Area={result.WoundAreaCm2}, InfectionRisk={result.InfectionRisk.RiskLevel}, Classification={result.Classification.PrimaryType}");
        }
        catch (Exception ex)
        {
            await ShowError("AI Analysis Error", ex.Message);
        }
    }
    
    #region Data Methods

    private async Task LoadWounds()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            
            var wounds = await _woundRepository.GetAllWoundsAsync();
            var activeWounds = wounds.Where(w => w.IsActive).ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Wounds.Clear();
                ActiveWounds.Clear();
                
                foreach (var wound in wounds)
                    Wounds.Add(wound);
                    
                foreach (var wound in activeWounds)
                    ActiveWounds.Add(wound);
            });
        }
        catch (Exception ex)
        {
            await ShowError("Failed to Load Wounds", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddWound()
    {
        try
        {
            await Shell.Current.GoToAsync("AddWound");
        }
        catch (Exception ex)
        {
            await ShowError("Navigation Error", $"Failed to navigate to Add Wound page: {ex.Message}");
        }
    }

    private async Task SelectWound(Wound wound)
    {
        if (wound == null) return;
        
        try
        {
            var route = $"WoundDetail?woundId={wound.Id}";
            await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            await ShowError("Navigation Error", $"Failed to open wound details: {ex.Message}");
        }
    }

    private async Task RefreshWounds()
    {
        await LoadWounds();
    }

    #endregion

    #region Helper Methods

    private async Task ShowError(string title, string message)
    {
        try
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert(title, message, "OK");
            }
        }
        catch
        {
            // Fallback if Shell is not available
            System.Diagnostics.Debug.WriteLine($"{title}: {message}");
        }
    }

    #endregion

    #region Public Methods

    public async Task Initialize()
    {
        await LoadWounds();
    }

    /// <summary>
    /// Deletes a wound from the database
    /// </summary>
    public async Task DeleteWound(Wound wound)
    {
        if (wound == null) return;

        try
        {
            var confirm = await Shell.Current.DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete '{wound.Name}'?",
                "Delete",
                "Cancel");

            if (confirm)
            {
                await _woundRepository.DeleteWoundAsync(wound.Id);
                await LoadWounds(); // Refresh the list
                
                await Shell.Current.DisplayAlert(
                    "Success",
                    "Wound deleted successfully.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await ShowError("Delete Error", $"Failed to delete wound: {ex.Message}");
        }
    }

    /// <summary>
    /// Marks a wound as healed/inactive
    /// </summary>
    public async Task MarkAsHealed(Wound wound)
    {
        if (wound == null) return;

        try
        {
            wound.IsActive = false;
            wound.DateHealed = DateTime.Now;
            
            await _woundRepository.UpdateWoundAsync(wound);
            await LoadWounds(); // Refresh the list
            
            await Shell.Current.DisplayAlert(
                "Success",
                $"'{wound.Name}' marked as healed!",
                "OK");
        }
        catch (Exception ex)
        {
            await ShowError("Update Error", $"Failed to update wound: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets statistics about wounds
    /// </summary>
    public WoundStatistics GetStatistics()
    {
        return new WoundStatistics
        {
            TotalWounds = Wounds.Count,
            ActiveWounds = ActiveWounds.Count,
            HealedWounds = Wounds.Count(w => !w.IsActive),
            TotalPhotos = Wounds.Sum(w => w.Photos?.Count ?? 0),
            AverageHealingDays = CalculateAverageHealingDays()
        };
    }

    private double CalculateAverageHealingDays()
    {
        var healedWounds = Wounds.Where(w => !w.IsActive && w.DateHealed.HasValue).ToList();
        
        if (!healedWounds.Any())
            return 0;

        var totalDays = healedWounds.Sum(w => 
            (w.DateHealed!.Value - w.DateCreated).TotalDays);
        
        return totalDays / healedWounds.Count;
    }

    #endregion
}

/// <summary>
/// Statistics model for wound data
/// </summary>
public class WoundStatistics
{
    public int TotalWounds { get; set; }
    public int ActiveWounds { get; set; }
    public int HealedWounds { get; set; }
    public int TotalPhotos { get; set; }
    public double AverageHealingDays { get; set; }
}
