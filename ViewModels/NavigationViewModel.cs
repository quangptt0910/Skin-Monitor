using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows.Input;

namespace SkinMonitor.ViewModels;

public partial class NavigationViewModel : ObservableObject
{
    [ObservableProperty]
    private string currentPage = "Main";

    public ICommand GoToHomeCommand { get; }
    public ICommand GoToMainCommand { get; }
    public ICommand GoToAnalysisCommand { get; }

    public NavigationViewModel()
    {
        GoToHomeCommand = new AsyncRelayCommand(NavigateToHome);
        GoToMainCommand = new AsyncRelayCommand(NavigateToMain);
        GoToAnalysisCommand = new AsyncRelayCommand(NavigateToAnalysis);
    }

    private async Task NavigateToHome()
    {
        try
        {
            CurrentPage = "Home";
            await Shell.Current.GoToAsync("HomePage");
        }
        catch (Exception ex)
        {
            await ShowError("Navigation Error", $"Failed to navigate to Home: {ex.Message}");
        }
    }

    private async Task NavigateToMain()
    {
        try
        {
            CurrentPage = "Main";
            await Shell.Current.GoToAsync("///MainPage");
        }
        catch (Exception ex)
        {
            await ShowError("Navigation Error", $"Failed to navigate to Main: {ex.Message}");
        }
    }

    private async Task NavigateToAnalysis()
    {
        try
        {
            CurrentPage = "Analysis";
            await Shell.Current.GoToAsync("Analysis");
        }
        catch (Exception ex)
        {
            await ShowError("Navigation Error", $"Failed to open analysis: {ex.Message}");
        }
    }
    
    public void UpdateCurrentPage(string pageName)
    {
        CurrentPage = pageName;
    }
    private async Task ShowError(string title, string message)
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
            System.Diagnostics.Debug.WriteLine($"Error displaying alert: {ex.Message}");
        }
    }
}