using SKR_Backend_API.DTOs;
using SKR_Backend_API.Models;
using SKR_Backend_API.Repositories;

namespace SKR_Backend_API.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ISeasonRepository _seasonRepository;

    public SessionService(ISessionRepository sessionRepository, ISeasonRepository seasonRepository)
    {
        _sessionRepository = sessionRepository;
        _seasonRepository = seasonRepository;
    }

    public async Task<Session> CreateSessionAsync(CreateSessionDto createDto)
    {
        // Validate that the season exists
        var season = await _seasonRepository.GetByIdAsync(createDto.SeasonId);
        if (season == null)
            throw new ArgumentException("Season not found", nameof(createDto.SeasonId));

        // Validate Status
        if (season.Status == "season-end")
        {
            throw new InvalidOperationException("Cannot add session to an ended season");
        }

        // Convert string SeasonId to Guid
        if (!Guid.TryParse(createDto.SeasonId, out var guidSeasonId))
        {
            throw new ArgumentException("Invalid season ID format", nameof(createDto.SeasonId));
        }

        // Convert DateTime to UTC - PostgreSQL requires UTC for timestamp with time zone
        DateTime dateUtc;
        if (createDto.Date.Kind == DateTimeKind.Utc)
        {
            dateUtc = createDto.Date;
        }
        else if (createDto.Date.Kind == DateTimeKind.Local)
        {
            dateUtc = createDto.Date.ToUniversalTime();
        }
        else // DateTimeKind.Unspecified
        {
            // Assume Unspecified dates are already in UTC and just specify the kind
            dateUtc = DateTime.SpecifyKind(createDto.Date, DateTimeKind.Utc);
        }

        var session = new Session
        {
            SeasonId = guidSeasonId,
            SessionName = createDto.SessionName,
            Date = dateUtc,
            YieldKg = (decimal)createDto.YieldKg,
            AreaHarvested = (decimal)createDto.AreaHarvested,
            Notes = createDto.Notes
        };

        var createdSession = await _sessionRepository.CreateAsync(session);

        // Update Season Total Yield
        season.TotalHarvestedYield += session.YieldKg;
        await _seasonRepository.UpdateAsync(season.Id.ToString(), season);

        return createdSession;
    }

    public async Task<IEnumerable<Session>> GetSessionsBySeasonIdAsync(string seasonId)
    {
        return await _sessionRepository.GetBySeasonIdAsync(seasonId);
    }

    public async Task<Session?> GetSessionByIdAsync(string sessionId)
    {
        return await _sessionRepository.GetByIdAsync(sessionId);
    }

    public async Task<Session?> UpdateSessionAsync(string sessionId, UpdateSessionDto updateDto)
    {
        var existingSession = await _sessionRepository.GetByIdAsync(sessionId);
        if (existingSession == null)
            return null;

        if (!string.IsNullOrWhiteSpace(updateDto.SessionName))
            existingSession.SessionName = updateDto.SessionName;

        if (updateDto.Date.HasValue)
        {
            // Convert DateTime to UTC - PostgreSQL requires UTC for timestamp with time zone
            DateTime dateUtc;
            if (updateDto.Date.Value.Kind == DateTimeKind.Utc)
            {
                dateUtc = updateDto.Date.Value;
            }
            else if (updateDto.Date.Value.Kind == DateTimeKind.Local)
            {
                dateUtc = updateDto.Date.Value.ToUniversalTime();
            }
            else // DateTimeKind.Unspecified
            {
                // Assume Unspecified dates are already in UTC and just specify the kind
                dateUtc = DateTime.SpecifyKind(updateDto.Date.Value, DateTimeKind.Utc);
            }
            
            existingSession.Date = dateUtc;
        }

        if (updateDto.YieldKg.HasValue)
            existingSession.YieldKg = (decimal)updateDto.YieldKg.Value;

        if (updateDto.AreaHarvested.HasValue)
            existingSession.AreaHarvested = (decimal)updateDto.AreaHarvested.Value;

        if (updateDto.Notes != null)
            existingSession.Notes = updateDto.Notes;

        return await _sessionRepository.UpdateAsync(sessionId, existingSession);
    }

    public async Task<bool> DeleteSessionAsync(string sessionId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null) return false;

        var deleted = await _sessionRepository.DeleteAsync(sessionId);
        if (deleted)
        {
            var season = await _seasonRepository.GetByIdAsync(session.SeasonId.ToString());
            if (season != null)
            {
                season.TotalHarvestedYield -= session.YieldKg;
                if (season.TotalHarvestedYield < 0) season.TotalHarvestedYield = 0; // Prevent negative yield
                await _seasonRepository.UpdateAsync(season.Id.ToString(), season);
            }
        }

        return deleted;
    }
}

