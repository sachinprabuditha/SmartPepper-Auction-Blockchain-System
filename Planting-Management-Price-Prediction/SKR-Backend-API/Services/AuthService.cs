using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SKR_Backend_API.DTOs;
using SKR_Backend_API.Models;
using SKR_Backend_API.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SKR_Backend_API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponseDto> SignUpAsync(SignUpDto signUpDto)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(signUpDto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(signUpDto.Password);

        // Create user
        var user = new User
        {
            Email = signUpDto.Email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = signUpDto.FullName,
            PhoneNumber = signUpDto.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.CreateAsync(user);

        // Generate JWT token
        var token = GenerateJwtToken(createdUser);

        return new AuthResponseDto
        {
            UserId = createdUser.Id.ToString(),
            Email = createdUser.Email,
            FullName = createdUser.FullName,
            PhoneNumber = createdUser.PhoneNumber,
            Token = token
        };
    }

    public async Task<AuthResponseDto> SignInAsync(SignInDto signInDto)
    {
        try
        {
            _logger.LogInformation("Attempting to sign in user with email: {Email}", signInDto.Email);
            
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(signInDto.Email);
            if (user == null)
            {
                _logger.LogWarning("User not found with email: {Email}", signInDto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            _logger.LogInformation("User found: {UserId}, verifying password", user.Id);

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(signInDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Password verification failed for user: {Email}", signInDto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            _logger.LogInformation("Password verified successfully for user: {Email}", signInDto.Email);

            // Generate JWT token
            var token = GenerateJwtToken(user);

            _logger.LogInformation("JWT token generated successfully for user: {Email}", signInDto.Email);

            return new AuthResponseDto
            {
                UserId = user.Id.ToString(),
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Token = token
            };
        }
        catch (UnauthorizedAccessException)
        {
            // Re-throw authentication exceptions
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign in for email: {Email}", signInDto.Email);
            throw;
        }
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!";
        var issuer = jwtSettings["Issuer"] ?? "HarvestTrackingAPI";
        var audience = jwtSettings["Audience"] ?? "HarvestTrackingClient";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "1440"); // Default 24 hours

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

