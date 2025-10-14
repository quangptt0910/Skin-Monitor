using Microsoft.Extensions.Logging;

using SkinMonitor.Services;
using SkinMonitor.ViewModels;
using CommunityToolkit.Maui;
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
            // ⭐ Register Services - THIS IS CRITICAL!
            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
            builder.Services.AddScoped<IWoundRepository, WoundRepository>();
            builder.Services.AddScoped<IPhotoService, PhotoService>();
            builder.Services.AddScoped<IAIAnalysisService, AIAnalysisService>();

            // Register ViewModels
            builder.Services.AddTransient<WoundListViewModel>();
            builder.Services.AddTransient<WoundDetailViewModel>();

            // Register Views/Pages
            builder.Services.AddTransient<MainPage>();
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

// public static class MauiProgram
// {
//     public static MauiApp CreateMauiApp()
//     {
//         var builder = MauiApp.CreateBuilder();
//         
//         builder
//             .UseMauiApp<App>()
//             .UseMauiCommunityToolkit()
//             .UseMauiCommunityToolkitMediaElement()  
//             .ConfigureFonts(fonts =>
//             {
//                 fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
//                 fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
//             });
//
// #if DEBUG
//         builder.Services.AddLogging(configure => configure.AddDebug());
// #endif
//
//         // Register Services
//         builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
//         builder.Services.AddScoped<IWoundRepository, WoundRepository>();
//         builder.Services.AddScoped<IPhotoService, PhotoService>();
//         builder.Services.AddScoped<IAIAnalysisService, AIAnalysisService>();
//
//         // Register ViewModels
//         builder.Services.AddTransient<WoundListViewModel>();
//         builder.Services.AddTransient<WoundDetailViewModel>();
//
//         // Register Views
//         builder.Services.AddTransient<MainPage>();
//
//         return builder.Build();
//     }
// }