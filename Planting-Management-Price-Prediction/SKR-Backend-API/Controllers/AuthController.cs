using Microsoft.AspNetCore.Mvc;
using SKR_Backend_API.DTOs;
using SKR_Backend_API.Models;
using SKR_Backend_API.Services;

namespace SKR_Backend_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("signup")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> SignUp([FromBody] SignUpDto signUpDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.ErrorResponse($"Validation failed: {string.Join(", ", errors)}"));
        }

        try
        {
            var authResponse = await _authService.SignUpAsync(signUpDto);
            return CreatedAtAction(nameof(SignUp), authResponse,
                ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "User registered successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign up");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while registering the user"));
        }
    }

    /// <summary>
    /// Sign in an existing user
    /// </summary>
    [HttpPost("signin")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> SignIn([FromBody] SignInDto signInDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.ErrorResponse($"Validation failed: {string.Join(", ", errors)}"));
        }

        try
        {
            var authResponse = await _authService.SignInAsync(signInDto);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "Sign in successful"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign in: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
            // In development, return more detailed error information
            var errorMessage = "An error occurred while signing in";
            if (ex.InnerException != null)
            {
                errorMessage += $": {ex.InnerException.Message}";
            }
            return StatusCode(500, ApiResponse<object>.ErrorResponse(errorMessage));
        }
    }
}

