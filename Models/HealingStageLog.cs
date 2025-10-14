namespace SkinMonitor.Models;

public class HealingStageLog
{
    public int Id { get; set; }
    public int WoundId { get; set; }
    public DateTime Date { get; set; }
    public HealingStage Stage { get; set; }
    public int PainLevel { get; set; } // 0-10 scale
    public bool HasRedness { get; set; }
    public bool HasSwelling { get; set; }
    public bool HasDrainage { get; set; }
    public string DrainageType { get; set; } // "Clear", "Bloody", "Pus"
    public string Notes { get; set; }
    
    public Wound Wound { get; set; }
}