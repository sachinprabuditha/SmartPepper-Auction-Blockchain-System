using SKR_Backend_API.DTOs;

namespace SKR_Backend_API.Services;

public interface IChatService
{
    Task<RagChatResponse> ProcessMessageAsync(RagChatRequest request);
}
