using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SKR_Backend_API.Models;

[Table("HarvestSessions")]
public class Session
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("seasonid")]
    public Guid SeasonId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("sessionname")]
    public string SessionName { get; set; } = string.Empty;

    [Column("date", TypeName = "timestamp with time zone")]
    public DateTime Date { get; set; }

    [Column("yieldkg", TypeName = "numeric")]
    public decimal YieldKg { get; set; }

    [Column("areaharvested", TypeName = "numeric")]
    public decimal AreaHarvested { get; set; }

    [Column("notes", TypeName = "text")]
    public string? Notes { get; set; }
}

