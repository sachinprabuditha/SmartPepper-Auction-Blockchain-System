using System.ComponentModel.DataAnnotations;

namespace SKR_Backend_API.DTOs;

public class CreateManualTaskDto
{
    [Required]
    [StringLength(200)]
    public string FarmId { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string TaskName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Phase { get; set; } // Optional - can be set manually

    [StringLength(100)]
    public string TaskType { get; set; } = "Manual";

    [Required]
    public DateTime DueDate { get; set; }

    [StringLength(50)]
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Emergency

    public List<string>? DetailedSteps { get; set; }

    [StringLength(500)]
    public string? ReasonWhy { get; set; }
}

