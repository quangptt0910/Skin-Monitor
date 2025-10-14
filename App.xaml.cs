using SkinMonitor.Services;

namespace SkinMonitor;

public partial class App : Application
{
    public App(IDatabaseService databaseService)
    {
        InitializeComponent();
        
        // Initialize database
        Task.Run(async () => await databaseService.InitializeAsync());
        
        MainPage = new AppShell();
    }
}