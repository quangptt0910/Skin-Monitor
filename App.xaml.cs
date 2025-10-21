using SkinMonitor.Services;

namespace SkinMonitor;

public partial class App : Application
{
    private readonly IDatabaseService _databaseService;

    public App(IDatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;

        // Initialize database
        Task.Run(async () => await _databaseService.InitializeAsync());
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell())
        {
            Title = "SkinMonitor - Wound Healing Tracker"
        };
    }
}
