namespace SkinMonitor.Models;

public class Wound
{
    public int Id { get; set; }
    public string Name { get; set; } // "Knee Scrape", "Surgery Incision"
    public string BodyLocation { get; set; } // "Left Knee", "Abdomen"
    public WoundType Type { get; set; } // Enum: Surgical, Burn, Diabetic, Traumatic
    public DateTime DateCreated { get; set; }
    public DateTime? DateHealed { get; set; }
    public bool IsActive { get; set; } = true;
    public string Notes { get; set; }
    
    // Relationships
    public List<WoundPhoto> Photos { get; set; } = new();
    public List<HealingStageLog> HealingLogs { get; set; } = new();
}