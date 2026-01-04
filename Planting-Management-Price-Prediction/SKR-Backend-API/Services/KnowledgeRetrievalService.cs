using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using SKR_Backend_API.Data;
using SKR_Backend_API.Models;

namespace SKR_Backend_API.Services;

public interface IKnowledgeRetrievalService
{
    Task<List<PepperKnowledge>> SearchAsync(
        Vector embedding, 
        int? districtId, 
        string? varietyId, 
        int? plantAgeMonths, 
        string? category, 
        SoftContext? softContext);
}

public class KnowledgeRetrievalService : IKnowledgeRetrievalService
{
    private readonly AppDbContext _context;

    public KnowledgeRetrievalService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PepperKnowledge>> SearchAsync(
        Vector embedding, 
        int? hardDistrictId, 
        string? hardVarietyId, 
        int? hardPlantAgeMonths, 
        string? category,
        SoftContext? softContext)
    {
        // 1. Resolve Hard Context (Farmer Mode)
        string? hardDistrictName = null;
        if (hardDistrictId.HasValue)
        {
            var district = await _context.Districts.FindAsync(hardDistrictId.Value);
            hardDistrictName = district?.Name;
        }

        string? hardVarietyName = null;
        if (!string.IsNullOrEmpty(hardVarietyId))
        {
            var variety = await _context.PepperVarieties.FindAsync(hardVarietyId);
            hardVarietyName = variety?.Name;
        }

        int currentMonth = DateTime.UtcNow.Month;
        
        var query = _context.PepperKnowledge.AsQueryable();

        // 2. APPLY HARD FILTERS (Strict WHERE Clause)
        // Rule: Only filter if constraints are explicitly provided strings/ints.
        
        query = query.Where(k => 
            // Category Lock (Strict - No NULL allowed)
            (k.Category == category) &&

            // District: If hard set, must match (or be generic). If null (Guide Mode), ignore.
            (hardDistrictName == null 
                ? true 
                : k.District == hardDistrictName || k.District == null) &&

            // Variety: If hard set, must match. If null (Guide Mode), ignore.
            // NO AUTO-FALLBACK TO LOCAL.
            (hardVarietyName == null 
                ? true 
                : k.Variety == hardVarietyName || k.Variety == null) &&
            
            // Age: If hard set, match range.
            (hardPlantAgeMonths == null 
                ? true 
                : (k.PlantAgeMin == null || k.PlantAgeMin <= hardPlantAgeMonths.Value) && 
                  (k.PlantAgeMax == null || k.PlantAgeMax >= hardPlantAgeMonths.Value)) &&
            
            // Season: Always apply if data exists
            (k.MonthStart == null || k.MonthEnd == null 
                ? true 
                : currentMonth >= k.MonthStart && currentMonth <= k.MonthEnd)
        );
        
        // 3. APPLY SOFT RANKING (Boost Score in ORDER BY)
        // Logic: Calculate a 'relevance penalty' score. 0 = Best Match. Higher = Worse.
        // We sort by (Penalty ASC, VectorDistance ASC).
        
        var candidates = await query
            .OrderBy(k => k.Embedding!.L2Distance(embedding))
            .Take(50) // Fetch strictly more than needed to allow re-ranking
            .ToListAsync();

        // 4. In-Memory Soft Ranking
        if (softContext != null)
        {
            candidates = candidates.OrderBy(k => 
            {
                int penalty = 0;

                // Boost District Match (Balanced Weights + Text Fallback)
                if (!string.IsNullOrEmpty(softContext.DistrictName))
                {
                    if (k.District == softContext.DistrictName) 
                    {
                        penalty -= 5; // Strong Boost (Metadata Match)
                    }
                    else if (k.District != null) 
                    {
                        penalty += 3; // Minor Penalty (Wrong District)
                    }
                    else 
                    {
                        // Text-Based Soft Match (Fallback for missing metadata)
                        // If "Hambantota" appears in Title or Content, boost it significantly.
                        bool textMatch = (k.Title != null && k.Title.Contains(softContext.DistrictName, StringComparison.OrdinalIgnoreCase)) ||
                                         (k.Content != null && k.Content.Contains(softContext.DistrictName, StringComparison.OrdinalIgnoreCase));
                        
                        if (textMatch) penalty -= 3; // Content Match Boost
                    }
                }

                // Boost Variety Match (Balanced Weights + Text Fallback)
                if (!string.IsNullOrEmpty(softContext.VarietyName))
                {
                    if (k.Variety == softContext.VarietyName) 
                    {
                        penalty -= 5;
                    }
                    else if (k.Variety != null) 
                    {
                        penalty += 3;
                    }
                    else
                    {
                        // Text-Based Check
                        bool textMatch = (k.Title != null && k.Title.Contains(softContext.VarietyName, StringComparison.OrdinalIgnoreCase)) ||
                                         (k.Content != null && k.Content.Contains(softContext.VarietyName, StringComparison.OrdinalIgnoreCase));
                                         
                         if (textMatch) penalty -= 3;
                    }
                }

                // Boost Plant Age Match (New Logic)
                if (softContext.PlantAgeMonths.HasValue)
                {
                    if ((k.PlantAgeMin == null || k.PlantAgeMin <= softContext.PlantAgeMonths.Value) && 
                        (k.PlantAgeMax == null || k.PlantAgeMax >= softContext.PlantAgeMonths.Value))
                    {
                        penalty -= 4; // Boost applicable age
                    }
                    else if (k.PlantAgeMin != null || k.PlantAgeMax != null)
                    {
                        penalty += 2; // Penalty if age mismatch (but generic allows)
                    }
                }

                return penalty; 
            })
            // Secondary sort by Vector Distance
            .ToList();
        }

        return candidates.Take(15).ToList();
    }
}
