using CrudApi.Data;
using CrudApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Logging to see errors in Docker Logs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// 2. Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. MIGRATION BLOCK (Crucial for the 500 Error)
// This will run AUTOMATICALLY because Docker now waits for SQL to be healthy.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // Ensure Database is created
        context.Database.Migrate();
        Console.WriteLine("--> Database Migration Applied Successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> Could not apply migrations: {ex.Message}");
        // If this fails, the app usually shouldn't start, but we log it.
    }
}

app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();

// --- API ENDPOINTS ---

app.MapGet("/products", async (int? page, int? pageSize, AppDbContext db) =>
{
    try
    {
        int p = page ?? 1;
        int s = pageSize ?? 5;

        // Check if DB can connect before querying
        if (!db.Database.CanConnect())
            return Results.Problem("Database not accessible");

        var query = db.Products.AsQueryable();
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.Id).Skip((p - 1) * s).Take(s).ToListAsync();

        return Results.Ok(new { items, totalCount = total, pageIndex = p, pageSize = s, totalPages = (int)Math.Ceiling((double)total / s) });
    }
    catch (Exception ex)
    {
        // THIS is what generates the 500 error. We log it now.
        Console.WriteLine($"Error in GET /products: {ex.Message}");
        return Results.Problem(ex.Message);
    }
});

// ... (Include your other endpoints here: GET ID, POST, PUT, DELETE, BULK)

app.Run();