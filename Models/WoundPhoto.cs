namespace SkinMonitor.Models;

public class WoundPhoto
{
    public int Id { get; set; }
    public int WoundId { get; set; }
    public DateTime DateTaken { get; set; }
    public string PhotoPath { get; set; }
    public string ThumbnailPath { get; set; }
    
    // Measurements
    public double? WoundAreaCm2 { get; set; }
    public double? LengthMm { get; set; }
    public double? WidthMm { get; set; }
    public double? DepthMm { get; set; }
    
    // Positioning data for consistency
    public double CameraDistanceCm { get; set; }
    public double CameraAngle { get; set; }
    public string ReferenceObjectUsed { get; set; } // "Coin", "Ruler"
    
    // AI Analysis Results (populated later)
    public double? InfectionRiskScore { get; set; }
    public double? HealingProgressScore { get; set; }
    public string AIClassification { get; set; }
    public string AIDetectedIssues { get; set; } // JSON array
    
    public string Notes { get; set; }
    
    // Foreign Key
    public Wound Wound { get; set; }
}