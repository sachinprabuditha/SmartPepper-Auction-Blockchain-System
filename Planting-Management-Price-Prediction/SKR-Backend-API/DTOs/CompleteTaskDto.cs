using System.ComponentModel.DataAnnotations;

namespace SKR_Backend_API.DTOs;

public class CompleteTaskDto
{
    // Items are optional - task can be completed without logging items
    public List<InputItemDto>? Items { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Labor hours must be greater than or equal to 0")]
    public double LaborHours { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class InputItemDto
{
    [Required]
    [StringLength(200)]
    public string ItemName { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0")]
    public double Quantity { get; set; }

    // Unit cost is optional - can be null if cost is unknown
    [Range(0, double.MaxValue, ErrorMessage = "Unit cost must be greater than or equal to 0")]
    public double? UnitCostLKR { get; set; }

    [StringLength(20)]
    public string Unit { get; set; } = "kg"; // kg, liters, bags, etc.
}

