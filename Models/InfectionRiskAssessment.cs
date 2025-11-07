namespace SkinMonitor.Models;
public class InfectionRiskAssessment
{
    public string RiskLevel { get; set; }
    public double RiskScore { get; set; }
    public List<string> RiskFactors { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public string? ErrorMessage { get; set; }
}