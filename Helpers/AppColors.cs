using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SkinMonitor.Helpers;

public class AppColors
{

    public static AppColors Instance { get; } = new AppColors();

    // Primary Colors
    public static Color Primary => Color.FromArgb("#512BD4");
    public static Color PrimaryDark => Color.FromArgb("#ac99ea");

    // Background Colors
    public static Color BackgroundLight => Color.FromArgb("#F5F5F5");
    public static Color BackgroundWhite => Colors.White;

    // Text Colors
    public static Color TextPrimary => Color.FromArgb("#212121");
    public static Color TextSecondary => Color.FromArgb("#757575");
    public static Color TextWhite => Colors.White;

    // Status Colors
    public static Color Success => Color.FromArgb("#4CAF50");
    public static Color Info => Color.FromArgb("#2196F3");
    public static Color Warning => Color.FromArgb("#FF9800");
    public static Color Danger => Color.FromArgb("#F44336");

    // Gray Scale
    public static Color Gray100 => Color.FromArgb("#E1E1E1");
    public static Color Gray200 => Color.FromArgb("#C8C8C8");
    public static Color Gray300 => Color.FromArgb("#ACACAC");
    public static Color Gray600 => Color.FromArgb("#404040");
    public static Color Gray800 => Color.FromArgb("#303030");
    public static Color Gray900 => Color.FromArgb("#212121");

    // Shadow
    public static Color ShadowColor => Colors.Black;

    // Helper method to get themed color (for future dark mode support)
    public static Color GetThemedColor(Color lightColor, Color darkColor)
    {
        return Application.Current?.RequestedTheme == AppTheme.Dark
            ? darkColor
            : lightColor;
    }
}
