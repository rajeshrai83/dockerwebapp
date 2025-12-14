using CrudWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CrudWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        public IndexModel(IHttpClientFactory factory) => _factory = factory;
        public PagedResult<Product> ProductData { get; set; } = new();

        public async Task OnGet()
        {
            try
            {
                var client = _factory.CreateClient("ApiClient");
                // Uses Docker Internal Network URL
                ProductData = await client.GetFromJsonAsync<PagedResult<Product>>("/products") ?? new();
            }
            catch { /* Handle connection error gracefully */ }
        }
    }
}