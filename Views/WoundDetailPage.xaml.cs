using SkinMonitor.Models;
using SkinMonitor.Services;

namespace SkinMonitor.Views;

[QueryProperty(nameof(WoundId), "woundId")]
public partial class WoundDetailPage : ContentPage
{
    private readonly IWoundRepository _woundRepository;
    private string _woundId = string.Empty;
    private Wound? _currentWound;

    public string WoundId
    {
        get => _woundId;
        set
        {
            _woundId = value;
            LoadWoundDetails();
        }
    }

    public WoundDetailPage(IWoundRepository woundRepository)
    {
        InitializeComponent();
        _woundRepository = woundRepository;
    }

    private async void LoadWoundDetails()
    {
        if (int.TryParse(_woundId, out int id))
        {
            try
            {
                _currentWound = await _woundRepository.GetWoundByIdAsync(id);

                if (_currentWound != null)
                {
                    WoundNameLabel.Text = _currentWound.Name;
                    BodyLocationLabel.Text = $"📍 {_currentWound.BodyLocation}";
                    WoundTypeLabel.Text = _currentWound.Type.ToString();
                    DateCreatedLabel.Text = $"Created: {_currentWound.DateCreated:MMM dd, yyyy}";
                    NotesLabel.Text = string.IsNullOrEmpty(_currentWound.Notes)
                        ? "No notes available"
                        : _currentWound.Notes;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load wound: {ex.Message}", "OK");
            }
        }
    }

    private async void OnTakePhotoClicked(object sender, EventArgs e)
    {
        if (_currentWound != null)
        {
            await Shell.Current.GoToAsync($"PhotoCapture?woundId={_currentWound.Id}");
        }
    }

    private async void OnViewPhotosClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Photos", "Photo gallery coming soon!", "OK");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
