namespace SkinMonitor.Models;

public class HealingPrediction
{
    public int EstimatedDaysToHeal { get; set; }
    public double ConfidenceLevel { get; set; }
    public DateTime PredictionDate { get; set; }
    public HealingStage HealingStage { get; set; }
}