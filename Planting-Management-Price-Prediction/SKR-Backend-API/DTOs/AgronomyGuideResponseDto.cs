using SKR_Backend_API.Models;

namespace SKR_Backend_API.DTOs;

public class AgronomyGuideResponseDto
{
    public int Id { get; set; }
    public int DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public int SoilTypeId { get; set; }
    public string SoilTypeName { get; set; } = string.Empty;
    public string VarietyId { get; set; } = string.Empty;
    public string VarietyName { get; set; } = string.Empty;
    public string VarietySpecialities { get; set; } = string.Empty;
    public string VarietySuitabilityReason { get; set; } = string.Empty;
    public string VarietySoilTypeRecommendation { get; set; } = string.Empty;
    public string VarietySpacingMeters { get; set; } = string.Empty;
    public int? VarietyVinesPerHectare { get; set; }
    public string VarietyPitDimensionsCm { get; set; } = string.Empty;
    public List<GuideStepDto> Steps { get; set; } = new();
}
