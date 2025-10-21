using SkinMonitor.ViewModels;

namespace SkinMonitor.Views;

public partial class NavigationBar : ContentView
{
    private static NavigationViewModel _sharedNavigationViewModel;

    public NavigationBar()
    {
        InitializeComponent();
    }
    
    protected override void OnParentSet()
    {
        base.OnParentSet();
        
        try
        {
            // Always use the same singleton NavigationViewModel instance
            if (_sharedNavigationViewModel == null)
            {
                _sharedNavigationViewModel = GetOrCreateNavigationViewModel();
            }
            
            BindingContext = _sharedNavigationViewModel;
            
            System.Diagnostics.Debug.WriteLine("NavigationBar BindingContext set to shared NavigationViewModel");
        }
        catch (Exception ex)
        {
            // Emergency fallback
            _sharedNavigationViewModel = new NavigationViewModel();
            BindingContext = _sharedNavigationViewModel;
            System.Diagnostics.Debug.WriteLine($"NavigationBar OnParentSet error: {ex.Message}");
        }
    }

    private NavigationViewModel GetOrCreateNavigationViewModel()
    {
        try
        {
            // Try to get from parent page first
            if (Parent is ContentPage page)
            {
                // Check if page has MainPageViewModel (composite)
                if (page.BindingContext is MainPageViewModel mainPageVM)
                {
                    return mainPageVM.NavigationViewModel;
                }
                
                // Check if page already has NavigationViewModel
                if (page.BindingContext is NavigationViewModel existingNavVM)
                {
                    return existingNavVM;
                }
            }

            // Try to get from DI
            var services = Application.Current?.MainPage?.Handler?.MauiContext?.Services;
            var navVM = services?.GetService<NavigationViewModel>();
            
            if (navVM != null)
            {
                return navVM;
            }

            // Create new instance as last resort
            return new NavigationViewModel();
        }
        catch
        {
            return new NavigationViewModel();
        }
    }
}
