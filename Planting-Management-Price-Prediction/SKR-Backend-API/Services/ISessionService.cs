using SKR_Backend_API.DTOs;
using SKR_Backend_API.Models;

namespace SKR_Backend_API.Services;

public interface ISessionService
{
    Task<Session> CreateSessionAsync(CreateSessionDto createDto);
    Task<IEnumerable<Session>> GetSessionsBySeasonIdAsync(string seasonId);
    Task<Session?> GetSessionByIdAsync(string sessionId);
    Task<Session?> UpdateSessionAsync(string sessionId, UpdateSessionDto updateDto);
    Task<bool> DeleteSessionAsync(string sessionId);
}

