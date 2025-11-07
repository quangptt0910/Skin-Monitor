using System;
using System.Globalization;
using Microsoft.Maui.Controls; // Or use Xamarin.Forms if you are on an older project

/// <summary>
/// Converts a boolean value to its inverse (true -> false, false -> true).
/// Used for binding UI element properties (like IsVisible or IsEnabled)
/// to the opposite of a boolean view model property (like IsLoading).
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean to its opposite.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
        {
            return false; // Default value if input is not a bool
        }
        
        // Return the opposite value
        return !boolValue;
    }

    /// <summary>
    /// Converts an inverted boolean back to its original value.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
        {
            return false;
        }
        
        // Return the opposite value
        return !boolValue;
    }
}