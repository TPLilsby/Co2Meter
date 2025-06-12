using System;
using System.ComponentModel.DataAnnotations;

namespace Co2Api.Models;

public class Co2Reading
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public double Ppm { get; set; }

    [Required]
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
} 