using SkinMonitor.ViewModels;

namespace SkinMonitor;

public partial class MainPage : ContentPage
{
    public MainPage(WoundListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is WoundListViewModel viewModel)
        {
            await viewModel.Initialize();
        }
    }
}