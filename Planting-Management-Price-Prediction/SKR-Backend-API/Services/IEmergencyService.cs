using SKR_Backend_API.Models;

namespace SKR_Backend_API.Services;

public interface IEmergencyService
{
    Task<IEnumerable<EmergencyTemplate>> GetAllEmergencyTemplatesAsync();
    Task<IEnumerable<EmergencyTemplate>> SearchEmergencyTemplatesAsync(string searchTerm);
    Task<EmergencyTemplate?> GetEmergencyTemplateByIdAsync(string id);
}

