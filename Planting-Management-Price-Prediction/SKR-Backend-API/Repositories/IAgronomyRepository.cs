using SKR_Backend_API.Models;

namespace SKR_Backend_API.Repositories;

public interface IAgronomyRepository
{
    Task<List<SoilType>> GetSoilsByDistrictAsync(int districtId);
    Task<AgronomyGuide?> GetGuideAsync(int districtId, int soilTypeId, string varietyId);
    Task<List<District>> GetAllDistrictsAsync();
    Task<List<BlackPepperVariety>> GetVarietiesByDistrictAndSoilAsync(int districtId, int soilTypeId);
    Task<List<AgronomyGuide>> GetAllGuidesByDistrictAndSoilAsync(int districtId, int soilTypeId);
    Task<List<AgronomyGuide>> SearchGuidesAsync(int districtId, int? soilTypeId);
}

