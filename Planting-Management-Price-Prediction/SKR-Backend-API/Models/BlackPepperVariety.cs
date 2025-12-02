using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SKR_Backend_API.Models;

[Table("PepperVarieties")]
public class BlackPepperVariety
{
    [Key]
    [Column("Id", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("Name")]
    public string Name { get; set; } = string.Empty;

    [Column("Specialities", TypeName = "text")]
    public string Specialities { get; set; } = string.Empty;

    [Column("SuitabilityReason", TypeName = "text")]
    public string SuitabilityReason { get; set; } = string.Empty;

    [Column("SoilTypeRecommendation", TypeName = "text")]
    public string SoilTypeRecommendation { get; set; } = string.Empty;

    [Column("SpacingMeters", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string SpacingMeters { get; set; } = string.Empty;

    [Column("VinesPerHectare")]
    public int? VinesPerHectare { get; set; }

    [Column("PitDimensionsCm", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string PitDimensionsCm { get; set; } = string.Empty;

    // Computed property for backward compatibility with PlantingSpecifications
    [NotMapped]
    public PlantingSpecifications PlantingSpecifications
    {
        get => new PlantingSpecifications
        {
            SpacingMeters = SpacingMeters,
            VinesPerHectare = VinesPerHectare ?? 0,
            PitDimensionsCm = PitDimensionsCm
        };
        set
        {
            SpacingMeters = value.SpacingMeters;
            VinesPerHectare = value.VinesPerHectare;
            PitDimensionsCm = value.PitDimensionsCm;
        }
    }
}

public class PlantingSpecifications
{
    public string SpacingMeters { get; set; } = string.Empty;
    public int VinesPerHectare { get; set; }
    public string PitDimensionsCm { get; set; } = string.Empty;
}

