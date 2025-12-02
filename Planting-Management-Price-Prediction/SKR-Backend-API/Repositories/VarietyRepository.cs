using Microsoft.EntityFrameworkCore;
using SKR_Backend_API.Data;
using SKR_Backend_API.Models;

namespace SKR_Backend_API.Repositories;

public class VarietyRepository : IVarietyRepository
{
    private readonly AppDbContext _context;

    public VarietyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BlackPepperVariety?> GetByIdAsync(string id)
    {
        return await _context.PepperVarieties
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<BlackPepperVariety?> GetByNameAsync(string name)
    {
        return await _context.PepperVarieties
            .FirstOrDefaultAsync(v => v.Name == name);
    }

    public async Task<List<BlackPepperVariety>> GetByIdsAsync(List<string> ids)
    {
        return await _context.PepperVarieties
            .Where(v => ids.Contains(v.Id))
            .ToListAsync();
    }

    public async Task<List<BlackPepperVariety>> GetAllAsync()
    {
        try
        {
            return await _context.PepperVarieties.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error querying varieties: {ex.Message}", ex);
        }
    }
}

