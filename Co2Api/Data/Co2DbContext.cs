using Microsoft.EntityFrameworkCore;
using Co2Api.Models;

namespace Co2Api.Data;

public class Co2DbContext : DbContext
{
    public Co2DbContext(DbContextOptions<Co2DbContext> options)
        : base(options)
    {
    }

    public DbSet<Co2Reading> Co2Readings { get; set; }
} 