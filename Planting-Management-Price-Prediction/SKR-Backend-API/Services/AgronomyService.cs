using SKR_Backend_API.DTOs;
using SKR_Backend_API.Models;
using SKR_Backend_API.Repositories;

namespace SKR_Backend_API.Services;

public class AgronomyService : IAgronomyService
{
    private readonly IAgronomyRepository _agronomyRepository;
    private readonly IVarietyRepository _varietyRepository;

    public AgronomyService(IAgronomyRepository agronomyRepository, IVarietyRepository varietyRepository)
    {
        _agronomyRepository = agronomyRepository;
        _varietyRepository = varietyRepository;
    }

    public async Task<List<SoilTypeDto>> GetSoilsByDistrictAsync(int districtId)
    {
        var soils = await _agronomyRepository.GetSoilsByDistrictAsync(districtId);
        return soils.Select(s => new SoilTypeDto
        {
            Id = s.Id,
            TypeName = s.TypeName
        }).ToList();
    }

    public async Task<AgronomyGuideResponseDto?> GetGuideAsync(int districtId, int soilTypeId, string varietyId)
    {
        var guide = await _agronomyRepository.GetGuideAsync(districtId, soilTypeId, varietyId);
        if (guide == null)
            return null;

        return new AgronomyGuideResponseDto
        {
            Id = guide.Id,
            DistrictId = guide.DistrictId,
            DistrictName = guide.District.Name,
            SoilTypeId = guide.SoilTypeId,
            SoilTypeName = guide.SoilType.TypeName,
            VarietyId = guide.VarietyId,
            VarietyName = guide.Variety.Name,
            VarietySpecialities = guide.Variety.Specialities ?? string.Empty,
            VarietySuitabilityReason = guide.Variety.SuitabilityReason ?? string.Empty,
            VarietySoilTypeRecommendation = guide.Variety.SoilTypeRecommendation ?? string.Empty,
            VarietySpacingMeters = guide.Variety.SpacingMeters ?? string.Empty,
            VarietyVinesPerHectare = guide.Variety.VinesPerHectare,
            VarietyPitDimensionsCm = guide.Variety.PitDimensionsCm ?? string.Empty,
            Steps = guide.Steps.Select(s => new GuideStepDto
            {
                Id = s.Id,
                StepNumber = s.StepNumber,
                Title = s.Title,
                Details = s.Details
            }).ToList()
        };
    }

    public async Task<List<District>> GetAllDistrictsAsync()
    {
        return await _agronomyRepository.GetAllDistrictsAsync();
    }

    public async Task<List<BlackPepperVariety>> GetVarietiesByDistrictAndSoilAsync(int districtId, int soilTypeId)
    {
        return await _agronomyRepository.GetVarietiesByDistrictAndSoilAsync(districtId, soilTypeId);
    }

    public async Task<List<AgronomyGuideResponseDto>> GetAllGuidesByDistrictAndSoilAsync(int districtId, int soilTypeId)
    {
        return await SearchGuidesAsync(districtId, soilTypeId);
    }

    public async Task<List<AgronomyGuideResponseDto>> SearchGuidesAsync(int districtId, int? soilTypeId)
    {
        try
        {
            var guides = await _agronomyRepository.SearchGuidesAsync(districtId, soilTypeId);
            
            var result = new List<AgronomyGuideResponseDto>();
            foreach (var guide in guides)
            {
                try
                {
                    result.Add(new AgronomyGuideResponseDto
                    {
                        Id = guide.Id,
                        DistrictId = guide.DistrictId,
                        DistrictName = guide.District?.Name ?? string.Empty,
                        SoilTypeId = guide.SoilTypeId,
                        SoilTypeName = guide.SoilType?.TypeName ?? string.Empty,
                        VarietyId = guide.VarietyId,
                        VarietyName = guide.Variety?.Name ?? string.Empty,
                        VarietySpecialities = guide.Variety?.Specialities ?? string.Empty,
                        VarietySuitabilityReason = guide.Variety?.SuitabilityReason ?? string.Empty,
                        VarietySoilTypeRecommendation = guide.Variety?.SoilTypeRecommendation ?? string.Empty,
                        VarietySpacingMeters = guide.Variety?.SpacingMeters ?? string.Empty,
                        VarietyVinesPerHectare = guide.Variety?.VinesPerHectare,
                        VarietyPitDimensionsCm = guide.Variety?.PitDimensionsCm ?? string.Empty,
                        Steps = guide.Steps?.Select(s => new GuideStepDto
                        {
                            Id = s.Id,
                            StepNumber = s.StepNumber,
                            Title = s.Title,
                            Details = s.Details
                        }).ToList() ?? new List<GuideStepDto>()
                    });
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error mapping guide {guide.Id}: {ex.Message}", ex);
                }
            }
            
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error in SearchGuidesAsync: {ex.Message}", ex);
        }
    }

    public async Task<List<BlackPepperVariety>> GetVarietiesByIdsAsync(List<string> ids)
    {
        if (ids == null || ids.Count == 0)
            return new List<BlackPepperVariety>();

        return await _varietyRepository.GetByIdsAsync(ids);
    }

    public async Task<BlackPepperVariety?> GetVarietyByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        return await _varietyRepository.GetByIdAsync(id);
    }

    public async Task<List<BlackPepperVariety>> GetAllVarietiesAsync()
    {
        return await _varietyRepository.GetAllAsync();
    }
}

