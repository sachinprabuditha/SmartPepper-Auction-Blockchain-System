using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SKR_Backend_API.DTOs;

public class UpdateCompletionDetailsDto
{
    // Items are optional - can update without items
    public List<InputItemDto>? Items { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Labor hours must be greater than or equal to 0")]
    public double LaborHours { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

