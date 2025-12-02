using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SKR_Backend_API.Models;

[Table("AgronomyTemplates")]
public class AgronomyTemplate
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(255)]
    [Column("taskname")]
    public string TaskName { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("phase")]
    public string? Phase { get; set; } // Landscaping, Planting, Maintenance, Harvesting

    [MaxLength(50)]
    [Column("tasktype")]
    public string TaskType { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("varietykey")]
    public string VarietyKey { get; set; } = string.Empty; // "ALL" or specific variety id

    [Column("timingdaysafterstarting")]
    public int TimingDaysAfterStartingOfFarm { get; set; }

    [Column("instructionaldetails", TypeName = "text")]
    public string? InstructionalDetails { get; set; } // Legacy field - single string instruction

    [NotMapped]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Helper method to get steps as a list (from instructional_details)
    // Splits by newlines first, then by sentences (periods) to create multiple steps
    public List<string> GetDetailedStepsList()
    {
        if (string.IsNullOrEmpty(InstructionalDetails))
        {
            return new List<string>();
        }

        var steps = new List<string>();
        
        // First, try splitting by newlines (if the data is formatted with line breaks)
        var lines = InstructionalDetails.Split(
            new[] { "\r\n", "\r", "\n" }, 
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

        // If we have multiple lines, use each line as a step
        if (lines.Length > 1)
        {
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    steps.Add(trimmed);
                }
            }
        }
        else
        {
            // Single line - try splitting by sentences (period followed by space)
            var text = InstructionalDetails.Trim();
            var sentences = text.Split(
                new[] { ". " }, 
                StringSplitOptions.RemoveEmptyEntries
            );

            if (sentences.Length > 1)
            {
                // Multiple sentences - each becomes a step
                foreach (var sentence in sentences)
                {
                    var trimmed = sentence.Trim();
                    // Add period back if it was removed by split
                    if (!trimmed.EndsWith(".") && !trimmed.EndsWith("!") && !trimmed.EndsWith("?"))
                    {
                        trimmed += ".";
                    }
                    
                    if (!string.IsNullOrWhiteSpace(trimmed))
                    {
                        steps.Add(trimmed);
                    }
                }
            }
            else
            {
                // Single sentence or no periods - return as one step
                steps.Add(text);
            }
        }

        return steps;
    }
}

public class FertilizerSpec
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("dosage_g")]
    public int DosageGramsPerVine { get; set; }
}

