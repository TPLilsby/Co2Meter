using System.ComponentModel.DataAnnotations;

namespace Co2Api.Models;

public class Room
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? Floor { get; set; }

    public int? MaxOccupancy { get; set; }

    // Navigation property for the one-to-many relationship with Co2Readings
    public ICollection<Co2Reading> Co2Readings { get; set; } = new List<Co2Reading>();
} 