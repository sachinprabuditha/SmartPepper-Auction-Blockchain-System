namespace SKR_Backend_API.DTOs;

public class RagChatRequest
{
    public string Message { get; set; } = string.Empty;
    public Guid? ActiveFarmId { get; set; } // Added for Context-Aware RAG
}
