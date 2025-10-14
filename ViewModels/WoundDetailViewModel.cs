
// ViewModels/WoundDetailViewModel.cs
using System.Collections.ObjectModel;
using System.Windows.Input;
using SkinMonitor.Models;
using SkinMonitor.Services;

namespace SkinMonitor.ViewModels;

public class WoundDetailViewModel : BaseViewModel
{
    private readonly IWoundRepository _woundRepository;
    private readonly IPhotoService _photoService;
    
    private Wound? _selectedWound;
    private WoundPhoto? _selectedPhoto;

    public Wound? SelectedWound
    {
        get => _selectedWound;
        set => SetProperty(ref _selectedWound, value);
    }

    public WoundPhoto? SelectedPhoto
    {
        get => _selectedPhoto;
        set => SetProperty(ref _selectedPhoto, value);
    }

    public ObservableCollection<WoundPhoto> Photos { get; } = new();
    public ObservableCollection<HealingStageLog> HealingLogs { get; } = new();

    public ICommand TakePhotoCommand { get; }
    public ICommand AddHealingLogCommand { get; }
    public ICommand ViewAnalysisCommand { get; }
    public ICommand EditWoundCommand { get; }

    public WoundDetailViewModel(IWoundRepository woundRepository, IPhotoService photoService)
    {
        _woundRepository = woundRepository;
        _photoService = photoService;
        
        TakePhotoCommand = new Command(async () => await TakePhoto());
        AddHealingLogCommand = new Command(async () => await AddHealingLog());
        ViewAnalysisCommand = new Command(async () => await ViewAnalysis());
        EditWoundCommand = new Command(async () => await EditWound());
    }

    public async Task LoadWound(int woundId)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            SelectedWound = await _woundRepository.GetWoundByIdAsync(woundId);
            
            if (SelectedWound != null)
            {
                Title = SelectedWound.Name;
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Photos.Clear();
                    HealingLogs.Clear();
                    
                    foreach (var photo in SelectedWound.Photos)
                        Photos.Add(photo);
                        
                    foreach (var log in SelectedWound.HealingLogs)
                        HealingLogs.Add(log);
                });
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", 
                $"Failed to load wound: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TakePhoto()
    {
        if (SelectedWound == null) return;

        var route = $"//PhotoCapture?woundId={SelectedWound.Id}";
        await Shell.Current.GoToAsync(route);
    }

    private async Task AddHealingLog()
    {
        if (SelectedWound == null) return;

        // Navigate to healing log page or show popup
        var stage = await Application.Current.MainPage.DisplayActionSheet(
            "Select Healing Stage", "Cancel", null,
            "Initial", "Inflammatory", "Proliferative", "Remodeling", "Healed");

        if (stage != "Cancel" && stage != null)
        {
            var healingStage = Enum.Parse<HealingStage>(stage);
            var log = new HealingStageLog
            {
                WoundId = SelectedWound.Id,
                Stage = healingStage,
                Date = DateTime.Now
            };

            await _woundRepository.AddHealingLogAsync(log);
            await LoadWound(SelectedWound.Id);
        }
    }

    private async Task ViewAnalysis()
    {
        if (SelectedWound == null) return;

        var route = $"//Analysis?woundId={SelectedWound.Id}";
        await Shell.Current.GoToAsync(route);
    }

    private async Task EditWound()
    {
        if (SelectedWound == null) return;

        var route = $"//EditWound?woundId={SelectedWound.Id}";
        await Shell.Current.GoToAsync(route);
    }
}