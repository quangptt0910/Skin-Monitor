
using SkinMonitor.Models;
using SkinMonitor.Services;
using SkinMonitor.ViewModels;

namespace SkinMonitor.Views;

public partial class AddWoundPage : ContentPage
{
    public AddWoundViewModel ViewModel { get; }
    
    
    public AddWoundPage(AddWoundViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = ViewModel;
    }

}