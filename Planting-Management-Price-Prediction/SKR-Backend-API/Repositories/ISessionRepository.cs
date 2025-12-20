using SKR_Backend_API.Models;

namespace SKR_Backend_API.Repositories;

public interface ISessionRepository
{
    Task<Session> CreateAsync(Session session);
    Task<IEnumerable<Session>> GetBySeasonIdAsync(string seasonId);
    Task<Session?> GetByIdAsync(string sessionId);
    Task<Session?> UpdateAsync(string sessionId, Session session);
    Task<bool> DeleteAsync(string sessionId);
}

