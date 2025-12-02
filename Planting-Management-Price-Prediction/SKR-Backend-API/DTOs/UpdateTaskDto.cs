using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SKR_Backend_API.DTOs;

public class UpdateTaskDto
{
    [Required]
    [StringLength(200)]
    public string TaskName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Phase { get; set; }

    [StringLength(50)]
    public string Priority { get; set; } = "Medium";

    [Required]
    public DateTime DueDate { get; set; }

    public List<string>? DetailedSteps { get; set; }

    [StringLength(500)]
    public string? ReasonWhy { get; set; }
}
