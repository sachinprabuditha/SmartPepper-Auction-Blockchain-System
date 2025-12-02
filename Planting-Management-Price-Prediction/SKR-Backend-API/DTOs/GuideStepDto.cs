namespace SKR_Backend_API.DTOs;

public class GuideStepDto
{
    public int Id { get; set; }
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}

