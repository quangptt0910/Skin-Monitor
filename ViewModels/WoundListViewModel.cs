
// ViewModels/WoundListViewModel.cs
using System.Collections.ObjectModel;
using System.Windows.Input;
using SkinMonitor.Models;
using SkinMonitor.Services;

namespace SkinMonitor.ViewModels;

public class WoundListViewModel : BaseViewModel
{
    private readonly IWoundRepository _woundRepository;
    
    public ObservableCollection<Wound> Wounds { get; } = new();
    public ObservableCollection<Wound> ActiveWounds { get; } = new();

    public ICommand LoadWoundsCommand { get; }
    public ICommand AddWoundCommand { get; }
    public ICommand SelectWoundCommand { get; }
    public ICommand RefreshCommand { get; }

    public WoundListViewModel(IWoundRepository woundRepository)
    {
        _woundRepository = woundRepository;
        Title = "My Wounds";
        
        LoadWoundsCommand = new Command(async () => await LoadWounds());
        AddWoundCommand = new Command(async () => await AddWound());
        SelectWoundCommand = new Command<Wound>(async (wound) => await SelectWound(wound));
        RefreshCommand = new Command(async () => await RefreshWounds());
    }

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
            await Application.Current.MainPage.DisplayAlert("Error", 
                $"Failed to load wounds: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddWound()
    {
        await Shell.Current.GoToAsync("AddWound");
    }

    private async Task SelectWound(Wound wound)
    {
        if (wound == null) return;
        
        var route = $"WoundDetail?woundId={wound.Id}";
        await Shell.Current.GoToAsync(route);
    }

    private async Task RefreshWounds()
    {
        await LoadWounds();
    }

    public async Task Initialize()
    {
        await LoadWounds();
    }
}