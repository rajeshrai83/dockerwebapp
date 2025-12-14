var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("ApiClient", client =>
{
    // Docker Internal URL
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ApiBaseUrl") ?? "http://crud_api:8080");
});
var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();