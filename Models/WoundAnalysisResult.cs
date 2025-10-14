namespace SkinMonitor.Models;

public class WoundAnalysisResult
{
    public double WoundAreaCm2 { get; set; }
    public WoundClassification Classification { get; set; } = new();
    public InfectionRiskAssessment InfectionRisk { get; set; } = new();
    public DateTime AnalysisDate { get; set; }
    public double ConfidenceScore { get; set; }
    public string? ErrorMessage { get; set; }
}