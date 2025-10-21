using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using SkinMonitor.Models;

namespace SkinMonitor.ViewModels;

/// <summary>
/// Composite ViewModel for MainPage that orchestrates WoundListViewModel and NavigationViewModel
/// Fixed to work with actual WoundListViewModel command types and structure
/// </summary>
public partial class MainPageViewModel : ObservableObject, INotifyPropertyChanged
{
    #region Private Fields
    private readonly NavigationViewModel _navigationViewModel;
    private readonly WoundListViewModel _woundListViewModel;
    #endregion

    #region Public Properties

    /// <summary>
    /// Navigation functionality for bottom navigation bar
    /// </summary>
    public NavigationViewModel NavigationViewModel => _navigationViewModel;

    /// <summary>
    /// Wound list functionality for main content
    /// </summary>
    public WoundListViewModel WoundListViewModel => _woundListViewModel;

    /// <summary>
    /// Page title exposed from WoundListViewModel for binding
    /// </summary>
    public string Title => _woundListViewModel?.Title ?? "SkinMonitor";

    /// <summary>
    /// Loading state exposed from WoundListViewModel
    /// </summary>
    public bool IsBusy => _woundListViewModel?.IsBusy ?? false;

    /// <summary>
    /// Active wounds collection exposed from WoundListViewModel
    /// </summary>
    public ObservableCollection<Wound> ActiveWounds => _woundListViewModel?.ActiveWounds ?? new();

    /// <summary>
    /// All wounds collection exposed from WoundListViewModel
    /// </summary>
    public ObservableCollection<Wound> Wounds => _woundListViewModel?.Wounds ?? new();

    #endregion

    #region Commands - Correctly typed for WoundListViewModel

    /// <summary>
    /// Refresh command that triggers WoundListViewModel refresh
    /// </summary>
    public ICommand RefreshCommand => _woundListViewModel?.RefreshCommand;

    /// <summary>
    /// Add wound command from WoundListViewModel
    /// </summary>
    public ICommand AddWoundCommand => _woundListViewModel?.AddWoundCommand;

    /// <summary>
    /// Select wound command from WoundListViewModel
    /// </summary>
    public ICommand SelectWoundCommand => _woundListViewModel?.SelectWoundCommand;

    /// <summary>
    /// Load wounds command from WoundListViewModel
    /// </summary>
    public ICommand LoadWoundsCommand => _woundListViewModel?.LoadWoundsCommand;

    /// <summary>
    /// Analyze wound command from WoundListViewModel
    /// </summary>
    public ICommand AnalyzeWoundCommand => _woundListViewModel?.AnalyzeWoundCommand;

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="woundListViewModel">Wound list functionality</param>
    /// <param name="navigationViewModel">Navigation functionality</param>
    public MainPageViewModel(WoundListViewModel woundListViewModel, NavigationViewModel navigationViewModel)
    {
        _woundListViewModel = woundListViewModel ?? throw new ArgumentNullException(nameof(woundListViewModel));
        _navigationViewModel = navigationViewModel ?? throw new ArgumentNullException(nameof(navigationViewModel));

        // Subscribe to child ViewModel property changes to forward notifications
        SubscribeToChildViewModelChanges();

        // Set current page for navigation
        _navigationViewModel.CurrentPage = "Home";
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initialize the ViewModel - called when page appears
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            // Initialize WoundListViewModel using its Initialize method
            if (_woundListViewModel != null)
            {
                await _woundListViewModel.Initialize();
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Initialization Error", $"Failed to initialize: {ex.Message}");
        }
    }

    /// <summary>
    /// Cleanup resources when page is disposed
    /// </summary>
    public void Dispose()
    {
        UnsubscribeFromChildViewModelChanges();
    }

    /// <summary>
    /// Execute refresh command - wrapper for WoundListViewModel RefreshCommand
    /// </summary>
    public async Task ExecuteRefreshAsync()
    {
        try
        {
            if (_woundListViewModel?.RefreshCommand?.CanExecute(null) == true)
            {
                // RefreshCommand is a Command, so we execute it directly
                _woundListViewModel.RefreshCommand.Execute(null);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Refresh Error", $"Failed to refresh data: {ex.Message}");
        }
    }

    /// <summary>
    /// Execute add wound command - wrapper for WoundListViewModel AddWoundCommand
    /// </summary>
    public async Task ExecuteAddWoundAsync()
    {
        try
        {
            if (_woundListViewModel?.AddWoundCommand?.CanExecute(null) == true)
            {
                _woundListViewModel.AddWoundCommand.Execute(null);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Add Wound Error", $"Failed to add wound: {ex.Message}");
        }
    }

    /// <summary>
    /// Execute select wound command - wrapper for WoundListViewModel SelectWoundCommand
    /// </summary>
    public async Task ExecuteSelectWoundAsync(Wound wound)
    {
        try
        {
            if (wound != null && _woundListViewModel?.SelectWoundCommand?.CanExecute(wound) == true)
            {
                _woundListViewModel.SelectWoundCommand.Execute(wound);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Select Wound Error", $"Failed to select wound: {ex.Message}");
        }
    }

    /// <summary>
    /// Get wound statistics from WoundListViewModel
    /// </summary>
    public WoundStatistics GetStatistics()
    {
        return _woundListViewModel?.GetStatistics() ?? new WoundStatistics();
    }

    /// <summary>
    /// Delete wound using WoundListViewModel method
    /// </summary>
    public async Task DeleteWoundAsync(Wound wound)
    {
        try
        {
            if (_woundListViewModel != null && wound != null)
            {
                await _woundListViewModel.DeleteWound(wound);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Delete Error", $"Failed to delete wound: {ex.Message}");
        }
    }

    /// <summary>
    /// Mark wound as healed using WoundListViewModel method
    /// </summary>
    public async Task MarkAsHealedAsync(Wound wound)
    {
        try
        {
            if (_woundListViewModel != null && wound != null)
            {
                await _woundListViewModel.MarkAsHealed(wound);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Mark as Healed Error", $"Failed to mark wound as healed: {ex.Message}");
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Subscribe to child ViewModel property changes to forward notifications
    /// </summary>
    private void SubscribeToChildViewModelChanges()
    {
        if (_woundListViewModel != null)
        {
            _woundListViewModel.PropertyChanged += OnWoundListViewModelPropertyChanged;
        }

        if (_navigationViewModel != null)
        {
            _navigationViewModel.PropertyChanged += OnNavigationViewModelPropertyChanged;
        }
    }

    /// <summary>
    /// Unsubscribe from child ViewModel property changes
    /// </summary>
    private void UnsubscribeFromChildViewModelChanges()
    {
        if (_woundListViewModel != null)
        {
            _woundListViewModel.PropertyChanged -= OnWoundListViewModelPropertyChanged;
        }

        if (_navigationViewModel != null)
        {
            _navigationViewModel.PropertyChanged -= OnNavigationViewModelPropertyChanged;
        }
    }

    /// <summary>
    /// Handle WoundListViewModel property changes
    /// </summary>
    private void OnWoundListViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Forward property change notifications for exposed properties
        switch (e.PropertyName)
        {
            case nameof(WoundListViewModel.Title):
                OnPropertyChanged(nameof(Title));
                break;
            case nameof(WoundListViewModel.IsBusy):
                OnPropertyChanged(nameof(IsBusy));
                break;
            case nameof(WoundListViewModel.ActiveWounds):
                OnPropertyChanged(nameof(ActiveWounds));
                break;
            case nameof(WoundListViewModel.Wounds):
                OnPropertyChanged(nameof(Wounds));
                break;
            case nameof(WoundListViewModel.WoundArea):
                // Forward AI analysis properties
                break;
            case nameof(WoundListViewModel.InfectionRisk):
                // Forward AI analysis properties
                break;
            case nameof(WoundListViewModel.WoundClassification):
                // Forward AI analysis properties
                break;
        }
    }

    /// <summary>
    /// Handle NavigationViewModel property changes
    /// </summary>
    private void OnNavigationViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Forward navigation-related property changes if needed
        switch (e.PropertyName)
        {
            case nameof(NavigationViewModel.CurrentPage):
                // Handle current page changes if needed
                break;
        }
    }

    /// <summary>
    /// Show error message with proper error handling
    /// </summary>
    private async Task ShowErrorAsync(string title, string message)
    {
        try
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert(title, message, "OK");
            }
            else if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, "OK");
            }
        }
        catch (Exception ex)
        {
            // Log error if display alert fails
            System.Diagnostics.Debug.WriteLine($"Error displaying alert: {ex.Message}");
        }
    }

    #endregion

    #region INotifyPropertyChanged Implementation

    public new event PropertyChangedEventHandler PropertyChanged;

    protected new virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        base.OnPropertyChanged(propertyName);
    }

    #endregion
}
