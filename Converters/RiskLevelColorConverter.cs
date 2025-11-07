using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics; // Required for the Colors class

/// <summary>
/// Converts a risk level string (e.g., "High", "Medium", "Low") 
/// into a corresponding Color.
/// </summary>
public class RiskLevelColorConverter : IValueConverter
{
    /// <summary>
    /// Converts the risk level string to a Color.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Get the risk level as a string
        var riskLevel = value as string;

        if (string.IsNullOrWhiteSpace(riskLevel))
        {
            return Colors.Gray; // Default color for invalid input
        }

        // Return a color based on the risk level
        switch (riskLevel.ToLowerInvariant())
        {
            case "high":
            case "high risk":
                return Colors.Red;
                
            case "medium":
            case "medium risk":
                return Colors.Orange; // Or Colors.Yellow, depending on your theme

            case "low":
            case "low risk":
                return Colors.Green;

            case "indeterminate":
            case "unknown":
                return Colors.Gray;

            default:
                return Colors.Transparent; // Default for other cases
        }
    }

    /// <summary>
    /// Converts a Color back to a risk level string (not typically used).
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // This converter is one-way, so we don't need to implement ConvertBack
        throw new NotImplementedException();
    }
}