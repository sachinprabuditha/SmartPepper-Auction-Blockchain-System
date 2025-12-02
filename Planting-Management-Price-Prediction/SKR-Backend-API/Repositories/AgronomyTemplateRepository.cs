using Microsoft.EntityFrameworkCore;
using SKR_Backend_API.Data;
using SKR_Backend_API.Models;

namespace SKR_Backend_API.Repositories;

public class AgronomyTemplateRepository : IAgronomyTemplateRepository
{
    private readonly AppDbContext _context;

    public AgronomyTemplateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AgronomyTemplate>> GetByVarietyKeyAsync(string varietyKey)
    {
        try
        {
            return await _context.AgronomyTemplates
                .Where(t => t.VarietyKey == "ALL" || t.VarietyKey == varietyKey)
                .OrderBy(t => t.TimingDaysAfterStartingOfFarm)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetByVarietyKeyAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    public async Task<IEnumerable<AgronomyTemplate>> GetAllAsync()
    {
        try
        {
            return await _context.AgronomyTemplates
                .OrderBy(t => t.TimingDaysAfterStartingOfFarm)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetAllAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    public async Task<AgronomyTemplate> CreateAsync(AgronomyTemplate template)
    {
        if (template.Id == Guid.Empty)
        {
            template.Id = Guid.NewGuid();
        }
        template.CreatedAt = DateTime.UtcNow;
        _context.AgronomyTemplates.Add(template);
        await _context.SaveChangesAsync();
        return template;
    }
}

