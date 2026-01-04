using System.Collections.Generic;

namespace SKR_Backend_API.DTOs;

public class RagChatResponse
{
    public string Reply { get; set; } = string.Empty;
    public List<string> Sources { get; set; } = new List<string>();
}
