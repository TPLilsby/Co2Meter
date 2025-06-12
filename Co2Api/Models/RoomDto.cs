using System.ComponentModel.DataAnnotations;

namespace Co2Api.Models;

public class RoomDto
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
}

public class BatchRoomDto
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one room is required")]
    public List<RoomDto> Rooms { get; set; } = new();
}

public class RoomFilterDto
{
    public string? Floor { get; set; }
    public int? MinOccupancy { get; set; }
    public int? MaxOccupancy { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}

public class RoomStatsDto
{
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public double AveragePpm { get; set; }
    public double MinPpm { get; set; }
    public double MaxPpm { get; set; }
    public int TotalReadings { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
} 