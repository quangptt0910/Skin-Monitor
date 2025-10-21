using SkinMonitor.Views;

namespace SkinMonitor;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register all navigation routes
        Routing.RegisterRoute("HomePage", typeof(HomePage));
        Routing.RegisterRoute("MainPage", typeof(MainPage));
        Routing.RegisterRoute("WoundDetail", typeof(WoundDetailPage));
        Routing.RegisterRoute("AddWound", typeof(AddWoundPage));
        Routing.RegisterRoute("PhotoCapture", typeof(PhotoCapturePage));
        Routing.RegisterRoute("Analysis", typeof(AnalysisPage));
    }
}