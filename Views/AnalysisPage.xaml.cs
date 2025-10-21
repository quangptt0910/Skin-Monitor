using SkinMonitor.ViewModels;

namespace SkinMonitor.Views;

public partial class AnalysisPage : ContentPage
{
    private readonly AnalysisPageViewModel _viewModel;
    
    public AnalysisPage(AnalysisPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        try
        {
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load page: {ex.Message}", "OK");
        }
    }
}