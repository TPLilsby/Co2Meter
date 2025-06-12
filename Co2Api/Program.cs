using Microsoft.EntityFrameworkCore;
using Co2Api.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Configure PostgreSQL
var connectionString = "Host=db;Database=co2meter;Username=postgres;Password=Kode1234!";
builder.Services.AddDbContext<Co2DbContext>(options =>
    options.UseNpgsql(connectionString));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// Ensure database is created with retry mechanism
const int maxRetries = 10;
const int retryDelaySeconds = 5;

for (int i = 0; i < maxRetries; i++)
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<Co2DbContext>();
            context.Database.EnsureCreated();
            break;
        }
    }
    catch (Exception ex)
    {
        if (i == maxRetries - 1)
            throw;
            
        Console.WriteLine($"Database connection attempt {i + 1} failed: {ex.Message}. Retrying in {retryDelaySeconds} seconds...");
        Thread.Sleep(retryDelaySeconds * 1000);
    }
}

app.Run();
