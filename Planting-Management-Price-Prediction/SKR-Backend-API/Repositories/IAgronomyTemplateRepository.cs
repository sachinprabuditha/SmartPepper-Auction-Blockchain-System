using SKR_Backend_API.Models;

namespace SKR_Backend_API.Repositories;

public interface IAgronomyTemplateRepository
{
    Task<IEnumerable<AgronomyTemplate>> GetByVarietyKeyAsync(string varietyKey);
    Task<IEnumerable<AgronomyTemplate>> GetAllAsync();
    Task<AgronomyTemplate> CreateAsync(AgronomyTemplate template);
}

