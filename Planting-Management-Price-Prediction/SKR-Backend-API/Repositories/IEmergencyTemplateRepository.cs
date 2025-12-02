using SKR_Backend_API.Models;

namespace SKR_Backend_API.Repositories;

public interface IEmergencyTemplateRepository
{
    Task<IEnumerable<EmergencyTemplate>> GetAllAsync();
    Task<EmergencyTemplate?> GetByIdAsync(string id);
    Task<IEnumerable<EmergencyTemplate>> SearchAsync(string searchTerm);
}

