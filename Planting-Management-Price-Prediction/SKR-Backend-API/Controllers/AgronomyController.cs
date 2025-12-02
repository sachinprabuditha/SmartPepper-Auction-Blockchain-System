using Microsoft.AspNetCore.Mvc;
using SKR_Backend_API.DTOs;
using SKR_Backend_API.Models;
using SKR_Backend_API.Services;

namespace SKR_Backend_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgronomyController : ControllerBase
{
    private readonly IAgronomyService _agronomyService;
    private readonly IEmergencyService _emergencyService;
    private readonly ILogger<AgronomyController> _logger;

    public AgronomyController(
        IAgronomyService agronomyService,
        IEmergencyService emergencyService,
        ILogger<AgronomyController> logger)
    {
        _agronomyService = agronomyService;
        _emergencyService = emergencyService;
        _logger = logger;
    }

    /// <summary>
    /// Get all districts
    /// </summary>
    [HttpGet("districts")]
    [ProducesResponseType(typeof(ApiResponse<List<District>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<District>>>> GetAllDistricts()
    {
        try
        {
            var districts = await _agronomyService.GetAllDistrictsAsync();
            return Ok(ApiResponse<List<District>>.SuccessResponse(districts, "Districts retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving districts");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving districts"));
        }
    }

    /// <summary>
    /// Get available soil types for a specific district
    /// </summary>
    [HttpGet("districts/{districtId}/soils")]
    [ProducesResponseType(typeof(ApiResponse<List<SoilTypeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<SoilTypeDto>>>> GetSoilsByDistrict(int districtId)
    {
        try
        {
            if (districtId <= 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid district ID"));
            }

            var soils = await _agronomyService.GetSoilsByDistrictAsync(districtId);
            return Ok(ApiResponse<List<SoilTypeDto>>.SuccessResponse(soils, "Soil types retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving soil types for district {DistrictId}", districtId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving soil types"));
        }
    }

    /// <summary>
    /// Get agronomy guide for a specific district, soil type, and variety combination
    /// </summary>
    [HttpGet("guide")]
    [ProducesResponseType(typeof(ApiResponse<AgronomyGuideResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AgronomyGuideResponseDto>>> GetGuide(
        [FromQuery] int districtId,
        [FromQuery] int soilTypeId,
        [FromQuery] string varietyId)
    {
        try
        {
            if (districtId <= 0 || soilTypeId <= 0 || string.IsNullOrWhiteSpace(varietyId))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("District ID, Soil Type ID, and Variety ID are required"));
            }

            var guide = await _agronomyService.GetGuideAsync(districtId, soilTypeId, varietyId);
            if (guide == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"No guide found for District ID: {districtId}, Soil Type ID: {soilTypeId}, Variety ID: {varietyId}"));
            }

            return Ok(ApiResponse<AgronomyGuideResponseDto>.SuccessResponse(guide, "Agronomy guide retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving guide for District: {DistrictId}, Soil: {SoilTypeId}, Variety: {VarietyId}", districtId, soilTypeId, varietyId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving the guide"));
        }
    }

    /// <summary>
    /// Get varieties by their IDs
    /// </summary>
    [HttpPost("varieties")]
    [ProducesResponseType(typeof(ApiResponse<List<BlackPepperVariety>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<BlackPepperVariety>>>> GetVarietiesByIds([FromBody] List<string> ids)
    {
        try
        {
            var varieties = await _agronomyService.GetVarietiesByIdsAsync(ids);
            return Ok(ApiResponse<List<BlackPepperVariety>>.SuccessResponse(varieties, "Varieties retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving varieties");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving varieties"));
        }
    }

    /// <summary>
    /// Get a single variety by ID
    /// </summary>
    [HttpGet("variety/{id}")]
    [ProducesResponseType(typeof(ApiResponse<BlackPepperVariety>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BlackPepperVariety>>> GetVarietyById(string id)
    {
        try
        {
            var variety = await _agronomyService.GetVarietyByIdAsync(id);
            if (variety == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"No variety found with ID: {id}"));
            }

            return Ok(ApiResponse<BlackPepperVariety>.SuccessResponse(variety, "Variety retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving variety {Id}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving the variety"));
        }
    }

    /// <summary>
    /// Get all guides (with variety details and steps) for a specific district and soil type combination
    /// </summary>
    [HttpGet("districts/{districtId}/soils/{soilTypeId}/guides")]
    [ProducesResponseType(typeof(ApiResponse<List<AgronomyGuideResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<AgronomyGuideResponseDto>>>> GetAllGuidesByDistrictAndSoil(int districtId, int soilTypeId)
    {
        // Redirect to new search method
        return await SearchGuides(districtId, soilTypeId);
    }

    /// <summary>
    /// Search agronomy guides by district and optional soil type
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<List<AgronomyGuideResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<AgronomyGuideResponseDto>>>> SearchGuides(
        [FromQuery] int districtId, 
        [FromQuery] int? soilTypeId = null)
    {
        try
        {
            if (districtId <= 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("District ID is required"));
            }

            _logger.LogInformation("Searching guides for District: {DistrictId}, SoilCode: {SoilTypeId}", districtId, soilTypeId);
            var guides = await _agronomyService.SearchGuidesAsync(districtId, soilTypeId);
            _logger.LogInformation("Successfully retrieved {Count} guides", guides.Count);
            return Ok(ApiResponse<List<AgronomyGuideResponseDto>>.SuccessResponse(guides, "Guides retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching guides for District: {DistrictId}, Soil: {SoilTypeId}. Error: {Message}", districtId, soilTypeId, ex.Message);
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"An error occurred while searching guides: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get varieties available for a specific district and soil type combination
    /// </summary>
    [HttpGet("districts/{districtId}/soils/{soilTypeId}/varieties")]
    [ProducesResponseType(typeof(ApiResponse<List<BlackPepperVariety>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<BlackPepperVariety>>>> GetVarietiesByDistrictAndSoil(int districtId, int soilTypeId)
    {
        try
        {
            if (districtId <= 0 || soilTypeId <= 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid district ID or soil type ID"));
            }

            _logger.LogInformation("Fetching varieties for District: {DistrictId}, Soil: {SoilTypeId}", districtId, soilTypeId);
            var varieties = await _agronomyService.GetVarietiesByDistrictAndSoilAsync(districtId, soilTypeId);
            _logger.LogInformation("Successfully retrieved {Count} varieties", varieties.Count);
            return Ok(ApiResponse<List<BlackPepperVariety>>.SuccessResponse(varieties, "Varieties retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving varieties: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
            var errorMessage = "An error occurred while retrieving varieties";
            if (ex.InnerException != null)
            {
                errorMessage += $": {ex.InnerException.Message}";
            }
            return StatusCode(500, ApiResponse<object>.ErrorResponse(errorMessage));
        }
    }

    /// <summary>
    /// Get all varieties
    /// </summary>
    [HttpGet("varieties")]
    [ProducesResponseType(typeof(ApiResponse<List<BlackPepperVariety>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<BlackPepperVariety>>>> GetAllVarieties()
    {
        try
        {
            _logger.LogInformation("Fetching all varieties");
            var varieties = await _agronomyService.GetAllVarietiesAsync();
            _logger.LogInformation("Successfully retrieved {Count} varieties", varieties.Count);
            return Ok(ApiResponse<List<BlackPepperVariety>>.SuccessResponse(varieties, "Varieties retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all varieties: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
            var errorMessage = "An error occurred while retrieving varieties";
            if (ex.InnerException != null)
            {
                errorMessage += $": {ex.InnerException.Message}";
            }
            return StatusCode(500, ApiResponse<object>.ErrorResponse(errorMessage));
        }
    }

    /// <summary>
    /// Get all emergency templates (pests/diseases)
    /// </summary>
    [HttpGet("emergency-templates")]
    [ProducesResponseType(typeof(ApiResponse<List<EmergencyTemplate>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<EmergencyTemplate>>>> GetEmergencyTemplates([FromQuery] string? search = null)
    {
        try
        {
            IEnumerable<EmergencyTemplate> templates;
            if (!string.IsNullOrWhiteSpace(search))
            {
                templates = await _emergencyService.SearchEmergencyTemplatesAsync(search);
            }
            else
            {
                templates = await _emergencyService.GetAllEmergencyTemplatesAsync();
            }
            return Ok(ApiResponse<List<EmergencyTemplate>>.SuccessResponse(templates.ToList(), "Emergency templates retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving emergency templates");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving emergency templates"));
        }
    }

    /// <summary>
    /// Get a single emergency template by ID
    /// </summary>
    [HttpGet("emergency-templates/{id}")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyTemplate>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EmergencyTemplate>>> GetEmergencyTemplateById(string id)
    {
        try
        {
            var template = await _emergencyService.GetEmergencyTemplateByIdAsync(id);
            if (template == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"No emergency template found with ID: {id}"));
            }
            return Ok(ApiResponse<EmergencyTemplate>.SuccessResponse(template, "Emergency template retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving emergency template {Id}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving the emergency template"));
        }
    }
}

