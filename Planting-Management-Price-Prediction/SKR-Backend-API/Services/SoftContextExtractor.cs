using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SKR_Backend_API.Data;
using SKR_Backend_API.Models;

namespace SKR_Backend_API.Services;

public interface ISoftContextExtractor
{
    Task<SoftContext> ExtractAsync(string message);
}

public class SoftContextExtractor : ISoftContextExtractor
{
    private readonly AppDbContext _context;
    
    public SoftContextExtractor(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SoftContext> ExtractAsync(string message)
    {
        var context = new SoftContext();
        var msgLower = message.ToLowerInvariant();

        // 1. Extract District (Database Lookup)
        // We cache this in production, but direct DB check is fine for now given low volume.
        var districts = await _context.Districts.ToListAsync();
        foreach (var d in districts)
        {
            if (msgLower.Contains(d.Name.ToLowerInvariant()))
            {
                context.DistrictName = d.Name;
                break; // Take first match
            }
        }

        // 2. Extract Variety (Database Lookup)
        var varieties = await _context.PepperVarieties.ToListAsync();
        foreach (var v in varieties)
        {
            // Check Name (e.g., "Kuching")
            if (msgLower.Contains(v.Name.ToLowerInvariant()))
            {
                context.VarietyName = v.Name;
                break;
            }
        }

        // 3. Extract Plant Age (Regex)
        // Patterns: "5 months", "1.5 years", "2 year"
        
        // Months pattern
        var monthMatch = Regex.Match(msgLower, @"(\d+)\s*month");
        if (monthMatch.Success && int.TryParse(monthMatch.Groups[1].Value, out int months))
        {
            context.PlantAgeMonths = months;
        }
        else
        {
            // Years pattern
            var yearMatch = Regex.Match(msgLower, @"(\d+)\s*year");
            if (yearMatch.Success && int.TryParse(yearMatch.Groups[1].Value, out int years))
            {
                context.PlantAgeMonths = years * 12;
            }
        }

        return context;
    }
}
