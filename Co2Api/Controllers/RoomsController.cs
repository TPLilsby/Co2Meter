using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Co2Api.Data;
using Co2Api.Models;

namespace Co2Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly Co2DbContext _context;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(Co2DbContext context, ILogger<RoomsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms([FromQuery] RoomFilterDto filter)
    {
        try
        {
            var query = _context.Rooms.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Floor))
                query = query.Where(r => r.Floor == filter.Floor);

            if (filter.MinOccupancy.HasValue)
                query = query.Where(r => r.MaxOccupancy >= filter.MinOccupancy);

            if (filter.MaxOccupancy.HasValue)
                query = query.Where(r => r.MaxOccupancy <= filter.MaxOccupancy);

            // Apply sorting
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                query = filter.SortBy.ToLower() switch
                {
                    "name" => filter.SortDescending ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
                    "floor" => filter.SortDescending ? query.OrderByDescending(r => r.Floor) : query.OrderBy(r => r.Floor),
                    "maxoccupancy" => filter.SortDescending ? query.OrderByDescending(r => r.MaxOccupancy) : query.OrderBy(r => r.MaxOccupancy),
                    _ => query
                };
            }

            // Apply pagination
            var totalCount = await query.CountAsync();
            var rooms = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(r => new RoomDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Floor = r.Floor,
                    MaxOccupancy = r.MaxOccupancy
                })
                .ToListAsync();

            return Ok(new
            {
                Data = rooms,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rooms");
            return StatusCode(500, "An error occurred while retrieving rooms");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomDto>> GetRoom(int id)
    {
        var room = await _context.Rooms.FindAsync(id);

        if (room == null)
        {
            return NotFound();
        }

        return new RoomDto
        {
            Id = room.Id,
            Name = room.Name,
            Description = room.Description,
            Floor = room.Floor,
            MaxOccupancy = room.MaxOccupancy
        };
    }

    [HttpGet("{id}/stats")]
    public async Task<ActionResult<RoomStatsDto>> GetRoomStats(int id, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            var stats = await _context.Co2Readings
                .Where(r => r.RoomId == id && r.Timestamp >= startDate && r.Timestamp <= endDate)
                .GroupBy(r => r.RoomId)
                .Select(g => new RoomStatsDto
                {
                    RoomId = g.Key,
                    RoomName = room.Name,
                    AveragePpm = g.Average(r => r.Ppm),
                    MinPpm = g.Min(r => r.Ppm),
                    MaxPpm = g.Max(r => r.Ppm),
                    TotalReadings = g.Count(),
                    StartDate = startDate,
                    EndDate = endDate
                })
                .FirstOrDefaultAsync();

            if (stats == null)
            {
                return new RoomStatsDto
                {
                    RoomId = id,
                    RoomName = room.Name,
                    AveragePpm = 0,
                    MinPpm = 0,
                    MaxPpm = 0,
                    TotalReadings = 0,
                    StartDate = startDate,
                    EndDate = endDate
                };
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving room statistics");
            return StatusCode(500, "An error occurred while retrieving room statistics");
        }
    }

    [HttpPost]
    public async Task<ActionResult<RoomDto>> PostRoom(RoomDto roomDto)
    {
        try
        {
            var room = new Room
            {
                Name = roomDto.Name,
                Description = roomDto.Description,
                Floor = roomDto.Floor,
                MaxOccupancy = roomDto.MaxOccupancy
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            roomDto.Id = room.Id;
            return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, roomDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating room");
            return StatusCode(500, "An error occurred while creating the room");
        }
    }

    [HttpPost("batch")]
    public async Task<ActionResult<IEnumerable<RoomDto>>> PostBatchRooms(BatchRoomDto batchDto)
    {
        try
        {
            var rooms = batchDto.Rooms.Select(dto => new Room
            {
                Name = dto.Name,
                Description = dto.Description,
                Floor = dto.Floor,
                MaxOccupancy = dto.MaxOccupancy
            }).ToList();

            _context.Rooms.AddRange(rooms);
            await _context.SaveChangesAsync();

            // Update DTOs with new IDs
            for (int i = 0; i < rooms.Count; i++)
            {
                batchDto.Rooms[i].Id = rooms[i].Id;
            }

            return CreatedAtAction(nameof(GetRooms), batchDto.Rooms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating batch rooms");
            return StatusCode(500, "An error occurred while creating the rooms");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutRoom(int id, RoomDto roomDto)
    {
        if (id != roomDto.Id)
        {
            return BadRequest();
        }

        try
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            room.Name = roomDto.Name;
            room.Description = roomDto.Description;
            room.Floor = roomDto.Floor;
            room.MaxOccupancy = roomDto.MaxOccupancy;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating room");
            return StatusCode(500, "An error occurred while updating the room");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        try
        {
            var room = await _context.Rooms
                .Include(r => r.Co2Readings)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            // Delete associated CO2 readings first
            if (room.Co2Readings.Any())
            {
                _context.Co2Readings.RemoveRange(room.Co2Readings);
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting room");
            return StatusCode(500, "An error occurred while deleting the room");
        }
    }

    [HttpDelete("batch")]
    public async Task<IActionResult> DeleteBatchRooms([FromBody] int[] ids)
    {
        try
        {
            var rooms = await _context.Rooms
                .Include(r => r.Co2Readings)
                .Where(r => ids.Contains(r.Id))
                .ToListAsync();

            if (!rooms.Any())
            {
                return NotFound();
            }

            // Delete all associated CO2 readings
            var allReadings = rooms.SelectMany(r => r.Co2Readings);
            if (allReadings.Any())
            {
                _context.Co2Readings.RemoveRange(allReadings);
            }

            _context.Rooms.RemoveRange(rooms);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting batch rooms");
            return StatusCode(500, "An error occurred while deleting the rooms");
        }
    }
} 