using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SKR_Backend_API.Models;

[Table("Users")]
public class User
{
    [Key]
    [Column("Id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(255)]
    [Column("Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("PasswordHash", TypeName = "text")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("FullName")]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("PhoneNumber")]
    public string? PhoneNumber { get; set; }

    [Column("CreatedAt", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt", TypeName = "timestamp with time zone")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

