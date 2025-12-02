using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SKR_Backend_API.Models;

[Table("GuideSteps")]
public class GuideStep
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Required]
    [Column("GuideId")]
    public int GuideId { get; set; }

    [Required]
    [Column("StepNumber")]
    public int StepNumber { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("Title")]
    public string Title { get; set; } = string.Empty;

    [Column("Details", TypeName = "text")]
    public string Details { get; set; } = string.Empty;

    // Navigation property
    [ForeignKey("GuideId")]
    public AgronomyGuide Guide { get; set; } = null!;
}

