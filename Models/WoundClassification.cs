namespace SkinMonitor.Models;
public class WoundClassification
{
    public WoundType PrimaryType { get; set; }
    public float Confidence { get; set; }
    public Dictionary<WoundType, float> AlternativeTypes { get; set; } = new();
    public string? ErrorMessage { get; set; }
}