using Microsoft.AspNetCore.Mvc;
using Co2Api.Data;
using Co2Api.Models;

namespace Co2Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Co2ReadingsController : ControllerBase
{
    private readonly Co2DbContext _context;

    public Co2ReadingsController(Co2DbContext context)
    {
        _context = context;
    }

    [HttpPut]
    public async Task<IActionResult> PutCo2Reading(Co2Reading reading)
    {
        if (reading.Timestamp == default)
        {
            reading.Timestamp = DateTime.UtcNow;
        }

        _context.Co2Readings.Add(reading);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(PutCo2Reading), new { id = reading.Id }, reading);
    }
} 