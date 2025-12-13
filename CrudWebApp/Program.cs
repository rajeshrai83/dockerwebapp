var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages support
builder.Services.AddRazorPages();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Configure the HTTP Client to talk to the API Container
builder.Services.AddHttpClient("ApiClient", client =>
{
    // "crud_api" is the service name in docker-compose
    // Port 8080 is the INTERNAL port inside the API container
    string apiBase = Environment.GetEnvironmentVariable("ApiBaseUrl") ?? "http://crud_api:8080";
    client.BaseAddress = new Uri(apiBase);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseCors("AllowAll");
app.MapRazorPages();

app.Run();