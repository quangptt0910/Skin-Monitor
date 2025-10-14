namespace SkinMonitor.Models;
public class InfectionRiskAssessment
{
    public double RiskScore { get; set; } // 0.0 to 1.0
    public string RiskLevel { get; set; } = string.Empty;
    public List<string> RiskFactors { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}