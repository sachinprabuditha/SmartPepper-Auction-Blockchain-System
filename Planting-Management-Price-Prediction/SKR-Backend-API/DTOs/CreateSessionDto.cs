using System.ComponentModel.DataAnnotations;

namespace SKR_Backend_API.DTOs;

public class CreateSessionDto
{
    [Required(ErrorMessage = "Season ID is required")]
    public string SeasonId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Session name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Session name must be between 1 and 200 characters")]
    public string SessionName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Yield in kg is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Yield must be greater than 0")]
    public double YieldKg { get; set; }

    [Required(ErrorMessage = "Area harvested is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Area harvested must be greater than 0")]
    public double AreaHarvested { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}

