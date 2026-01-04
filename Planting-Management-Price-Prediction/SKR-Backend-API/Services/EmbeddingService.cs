using OpenAI.Embeddings;

namespace SKR_Backend_API.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly EmbeddingClient _client;

    public EmbeddingService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI:ApiKey is not configured.");
        }
        _client = new EmbeddingClient("text-embedding-3-small", apiKey);
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        // Replace newlines to slightly improve performance/accuracy as per OpenAI guidelines
        var cleanText = text.Replace("\n", " ");
        
        var embedding = await _client.GenerateEmbeddingAsync(cleanText);
        return embedding.Value.ToFloats().ToArray();
    }
}
