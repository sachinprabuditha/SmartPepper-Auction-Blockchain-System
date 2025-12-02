using SKR_Backend_API.DTOs;
using SKR_Backend_API.Models;

namespace SKR_Backend_API.Services;

public interface IAgronomyService
{
    Task<List<SoilTypeDto>> GetSoilsByDistrictAsync(int districtId);
    Task<AgronomyGuideResponseDto?> GetGuideAsync(int districtId, int soilTypeId, string varietyId);
    Task<List<District>> GetAllDistrictsAsync();
    Task<List<BlackPepperVariety>> GetVarietiesByDistrictAndSoilAsync(int districtId, int soilTypeId);
    Task<List<AgronomyGuideResponseDto>> GetAllGuidesByDistrictAndSoilAsync(int districtId, int soilTypeId);
    Task<List<AgronomyGuideResponseDto>> SearchGuidesAsync(int districtId, int? soilTypeId);
    Task<List<BlackPepperVariety>> GetVarietiesByIdsAsync(List<string> ids);
    Task<BlackPepperVariety?> GetVarietyByIdAsync(string id);
    Task<List<BlackPepperVariety>> GetAllVarietiesAsync();
}

