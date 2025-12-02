using Microsoft.EntityFrameworkCore;
using SKR_Backend_API.Data;
using SKR_Backend_API.Models;

namespace SKR_Backend_API.Repositories;

public class EmergencyTemplateRepository : IEmergencyTemplateRepository
{
    private readonly AppDbContext _context;

    public EmergencyTemplateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmergencyTemplate>> GetAllAsync()
    {
        return await _context.EmergencyTemplates.ToListAsync();
    }

    public async Task<EmergencyTemplate?> GetByIdAsync(string id)
    {
        if (!Guid.TryParse(id, out var guidId))
        {
            return null;
        }

        return await _context.EmergencyTemplates
            .FirstOrDefaultAsync(t => t.Id == guidId);
    }

    public async Task<IEnumerable<EmergencyTemplate>> SearchAsync(string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return await _context.EmergencyTemplates
            .Where(t => 
                t.IssueName.ToLower().Contains(lowerSearchTerm) ||
                t.Symptoms.ToLower().Contains(lowerSearchTerm) ||
                t.TreatmentTask.ToLower().Contains(lowerSearchTerm))
            .ToListAsync();
    }
}

