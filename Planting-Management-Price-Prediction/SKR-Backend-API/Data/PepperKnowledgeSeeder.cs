using Microsoft.EntityFrameworkCore;
using Pgvector;
using SKR_Backend_API.Data;
using SKR_Backend_API.Models;
using SKR_Backend_API.Services;

namespace SKR_Backend_API.Data;

public class PepperKnowledgeSeeder
{
    private readonly AppDbContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<PepperKnowledgeSeeder> _logger;

    public PepperKnowledgeSeeder(
        AppDbContext context,
        IEmbeddingService embeddingService,
        ILogger<PepperKnowledgeSeeder> logger)
    {
        _context = context;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _context.PepperKnowledge.AnyAsync())
        {
            _logger.LogInformation("PepperKnowledge table already has data. Skipping seed.");
            return;
        }

        _logger.LogInformation("Seeding PepperKnowledge table...");

        var seedData = GetSeedData();

        foreach (var item in seedData)
        {
            try
            {
                // Generate embedding on the fly
                var embeddingFloats = await _embeddingService.GenerateEmbeddingAsync(item.Content);
                item.Embedding = new Vector(embeddingFloats);

                _context.PepperKnowledge.Add(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate embedding for item: {Title}", item.Title);
                // Optionally continue or throw. For seeder, maybe throw to ensure integrity?
                // Depending on requirements. I'll transform it to logic that stops or logs error.
                throw; 
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("PepperKnowledge seeding completed successfully.");
    }

    private List<PepperKnowledge> GetSeedData()
    {
        return new List<PepperKnowledge>
        {
            // 1. Planting – Spacing
            new PepperKnowledge
            {
                Category = "planting",
                SubCategory = "spacing",
                Title = "Pepper vine spacing",
                Content = "Plant black pepper vines at 2.5m x 2.5m spacing using live or dead standards.",
                ConfidenceLevel = "High",
                CreatedAt = DateTime.UtcNow
            },
            // 2. Variety – Kandy
            new PepperKnowledge
            {
                Category = "variety",
                District = "Kandy",
                Variety = "GK 49",
                Title = "Best variety for Kandy district",
                Content = "GK 49 performs well in Kandy district due to moderate rainfall and suitable elevation.",
                Source = "Department of Export Agriculture",
                ConfidenceLevel = "High",
                CreatedAt = DateTime.UtcNow
            },
            // 3. Fertilizer – 6 to 12 Months
            new PepperKnowledge
            {
                Category = "fertilizer",
                SubCategory = "basal",
                PlantAgeMin = 6,
                PlantAgeMax = 12,
                Title = "Fertilizer for 6–12 month old pepper vines",
                Content = "Apply 100g urea, 50g TSP, and 50g MOP per vine. Apply after rain.",
                ConfidenceLevel = "High",
                CreatedAt = DateTime.UtcNow
            },
            // 4. Fertilizer – Badulla
            new PepperKnowledge
            {
                Category = "fertilizer",
                District = "Badulla",
                PlantAgeMin = 6,
                PlantAgeMax = 12,
                Title = "Fertilizer timing in Badulla",
                Content = "Avoid fertilizer application during heavy rains to prevent nutrient loss.",
                ConfidenceLevel = "Medium",
                CreatedAt = DateTime.UtcNow
            },
            // 5. Pruning
            new PepperKnowledge
            {
                Category = "pruning",
                SubCategory = "leaf_cutting",
                PlantAgeMin = 12,
                Title = "Leaf cutting after first year",
                Content = "Light leaf pruning can be done after one year to improve air circulation.",
                ConfidenceLevel = "High",
                CreatedAt = DateTime.UtcNow
            },
            // 6. Disease – Quick Wilt
            new PepperKnowledge
            {
                Category = "pest_disease",
                SubCategory = "quick_wilt",
                Title = "Quick wilt disease management",
                Content = "Ensure proper drainage. Remove infected vines and apply Trichoderma to soil.",
                ConfidenceLevel = "High",
                CreatedAt = DateTime.UtcNow
            },
            // 7. Harvesting Season
            new PepperKnowledge
            {
                Category = "harvesting",
                MonthStart = 11,
                MonthEnd = 2,
                Title = "Pepper harvesting season",
                Content = "Harvest when one or two berries in the spike turn red. Peak season is November to February.",
                ConfidenceLevel = "High",
                CreatedAt = DateTime.UtcNow
            }
        };
    }
}
