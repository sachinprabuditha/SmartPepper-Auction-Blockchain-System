using SKR_Backend_API.Models;

namespace SKR_Backend_API.Repositories;

public interface IVarietyRepository
{
    Task<BlackPepperVariety?> GetByIdAsync(string id);
    Task<BlackPepperVariety?> GetByNameAsync(string name);
    Task<List<BlackPepperVariety>> GetByIdsAsync(List<string> ids);
    Task<List<BlackPepperVariety>> GetAllAsync();
}

