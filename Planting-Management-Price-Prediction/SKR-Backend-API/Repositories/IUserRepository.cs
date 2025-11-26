using SKR_Backend_API.Models;

namespace SKR_Backend_API.Repositories;

public interface IUserRepository
{
    Task<User> CreateAsync(User user);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(string userId);
}

