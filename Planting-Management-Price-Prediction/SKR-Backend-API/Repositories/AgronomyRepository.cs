using Microsoft.EntityFrameworkCore;
using SKR_Backend_API.Data;
using SKR_Backend_API.Models;

namespace SKR_Backend_API.Repositories;

public class AgronomyRepository : IAgronomyRepository
{
    private readonly AppDbContext _context;

    public AgronomyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SoilType>> GetSoilsByDistrictAsync(int districtId)
    {
        return await _context.DistrictSoils
            .Where(ds => ds.DistrictId == districtId)
            .Include(ds => ds.SoilType)
            .Select(ds => ds.SoilType)
            .ToListAsync();
    }

    public async Task<AgronomyGuide?> GetGuideAsync(int districtId, int soilTypeId, string varietyId)
    {
        var guide = await _context.AgronomyGuides
            .Where(ag => ag.DistrictId == districtId 
                && ag.SoilTypeId == soilTypeId 
                && ag.VarietyId == varietyId)
            .Include(ag => ag.District)
            .Include(ag => ag.SoilType)
            .Include(ag => ag.Variety)
            .Include(ag => ag.Steps)
            .FirstOrDefaultAsync();
        
        if (guide != null)
        {
            // Order steps by StepNumber after materializing
            guide.Steps = guide.Steps.OrderBy(s => s.StepNumber).ToList();
        }
        
        return guide;
    }

    public async Task<List<District>> GetAllDistrictsAsync()
    {
        return await _context.Districts
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<List<BlackPepperVariety>> GetVarietiesByDistrictAndSoilAsync(int districtId, int soilTypeId)
    {
        return await _context.AgronomyGuides
            .Where(ag => ag.DistrictId == districtId && ag.SoilTypeId == soilTypeId)
            .Include(ag => ag.Variety)
            .Select(ag => ag.Variety)
            .Distinct()
            .OrderBy(v => v.Name)
            .ToListAsync();
    }

    public async Task<List<AgronomyGuide>> GetAllGuidesByDistrictAndSoilAsync(int districtId, int soilTypeId)
    {
        var guides = await _context.AgronomyGuides
            .Where(ag => ag.DistrictId == districtId && ag.SoilTypeId == soilTypeId)
            .Include(ag => ag.District)
            .Include(ag => ag.SoilType)
            .Include(ag => ag.Variety)
            .Include(ag => ag.Steps)
            .ToListAsync();
        
        // Order guides by variety name and steps by step number after materializing
        guides = guides.OrderBy(ag => ag.Variety?.Name ?? string.Empty).ToList();
        foreach (var guide in guides)
        {
            if (guide.Steps != null)
            {
                guide.Steps = guide.Steps.OrderBy(s => s.StepNumber).ToList();
            }
        }
        
        return guides;
    }

    public async Task<List<AgronomyGuide>> SearchGuidesAsync(int districtId, int? soilTypeId)
    {
        var query = _context.AgronomyGuides
            .Where(ag => ag.DistrictId == districtId)
            .Include(ag => ag.District)
            .Include(ag => ag.SoilType)
            .Include(ag => ag.Variety)
            .Include(ag => ag.Steps)
            .AsQueryable();

        if (soilTypeId.HasValue)
        {
            query = query.Where(ag => ag.SoilTypeId == soilTypeId.Value);
        }

        var guides = await query.ToListAsync();
        
        // Order guides by variety name and steps by step number after materializing
        guides = guides.OrderBy(ag => ag.Variety?.Name ?? string.Empty).ToList();
        foreach (var guide in guides)
        {
            if (guide.Steps != null)
            {
                guide.Steps = guide.Steps.OrderBy(s => s.StepNumber).ToList();
            }
        }
        
        return guides;
    }
}

