using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SKR_Backend_API.Models;

[Table("EmergencyTemplates")]
public class EmergencyTemplate
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(255)]
    [Column("issuename")]
    public string IssueName { get; set; } = string.Empty;

    [Column("symptoms", TypeName = "text")]
    public string Symptoms { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("treatmenttask")]
    public string TreatmentTask { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("priority")]
    public string Priority { get; set; } = "High"; // Low, Medium, High, Emergency

    [Column("instructions", TypeName = "text")]
    public string Instructions { get; set; } = string.Empty;

    [NotMapped]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
