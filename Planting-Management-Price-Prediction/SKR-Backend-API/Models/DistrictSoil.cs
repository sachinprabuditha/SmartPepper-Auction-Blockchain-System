using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SKR_Backend_API.Models;

[Table("DistrictSoils")]
public class DistrictSoil
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Required]
    [Column("DistrictId")]
    public int DistrictId { get; set; }

    [Required]
    [Column("SoilTypeId")]
    public int SoilTypeId { get; set; }

    // Navigation properties
    [ForeignKey("DistrictId")]
    public District District { get; set; } = null!;

    [ForeignKey("SoilTypeId")]
    public SoilType SoilType { get; set; } = null!;
}

