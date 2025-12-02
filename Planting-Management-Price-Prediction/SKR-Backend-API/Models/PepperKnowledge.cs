using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pgvector; // Requires Pgvector.EntityFrameworkCore

namespace SKR_Backend_API.Models;

[Table("pepperknowledge")]
public class PepperKnowledge
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("category")]
    public string Category { get; set; } = string.Empty;

    [Column("sub_category")]
    public string? SubCategory { get; set; }

    [Column("district")]
    public string? District { get; set; }

    [Column("variety")]
    public string? Variety { get; set; }

    [Column("plant_age_min")]
    public int? PlantAgeMin { get; set; } // in months

    [Column("plant_age_max")]
    public int? PlantAgeMax { get; set; } // in months

    [Column("month_start")]
    public int? MonthStart { get; set; } // 1-12

    [Column("month_end")]
    public int? MonthEnd { get; set; } // 1-12

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("content", TypeName = "text")]
    public string Content { get; set; } = string.Empty;

    [Column("source")]
    public string? Source { get; set; }

    [Column("confidence_level")]
    public string? ConfidenceLevel { get; set; } // High, Medium, Low

    [Column("embedding", TypeName = "vector(1536)")]
    public Vector? Embedding { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
