using System.Globalization;

namespace SkinMonitor.Converters;

public class IsSelectedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string currentPage && parameter is string targetPage)
        {
            return currentPage == targetPage ? "#512BD4" : "Transparent";
        }
        return "Transparent";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}