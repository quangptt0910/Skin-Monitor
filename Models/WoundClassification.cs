namespace SkinMonitor.Models;
public class WoundClassification
{
    public WoundType PrimaryType { get; set; }
    public double Confidence { get; set; }
    public Dictionary<WoundType, double> AlternativeTypes { get; set; } = new();
    public string? ErrorMessage { get; set; }
}