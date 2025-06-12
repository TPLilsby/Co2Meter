using System.ComponentModel.DataAnnotations;

namespace Co2Api.Models;

public class Co2ReadingDto
{
    public int Id { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "PPM value must be positive")]
    public double Ppm { get; set; }

    public DateTime Timestamp { get; set; }

    [Required]
    public int RoomId { get; set; }
}

public class BatchCo2ReadingDto
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one reading is required")]
    public List<Co2ReadingDto> Readings { get; set; } = new();
}

public class Co2ReadingFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public double? MinPpm { get; set; }
    public double? MaxPpm { get; set; }
    public int? RoomId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

public class Co2ReadingStatsDto
{
    public double AveragePpm { get; set; }
    public double MinPpm { get; set; }
    public double MaxPpm { get; set; }
    public int TotalReadings { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int RoomId { get; set; }
} 