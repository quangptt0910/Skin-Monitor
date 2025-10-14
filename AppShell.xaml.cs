using SkinMonitor.Views;

namespace SkinMonitor;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes for navigation
        Routing.RegisterRoute("WoundDetail", typeof(WoundDetailPage));
        Routing.RegisterRoute("AddWound", typeof(AddWoundPage));
        Routing.RegisterRoute("PhotoCapture", typeof(PhotoCapturePage));
        Routing.RegisterRoute("Analysis", typeof(AnalysisPage));
    }
}