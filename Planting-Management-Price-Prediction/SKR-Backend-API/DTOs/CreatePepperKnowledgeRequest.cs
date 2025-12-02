using System.ComponentModel.DataAnnotations;

namespace SKR_Backend_API.DTOs;

public class CreatePepperKnowledgeRequest
{
    [Required]
    public string Category { get; set; } = string.Empty;

    public string? SubCategory { get; set; }

    public string? District { get; set; }

    public string? Variety { get; set; }

    public int? PlantAgeMin { get; set; }

    public int? PlantAgeMax { get; set; }

    public int? MonthStart { get; set; }

    public int? MonthEnd { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public string? Source { get; set; }

    public string? ConfidenceLevel { get; set; } // High, Medium, Low
}
