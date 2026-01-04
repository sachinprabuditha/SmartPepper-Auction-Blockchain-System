using System.Text;
using OpenAI.Chat;
using SKR_Backend_API.Data;
using SKR_Backend_API.DTOs;
using SKR_Backend_API.Models;

namespace SKR_Backend_API.Services;

public class ChatService : IChatService
{
    private readonly AppDbContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly IKnowledgeRetrievalService _retrievalService;
    private readonly ISoftContextExtractor _contextExtractor;
    private readonly ChatClient? _chatClient;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        AppDbContext context,
        IEmbeddingService embeddingService,
        IKnowledgeRetrievalService retrievalService,
        ISoftContextExtractor contextExtractor,
        IConfiguration configuration,
        ILogger<ChatService> logger)
    {
        _context = context;
        _embeddingService = embeddingService;
        _retrievalService = retrievalService;
        _contextExtractor = contextExtractor;
        _logger = logger;
        
        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
             // Fallback or throw, but strict mode implies we need it.
             // For now, let's assume it's there or handle gracefully if not.
             _logger.LogWarning("OpenAI API Key is missing!");
        }
        else
        {
             _chatClient = new ChatClient("gpt-3.5-turbo", apiKey); 
        }
    }

    public async Task<RagChatResponse> ProcessMessageAsync(RagChatRequest request)
    {
        _logger.LogInformation("Processing message: {Message}", request.Message);

        // 1. Determine Context Structure
        // Farmer Mode: Hard Constraints (db IDs)
        // Guide Mode: Soft Constraints (extracted strings)
        
        int? hardDistrictId = null;
        string? hardVarietyId = null;
        int? hardPlantAgeMonths = null;
        SoftContext? softContext = null;

        if (request.ActiveFarmId.HasValue)
        {
            // --- FARMER MODE ---
            var farm = await _context.Farms.FindAsync(request.ActiveFarmId.Value);
            if (farm != null)
            {
                 hardDistrictId = farm.DistrictId;
                 hardVarietyId = farm.ChosenVarietyId;
                 
                 // Calculate Plant Age
                 if (farm.FarmStartDate.HasValue)
                 {
                     var now = DateTime.UtcNow;
                     var start = farm.FarmStartDate.Value; 
                     var startDate = start.Date;
                     hardPlantAgeMonths = ((now.Year - startDate.Year) * 12) + now.Month - startDate.Month;
                     if (hardPlantAgeMonths < 0) hardPlantAgeMonths = 0;
                 }
                 _logger.LogInformation("Farmer Mode: District={District}, Variety={Variety}, Age={Age}", hardDistrictId, hardVarietyId, hardPlantAgeMonths);
            }
        }
        else
        {
            // --- GUIDE MODE ---
            // Extract hypothetical context from user message
            softContext = await _contextExtractor.ExtractAsync(request.Message);
            _logger.LogInformation("Guide Mode: SoftContext Extracted -> District={District}, Variety={Variety}, Age={Age}", 
                softContext.DistrictName, softContext.VarietyName, softContext.PlantAgeMonths);
        }

        // 2. Intent Scoring (Azure-Style)
        var intentScores = DetectIntentScores(request.Message);
        
        // Filter: Top 2, Score >= 0.5 (Refinement 1: Lower threshold)
        var candidateIntents = intentScores
            .Where(i => i.Value >= 0.5)
            .OrderByDescending(i => i.Value)
            .Take(2)
            .ToList();

        _logger.LogInformation("Intent Scoring: {Scores}", string.Join(", ", candidateIntents.Select(i => $"{i.Key}={i.Value:F2}")));

        // CRITICAL ISSUE #2: Safety Edge Case (Conflicting Intents)
        // If top 2 intents are too close (< 0.1 diff), we treat it as ambiguous and refuse to guess.
        if (candidateIntents.Count > 1 && (candidateIntents[0].Value - candidateIntents[1].Value < 0.1))
        {
             _logger.LogWarning("Ambiguous Intent Blocked: {Intent1} ({Score1}) vs {Intent2} ({Score2})", 
                 candidateIntents[0].Key, candidateIntents[0].Value, 
                 candidateIntents[1].Key, candidateIntents[1].Value);
                 
             return new RagChatResponse
             {
                 Reply = "No official recommendation available for this condition.",
                 Sources = new List<string>()
             };
        }

        // STRICT LOCK: If no valid intent found, block execution
        if (!candidateIntents.Any())
        {
            return new RagChatResponse
            {
                Reply = "I can only answer questions about: Variety, Planting, Irrigation, Fertilizer, Soil Management, Disease, and Harvest. Please refine your question.",
                Sources = new List<string>()
            };
        }

        // 3. Generate Embedding
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(request.Message);
        var vector = new Pgvector.Vector(queryEmbedding);

        // 4. Parallel Retrieval (Category Locked x Candidates)
        // CRITICAL ISSUE #1: Capture Rank explicitly to preserve vector relevance
        var allKnowledgeItems = new List<(PepperKnowledge Item, double IntentScore, int Rank)>();

        foreach (var intent in candidateIntents)
        {
            // Retrieve for THIS specific intent (Enforcing Category Lock per search)
            var results = await _retrievalService.SearchAsync(
                vector, 
                hardDistrictId, 
                hardVarietyId, 
                hardPlantAgeMonths, 
                intent.Key, 
                softContext
            );

            // Tag results with the Intent Score AND their original Rank (0-based index)
            for (int i = 0; i < results.Count; i++)
            {
                allKnowledgeItems.Add((results[i], intent.Value, i));
            }
        }
        
        // 5. Unified Ranking & Merging
        // CRITICAL ISSUE #1 Fix: Use .ThenBy(x => x.Rank)
        var finalResults = allKnowledgeItems
            .GroupBy(x => x.Item.Id) // Deduplicate
            .Select(g => g.First())
            .OrderByDescending(x => x.IntentScore) // Primary: Intent Confidence
            .ThenBy(x => x.Rank)                   // Secondary: Vector + Soft Ranking Preserved
            .Select(x => x.Item)
            .Take(15)
            .ToList();

        // Refinement 3: Log Final Decision
        var primaryIntent = candidateIntents.First().Key;
        _logger.LogInformation("Final selection: PrimaryIntent={Intent}, DocsFound={Count}", primaryIntent, finalResults.Count);

        // 6. Answerability Gate (CRITICAL)
        if (!finalResults.Any())
        {
             return new RagChatResponse 
            { 
                Reply = "No official recommendation available for this condition.",
                Sources = new List<string>() 
            };
        }
        
        // 7. Construct Prompt
        var sb = new StringBuilder();
        
        foreach (var k in finalResults)
        {
            sb.AppendLine($"- [{k.Category.ToUpper()}] {k.Content}");
            if (k.ConfidenceLevel == "Low")
            {
                sb.AppendLine("  (Note: General guideline only)");
            }
        }
        string knowledgeText = sb.ToString();

        var systemPrompt = @$"You are a Sri Lankan black pepper farming assistant.

CONTEXT:
 The user is asking about: {primaryIntent.ToUpper()} (and possibly related topics).
 You must Use ONLY the provided knowledge which has been strictly filtered.

RULES:
- Use ONLY the provided knowledge.
- Do NOT use external knowledge.
- Do NOT guess or generalize.
- If the knowledge does NOT explicitly answer the specific question, say EXACTLY:
  'No official recommendation available for this condition.'
- Keep answers short, practical, and farmer-friendly";

        var userPrompt = $@"Knowledge:
{knowledgeText}

Question:
{request.Message}";

        // 8. Generate Response
        if (_chatClient == null)
        {
             return new RagChatResponse 
             { 
                 Reply = "AI Service is currently unavailable (API Key missing).",
                 Sources = new List<string>() 
             };
        }

        ChatCompletion completion = await _chatClient.CompleteChatAsync(new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        });

        _logger.LogInformation("Generated Response for intent {Intent}", primaryIntent);

        return new RagChatResponse
        {
            Reply = completion.Content[0].Text,
            Sources = finalResults.Select(k => k.Title).Distinct().ToList()
        };
    }

    private Dictionary<string, double> DetectIntentScores(string message)
    {
        var msgLower = message.ToLowerInvariant();
        var scores = new Dictionary<string, double>();
        
        // Dictionary Mapping with Weights
        // 1.0 = Direct Keyword (Strong)
        // 0.8 = Related Term (Medium)
        var categoryMap = new Dictionary<string, Dictionary<string, double>>
        {
            { "fertilizer", new() { { "fertil", 1.0 }, { "urea", 1.0 }, { "npk", 1.0 }, { "compost", 0.9 }, { "nutrient", 0.7 } } },
            { "soil_management", new() { { "soil", 1.0 }, { "ph", 0.9 }, { "acidity", 0.9 }, { "drainage", 0.8 }, { "erosion", 0.8 } } },
            { "irrigation", new() { { "water", 1.0 }, { "irrigation", 1.0 }, { "drip", 0.9 }, { "sprinkler", 0.9 }, { "moisture", 0.7 } } },
            { "disease_management", new() { { "disease", 1.0 }, { "pest", 1.0 }, { "fungus", 0.9 }, { "wilt", 0.9 }, { "yellowing", 0.8 }, { "spots", 0.8 } } },
            { "harvest", new() { { "harvest", 1.0 }, { "yield", 1.0 }, { "picking", 0.9 }, { "berry", 0.8 }, { "maturit", 0.8 } } },
            { "planting", new() { { "planting", 1.0 }, { "plant", 0.8 }, { "spac", 0.9 }, { "hole", 0.9 }, { "nursery", 0.9 } } },
            { "variety", new() { { "variety", 1.0 }, { "variet", 1.0 }, { "cultivar", 0.9 }, { "type", 0.6 }, { "species", 0.8 }, { "kuching", 1.0 }, { "panniyur", 1.0 }, { "iw", 0.9 } } }
        };

        foreach (var category in categoryMap)
        {
            double maxScore = 0.0;
            foreach (var keyword in category.Value)
            {
                if (msgLower.Contains(keyword.Key))
                {
                    // Take the highest score found for this category
                    if (keyword.Value > maxScore) maxScore = keyword.Value;
                }
            }
            if (maxScore > 0)
            {
                scores[category.Key] = maxScore;
            }
        }
        
        return scores;
    }
}
