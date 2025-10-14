namespace SkinMonitor.Models;

public class HealingPrediction
{
    public int PredictedHealingDays { get; set; }
    public double ConfidenceLevel { get; set; }
    public string TrendAnalysis { get; set; } = string.Empty;
    public double DailyReductionRate { get; set; }
}