using SkinMonitor.ViewModels;

namespace SkinMonitor.Views;

public partial class HomePage : ContentPage
{
    private NavigationViewModel _navigationViewModel;

    public HomePage()
    {
        InitializeComponent();
        try
        {
            // Try to get NavigationViewModel from DI - with safe null checking
            _navigationViewModel = GetNavigationViewModelFromDI() ?? new NavigationViewModel();
            BindingContext = _navigationViewModel;
        }
        catch (Exception ex)
        {
            // Fallback to new instance if DI fails
            _navigationViewModel = new NavigationViewModel();
            BindingContext = _navigationViewModel;
            System.Diagnostics.Debug.WriteLine($"HomePage constructor error: {ex.Message}");
        }
    }

    private async void OnViewWoundsTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///MainPage");
    }

    private async void OnAddWoundTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("AddWound");
    }

    private async void OnAnalysisTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Analysis");
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Set current page when HomePage appears
        _navigationViewModel?.UpdateCurrentPage("Home");
    }
    
    private NavigationViewModel GetNavigationViewModelFromDI()
    {
        try
        {
            // Safe chain of null checks
            var app = Application.Current;
            if (app == null) return null;

            var mainPage = app.MainPage;
            if (mainPage == null) return null;

            var handler = mainPage.Handler;
            if (handler == null) return null;

            var mauiContext = handler.MauiContext;
            if (mauiContext == null) return null;

            var services = mauiContext.Services;
            if (services == null) return null;

            return services.GetService<NavigationViewModel>();
        }
        catch
        {
            return null;
        }
    }
}
