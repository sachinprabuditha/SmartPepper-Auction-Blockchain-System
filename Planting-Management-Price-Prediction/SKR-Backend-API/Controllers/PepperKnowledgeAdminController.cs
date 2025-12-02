using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pgvector;
using SKR_Backend_API.Data;
using SKR_Backend_API.DTOs;
using SKR_Backend_API.Models;
using SKR_Backend_API.Services;

namespace SKR_Backend_API.Controllers;

[ApiController]
[Route("api/admin/pepperknowledge")]
// [Authorize(Roles = "Admin")] // Uncomment when Auth roles are set up. Strict requirement "Admin-only"
public class PepperKnowledgeAdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly PepperKnowledgeSeeder _seeder;
    private readonly ILogger<PepperKnowledgeAdminController> _logger;

    public PepperKnowledgeAdminController(
        AppDbContext context,
        IEmbeddingService embeddingService,
        PepperKnowledgeSeeder seeder,
        ILogger<PepperKnowledgeAdminController> logger)
    {
        _context = context;
        _embeddingService = embeddingService;
        _seeder = seeder;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateKnowledge([FromBody] CreatePepperKnowledgeRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // PROD: Generate Embedding on Backend
            var embeddingFloats = await _embeddingService.GenerateEmbeddingAsync(request.Content);
            
            var entity = new PepperKnowledge
            {
                Category = request.Category,
                SubCategory = request.SubCategory,
                District = request.District,
                Variety = request.Variety,
                PlantAgeMin = request.PlantAgeMin,
                PlantAgeMax = request.PlantAgeMax,
                MonthStart = request.MonthStart,
                MonthEnd = request.MonthEnd,
                Title = request.Title,
                Content = request.Content,
                Source = request.Source,
                ConfidenceLevel = request.ConfidenceLevel,
                Embedding = new Vector(embeddingFloats),
                CreatedAt = DateTime.UtcNow
            };

            _context.PepperKnowledge.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Knowledge created successfully", Id = entity.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pepper knowledge");
            return StatusCode(500, new { Message = "An error occurred while saving knowledge." });
        }
    }

    [HttpPost("seed")]
    public async Task<IActionResult> TriggerSeed()
    {
        try
        {
            await _seeder.SeedAsync();
            return Ok(new { Message = "Seeding process completed (or skipped if data existed)." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing seeder");
            return StatusCode(500, new { Message = "Seeding failed. Check server logs." });
        }
    }
}
