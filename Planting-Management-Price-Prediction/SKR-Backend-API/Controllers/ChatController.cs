using Microsoft.AspNetCore.Mvc;
using SKR_Backend_API.DTOs;
using SKR_Backend_API.Models;
using SKR_Backend_API.Services;

namespace SKR_Backend_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RagChatResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<RagChatResponse>>> SendMessage([FromBody] RagChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Message))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Message cannot be empty"));
        }

        try
        {
            var response = await _chatService.ProcessMessageAsync(request);
            return Ok(ApiResponse<RagChatResponse>.SuccessResponse(response, "Response generated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while processing your message"));
        }
    }
}
