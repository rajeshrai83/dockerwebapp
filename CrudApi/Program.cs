using CrudApi.Data;
using CrudApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Setup DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Setup CORS (Fixes Browser Connection Errors)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // Allows localhost:5000, localhost:5002, etc.
              .AllowAnyMethod()   // Allows GET, POST, PUT, DELETE
              .AllowAnyHeader();  // Allows content-type: application/json
    });
});
var app = builder.Build();


// 3. ROBUST MIGRATION & SEEDING (Fixes "Manual Start" issue)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<AppDbContext>();

    // Retry Loop: Waits up to 60 seconds for SQL Server
    for (int i = 0; i < 20; i++)
    {
        try
        {
            if (context.Database.CanConnect())
            {
                // Apply Migrations
                context.Database.Migrate();

                // Seed Data if empty
                if (!context.Products.Any())
                {
                    context.Products.AddRange(
                        new Product { Name = "Seed Laptop", Price = 999.99m },
                        new Product { Name = "Seed Mouse", Price = 25.50m }
                    );
                    context.SaveChanges();
                }
                logger.LogInformation("--> Database Ready and Seeded.");
                break;
            }
            logger.LogWarning($"--> Waiting for SQL Server... ({i + 1}/20)");
            logger.LogInformation(context.Database.GetDbConnection().ConnectionString);
            System.Threading.Thread.Sleep(3000); // Wait 3s
        }
        catch
        {
            logger.LogWarning($"--> Retrying connection... ({i + 1}/20)");
            System.Threading.Thread.Sleep(3000);
        }
    }
}

app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();

// 4. API Endpoints
app.MapGet("/products", async (int? page, int? pageSize, AppDbContext db) =>
{
    int p = page ?? 1; int s = pageSize ?? 10;
    var items = await db.Products.OrderByDescending(x => x.Id).Skip((p - 1) * s).Take(s).ToListAsync();
    return Results.Ok(new { items, totalCount = await db.Products.CountAsync(), pageIndex = p, pageSize = s });
});

app.MapGet("/products/{id}", async (int id, AppDbContext db) =>
    await db.Products.FindAsync(id) is Product p ? Results.Ok(p) : Results.NotFound());

app.MapPost("/products", async (Product p, AppDbContext db) =>
{
    db.Products.Add(p); await db.SaveChangesAsync(); return Results.Created($"/products/{p.Id}", p);
});

app.MapPut("/products/{id}", async (int id, Product p, AppDbContext db) =>
{
    var exist = await db.Products.FindAsync(id);
    if (exist == null) return Results.NotFound();
    exist.Name = p.Name; exist.Price = p.Price;
    await db.SaveChangesAsync(); return Results.NoContent();
});

app.MapDelete("/products/{id}", async (int id, AppDbContext db) =>
{
    var p = await db.Products.FindAsync(id);
    if (p != null) { db.Products.Remove(p); await db.SaveChangesAsync(); }
    return Results.NoContent();
});

app.MapPost("/products/bulk", async (List<Product> products, AppDbContext db) =>
{
    db.Products.AddRange(products); await db.SaveChangesAsync(); return Results.Ok();
});

app.Run();