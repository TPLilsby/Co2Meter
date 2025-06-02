using Co2Api.Data;
using Co2Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Co2Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Co2Controller : ControllerBase
    {
        private readonly AppDbContext _context;

        public Co2Controller(AppDbContext context)
        {
            _context = context;
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Co2Data data)
        {
            data.Timestamp = DateTime.SpecifyKind(data.Timestamp, DateTimeKind.Utc);
            _context.Co2Readings.Add(data);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "CO2 data gemt i databasen",
                data
            });
        }

    }
}
