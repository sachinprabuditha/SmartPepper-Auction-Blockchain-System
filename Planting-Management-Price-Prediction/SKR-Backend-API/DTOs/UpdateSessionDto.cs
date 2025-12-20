using System.ComponentModel.DataAnnotations;

namespace SKR_Backend_API.DTOs;

public class UpdateSessionDto
{
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Session name must be between 1 and 200 characters")]
    public string? SessionName { get; set; }

    public DateTime? Date { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Yield must be greater than 0")]
    public double? YieldKg { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Area harvested must be greater than 0")]
    public double? AreaHarvested { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}

