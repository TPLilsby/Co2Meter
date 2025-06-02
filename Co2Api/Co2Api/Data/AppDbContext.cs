using Co2Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Co2Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base (options) { }

        public DbSet<Co2Data> Co2Readings { get; set; }
    }
}
