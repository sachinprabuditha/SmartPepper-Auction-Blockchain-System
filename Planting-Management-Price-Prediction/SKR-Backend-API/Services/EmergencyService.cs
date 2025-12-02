using SKR_Backend_API.Models;
using SKR_Backend_API.Repositories;

namespace SKR_Backend_API.Services;

public class EmergencyService : IEmergencyService
{
    private readonly IEmergencyTemplateRepository _repository;

    public EmergencyService(IEmergencyTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<EmergencyTemplate>> GetAllEmergencyTemplatesAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<IEnumerable<EmergencyTemplate>> SearchEmergencyTemplatesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await _repository.GetAllAsync();
        }
        return await _repository.SearchAsync(searchTerm);
    }

    public async Task<EmergencyTemplate?> GetEmergencyTemplateByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }
}

