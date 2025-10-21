using System.Globalization;

namespace SkinMonitor.Converters;

public class BoolToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isAnalyzing)
        {
            return isAnalyzing ? "🔄 Analyzing..." : "🔬 Run AI Analysis";
        }
        return "🔬 Run AI Analysis";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}