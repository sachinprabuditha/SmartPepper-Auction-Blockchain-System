using Microsoft.EntityFrameworkCore;
using SKR_Backend_API.Data;
using SKR_Backend_API.Models;

namespace SKR_Backend_API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> CreateAsync(User user)
    {
        // Ensure email is lowercase
        user.Email = user.Email.ToLowerInvariant();
        
        // Generate new Guid if not set
        if (user.Id == Guid.Empty)
        {
            user.Id = Guid.NewGuid();
        }
        
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            var lowerEmail = email.ToLowerInvariant();
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == lowerEmail);
        }
        catch (Exception ex)
        {
            // Log the exception for debugging
            throw new InvalidOperationException($"Error querying user by email: {email}", ex);
        }
    }

    public async Task<User?> GetByIdAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var guidId))
        {
            return null;
        }
        
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == guidId);
    }
}

