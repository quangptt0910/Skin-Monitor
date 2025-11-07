using Microsoft.Extensions.Logging;

using SkinMonitor.Services;
using SkinMonitor.ViewModels;
using CommunityToolkit.Maui;
using SkinMonitor.Data;
using SkinMonitor.Views;


namespace SkinMonitor;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        try
        {
            var builder = MauiApp.CreateBuilder();
            
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            

#if DEBUG
            builder.Logging.AddDebug();
#endif      
            // Data
            builder.Services.AddSingleton<WoundDbContext>();
            builder.Services.AddSingleton<WoundRepository>();
            
            builder.Services.AddSingleton<IAIAnalysisService, AIAnalysisService>();

            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
            builder.Services.AddSingleton<PhotoService>();

            builder.Services.AddScoped<IWoundRepository, WoundRepository>();
            builder.Services.AddScoped<IAIAnalysisService, AIAnalysisService>();

            // navigation
            builder.Services.AddSingleton<NavigationViewModel>();

            // Register ViewModels
            builder.Services.AddTransient<WoundListViewModel>();
            builder.Services.AddTransient<WoundDetailViewModel>();
            builder.Services.AddTransient<AddWoundViewModel>();
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddSingleton<NavigationViewModel>();
            builder.Services.AddTransient<AnalysisPageViewModel>(); // Add this

            
            // Register Views/Pages
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<WoundDetailPage>();
            builder.Services.AddTransient<AddWoundPage>();
            builder.Services.AddTransient<PhotoCapturePage>();
            builder.Services.AddTransient<AnalysisPage>();
            return builder.Build();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MAUI Program Error: {ex}");
            throw;
        }
    }
}
