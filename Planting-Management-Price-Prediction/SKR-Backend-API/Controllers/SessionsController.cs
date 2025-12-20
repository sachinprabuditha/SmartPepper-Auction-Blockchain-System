using Microsoft.AspNetCore.Mvc;
using SKR_Backend_API.DTOs;
using SKR_Backend_API.Models;
using SKR_Backend_API.Services;

namespace SKR_Backend_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(ISessionService sessionService, ILogger<SessionsController> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new harvesting session
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Session>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Session>>> CreateSession([FromBody] CreateSessionDto createDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.ErrorResponse($"Validation failed: {string.Join(", ", errors)}"));
        }

        try
        {
            var session = await _sessionService.CreateSessionAsync(createDto);
            return CreatedAtAction(nameof(GetSessionById), new { sessionId = session.Id.ToString() },
                ApiResponse<Session>.SuccessResponse(session, "Session created successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            var errorMessage = $"Database error: {dbEx.Message}";
            if (dbEx.InnerException != null)
            {
                errorMessage += $" Inner: {dbEx.InnerException.Message}";
                _logger.LogError(dbEx.InnerException, "Database inner exception: {Message}", dbEx.InnerException.Message);
            }
            _logger.LogError(dbEx, "Database error creating session: {Message}", dbEx.Message);
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"An error occurred while creating the session: {errorMessage}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session: {Message}", ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError(ex.InnerException, "Inner exception: {Message}", ex.InnerException.Message);
            }
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"An error occurred while creating the session: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get all sessions for a specific season
    /// </summary>
    [HttpGet("season/{seasonId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Session>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<Session>>>> GetSessionsBySeasonId(string seasonId)
    {
        try
        {
            var sessions = await _sessionService.GetSessionsBySeasonIdAsync(seasonId);
            return Ok(ApiResponse<IEnumerable<Session>>.SuccessResponse(sessions, "Sessions retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sessions for season {SeasonId}", seasonId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving sessions"));
        }
    }

    /// <summary>
    /// Get a session by ID
    /// </summary>
    [HttpGet("{sessionId}")]
    [ProducesResponseType(typeof(ApiResponse<Session>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Session>>> GetSessionById(string sessionId)
    {
        try
        {
            var session = await _sessionService.GetSessionByIdAsync(sessionId);
            if (session == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Session not found"));
            }

            return Ok(ApiResponse<Session>.SuccessResponse(session, "Session retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session {SessionId}", sessionId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving the session"));
        }
    }

    /// <summary>
    /// Update a session
    /// </summary>
    [HttpPut("{sessionId}")]
    [ProducesResponseType(typeof(ApiResponse<Session>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Session>>> UpdateSession(string sessionId, [FromBody] UpdateSessionDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.ErrorResponse($"Validation failed: {string.Join(", ", errors)}"));
        }

        try
        {
            var session = await _sessionService.UpdateSessionAsync(sessionId, updateDto);
            if (session == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Session not found"));
            }

            return Ok(ApiResponse<Session>.SuccessResponse(session, "Session updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session {SessionId}", sessionId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating the session"));
        }
    }

    /// <summary>
    /// Delete a session
    /// </summary>
    [HttpDelete("{sessionId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSession(string sessionId)
    {
        try
        {
            var deleted = await _sessionService.DeleteSessionAsync(sessionId);
            if (!deleted)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Session not found"));
            }

            return Ok(ApiResponse<object>.SuccessResponse("Session deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session {SessionId}", sessionId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting the session"));
        }
    }
}

