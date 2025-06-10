using System;

namespace Co2Api.Models;

public class Co2Reading
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public double Ppm { get; set; }
} 