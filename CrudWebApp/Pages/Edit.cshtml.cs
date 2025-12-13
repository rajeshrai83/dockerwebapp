using CrudWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CrudWebApp.Pages
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public EditModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var product = await client.GetFromJsonAsync<Product>($"/products/{id}");

            if (product == null) return NotFound();

            Product = product;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var client = _clientFactory.CreateClient("ApiClient");
            await client.PutAsJsonAsync($"/products/{Product.Id}", Product);

            return RedirectToPage("./Index");
        }
    }
}