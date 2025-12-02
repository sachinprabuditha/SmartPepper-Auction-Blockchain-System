using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SKR_Backend_API.Models;

[Table("AgronomyGuides")]
public class AgronomyGuide
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("districtid")]
    public int DistrictId { get; set; }

    [Required]
    [Column("soiltypeid")]
    public int SoilTypeId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("varietyid")]
    public string VarietyId { get; set; } = string.Empty;

    // CreatedAt column doesn't exist in the database table
    [NotMapped]
    public DateTime? CreatedAt { get; set; }

    // Navigation properties
    [ForeignKey("DistrictId")]
    public District District { get; set; } = null!;

    [ForeignKey("SoilTypeId")]
    public SoilType SoilType { get; set; } = null!;

    [ForeignKey("VarietyId")]
    public BlackPepperVariety Variety { get; set; } = null!;

    // One-to-many relationship with GuideSteps
    public ICollection<GuideStep> Steps { get; set; } = new List<GuideStep>();
}

// Keep PlantingGuideModel for backward compatibility (deprecated)
[Obsolete("Use AgronomyGuide instead")]
[Table("AgronomyGuides")]
public class PlantingGuideModel
{
    [Key]
    [Column("Id", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("District")]
    public string District { get; set; } = string.Empty;

    [Column("CreatedAt", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

