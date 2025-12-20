using Microsoft.EntityFrameworkCore;
using SKR_Backend_API.Data;
using SKR_Backend_API.Models;

namespace SKR_Backend_API.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly AppDbContext _context;

    public SessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Session> CreateAsync(Session session)
    {
        if (session.Id == Guid.Empty)
        {
            session.Id = Guid.NewGuid();
        }
        try
        {
            _context.HarvestSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            var errorMessage = $"Database error creating session: {dbEx.Message}";
            if (dbEx.InnerException != null)
            {
                errorMessage += $" Inner: {dbEx.InnerException.Message}";
            }
            System.Diagnostics.Debug.WriteLine($"Error in SessionRepository.CreateAsync: {errorMessage}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {dbEx.StackTrace}");
            throw new Exception(errorMessage, dbEx);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in SessionRepository.CreateAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    public async Task<IEnumerable<Session>> GetBySeasonIdAsync(string seasonId)
    {
        if (!Guid.TryParse(seasonId, out var guidSeasonId))
        {
            return Enumerable.Empty<Session>();
        }

        return await _context.HarvestSessions
            .AsNoTracking()
            .Where(s => s.SeasonId == guidSeasonId)
            .ToListAsync();
    }

    public async Task<Session?> GetByIdAsync(string sessionId)
    {
        if (!Guid.TryParse(sessionId, out var guidSessionId))
        {
            return null;
        }

        return await _context.HarvestSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == guidSessionId);
    }

    public async Task<Session?> UpdateAsync(string sessionId, Session session)
    {
        if (!Guid.TryParse(sessionId, out var guidSessionId))
        {
            return null;
        }

        var existingSession = await _context.HarvestSessions
            .FirstOrDefaultAsync(s => s.Id == guidSessionId);

        if (existingSession == null)
        {
            return null;
        }

        // Update properties
        existingSession.SessionName = session.SessionName;
        existingSession.Date = session.Date;
        existingSession.YieldKg = session.YieldKg;
        existingSession.AreaHarvested = session.AreaHarvested;
        existingSession.Notes = session.Notes;

        await _context.SaveChangesAsync();
        return existingSession;
    }

    public async Task<bool> DeleteAsync(string sessionId)
    {
        if (!Guid.TryParse(sessionId, out var guidSessionId))
        {
            return false;
        }

        var session = await _context.HarvestSessions
            .FirstOrDefaultAsync(s => s.Id == guidSessionId);

        if (session == null)
        {
            return false;
        }

        _context.HarvestSessions.Remove(session);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}

