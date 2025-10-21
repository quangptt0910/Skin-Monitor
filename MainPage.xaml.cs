using SkinMonitor.ViewModels;

namespace SkinMonitor;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _viewModel;
    
    public MainPage(MainPageViewModel viewModel)
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
            // Set current page in navigation
            _viewModel.NavigationViewModel.UpdateCurrentPage("Main");
            System.Diagnostics.Debug.WriteLine("MainPage appeared, set CurrentPage to Main");

            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load page: {ex.Message}", "OK");
        }
    }
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        System.Diagnostics.Debug.WriteLine("MainPage disappeared");

        _viewModel?.Dispose();
    }
}