using System.Globalization;

namespace SkinMonitor.Converters;

public class IsSelectedTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            if (value is string currentPage && parameter is string targetPage)
            {
                bool isSelected = currentPage == targetPage;
                System.Diagnostics.Debug.WriteLine($"TextColorConverter: CurrentPage='{currentPage}', TargetPage='{targetPage}', IsSelected={isSelected}");
                return isSelected ? "White" : "#512BD4";
            }
            
            System.Diagnostics.Debug.WriteLine($"TextColorConverter: Invalid parameters - value='{value}', parameter='{parameter}'");
            return "#512BD4"; // Default color
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TextColorConverter error: {ex.Message}");
            return "#512BD4";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}