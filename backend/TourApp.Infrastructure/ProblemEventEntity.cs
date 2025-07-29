using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourApp.Infrastructure;

[Table("ProblemEvents", Schema = "tourapp")]
public class ProblemEventEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid ProblemId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;
    
    public DateTime OccurredAt { get; set; }
    
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string UserRole { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string OldStatus { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string NewStatus { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Comment { get; set; }
    
    [Required]
    [Column(TypeName = "text")]
    public string EventData { get; set; } = string.Empty;
} 