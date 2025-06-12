using Microsoft.AspNetCore.Mvc;
using Co2Api.Data;
using Co2Api.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Co2Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Co2ReadingsController : ControllerBase
{
    private readonly Co2DbContext _context;
    private readonly ILogger<Co2ReadingsController> _logger;

    public Co2ReadingsController(Co2DbContext context, ILogger<Co2ReadingsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Co2Reading>>> GetCo2Readings([FromQuery] Co2ReadingFilterDto filter)
    {
        try
        {
            var query = _context.Co2Readings
                .Include(r => r.Room)
                .AsQueryable();

            // Apply filters
            if (filter.StartDate.HasValue)
                query = query.Where(r => r.Timestamp >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(r => r.Timestamp <= filter.EndDate.Value);

            if (filter.MinPpm.HasValue)
                query = query.Where(r => r.Ppm >= filter.MinPpm.Value);

            if (filter.MaxPpm.HasValue)
                query = query.Where(r => r.Ppm <= filter.MaxPpm.Value);

            if (filter.RoomId.HasValue)
                query = query.Where(r => r.RoomId == filter.RoomId.Value);

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "ppm" => filter.SortDescending ? query.OrderByDescending(r => r.Ppm) : query.OrderBy(r => r.Ppm),
                "timestamp" => filter.SortDescending ? query.OrderByDescending(r => r.Timestamp) : query.OrderBy(r => r.Timestamp),
                _ => query.OrderByDescending(r => r.Timestamp)
            };

            var totalCount = await query.CountAsync();
            var readings = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize),
                Data = readings
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving CO2 readings");
            return StatusCode(500, "An error occurred while retrieving the readings");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Co2Reading>> GetCo2Reading(int id)
    {
        var reading = await _context.Co2Readings
            .Include(r => r.Room)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reading == null)
    {
            return NotFound();
        }

        return reading;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<Co2ReadingStatsDto>> GetStats([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var readings = await _context.Co2Readings
                .Where(r => r.Timestamp >= startDate && r.Timestamp <= endDate)
                .ToListAsync();

            if (!readings.Any())
            {
                return NotFound("No readings found for the specified date range");
            }

            var stats = new Co2ReadingStatsDto
            {
                AveragePpm = readings.Average(r => r.Ppm),
                MinPpm = readings.Min(r => r.Ppm),
                MaxPpm = readings.Max(r => r.Ppm),
                TotalReadings = readings.Count,
                StartDate = startDate,
                EndDate = endDate
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating CO2 reading statistics");
            return StatusCode(500, "An error occurred while calculating statistics");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Co2Reading>> PostCo2Reading([FromBody] Co2ReadingDto readingDto)
    {
        try
        {
            // Verify room exists
            var room = await _context.Rooms.FindAsync(readingDto.RoomId);
            if (room == null)
            {
                return BadRequest($"Room with ID {readingDto.RoomId} does not exist");
            }

            var reading = new Co2Reading
            {
                Ppm = readingDto.Ppm,
                Timestamp = readingDto.Timestamp == default ? DateTime.UtcNow : readingDto.Timestamp,
                RoomId = readingDto.RoomId
            };

        _context.Co2Readings.Add(reading);
        await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCo2Reading), new { id = reading.Id }, reading);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating CO2 reading");
            return StatusCode(500, "An error occurred while creating the reading");
        }
    }

    [HttpPost("batch")]
    public async Task<ActionResult> PostBatchCo2Readings([FromBody] BatchCo2ReadingDto batchDto)
    {
        try
        {
            // Verify all rooms exist
            var roomIds = batchDto.Readings.Select(r => r.RoomId).Distinct();
            var existingRooms = await _context.Rooms
                .Where(r => roomIds.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync();

            var missingRooms = roomIds.Except(existingRooms).ToList();
            if (missingRooms.Any())
            {
                return BadRequest($"Rooms with IDs {string.Join(", ", missingRooms)} do not exist");
            }

            var readings = batchDto.Readings.Select(dto => new Co2Reading
            {
                Ppm = dto.Ppm,
                Timestamp = dto.Timestamp == default ? DateTime.UtcNow : dto.Timestamp,
                RoomId = dto.RoomId
            }).ToList();

            _context.Co2Readings.AddRange(readings);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Successfully added {readings.Count} readings" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating batch CO2 readings");
            return StatusCode(500, "An error occurred while creating the batch readings");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutCo2Reading(int id, [FromBody] Co2ReadingDto readingDto)
    {
        try
        {
            if (id != readingDto.Id)
            {
                return BadRequest("ID mismatch");
            }

            // Verify room exists
            var room = await _context.Rooms.FindAsync(readingDto.RoomId);
            if (room == null)
            {
                return BadRequest($"Room with ID {readingDto.RoomId} does not exist");
            }

            var existingReading = await _context.Co2Readings.FindAsync(id);
            if (existingReading == null)
            {
                return NotFound();
            }

            existingReading.Ppm = readingDto.Ppm;
            existingReading.Timestamp = readingDto.Timestamp == default ? DateTime.UtcNow : readingDto.Timestamp;
            existingReading.RoomId = readingDto.RoomId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Co2ReadingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating CO2 reading");
            return StatusCode(500, "An error occurred while updating the reading");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCo2Reading(int id)
    {
        try
        {
            var reading = await _context.Co2Readings.FindAsync(id);
            if (reading == null)
            {
                return NotFound();
            }

            _context.Co2Readings.Remove(reading);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting CO2 reading");
            return StatusCode(500, "An error occurred while deleting the reading");
        }
    }

    [HttpDelete("batch")]
    public async Task<IActionResult> DeleteBatchCo2Readings([FromBody] int[] ids)
    {
        try
        {
            var readings = await _context.Co2Readings
                .Where(r => ids.Contains(r.Id))
                .ToListAsync();

            if (!readings.Any())
            {
                return NotFound("No readings found with the provided IDs");
            }

            _context.Co2Readings.RemoveRange(readings);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Successfully deleted {readings.Count} readings" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting batch CO2 readings");
            return StatusCode(500, "An error occurred while deleting the batch readings");
        }
    }

    private bool Co2ReadingExists(int id)
    {
        return _context.Co2Readings.Any(e => e.Id == id);
    }
} 