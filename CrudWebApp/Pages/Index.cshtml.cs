using CrudWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace CrudWebApp.Pages
{
    public class IndexModel : PageModel
    {

        public async Task<IActionResult> OnAllApiProxyAsync(string path)
        {
            // This handler acts as a bridge. JS calls this -> This calls Docker API -> Returns result to JS
            var client = _clientFactory.CreateClient("ApiClient");

            // Read the body from the JS request
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response;

            // Determine HTTP Method
            if (Request.Method == "POST")
            {
                // Check if it is actually a PUT or DELETE sent via helper headers or just infer from path
                // For simplicity in this specific setup, we will route based on what JS sends
                response = await client.PostAsync(path, content);
            }
            else if (Request.Method == "PUT")
            {
                response = await client.PutAsync(path, content);
            }
            else if (Request.Method == "DELETE")
            {
                response = await client.DeleteAsync(path);
            }
            else
            {
                return BadRequest();
            }

            return StatusCode((int)response.StatusCode);
        }
        private readonly IHttpClientFactory _clientFactory;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public PagedResult<Product> ProductData { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;


        public async Task OnGetAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient("ApiClient");
                // This will now throw if API returns 500
                ProductData = await client.GetFromJsonAsync<PagedResult<Product>>($"/products?page={CurrentPage}&pageSize=5")
                              ?? new PagedResult<Product>();
            }
            catch (Exception ex)
            {
                // Log error locally or set an error message property to display on UI
                ProductData = new PagedResult<Product>(); // Empty list
                                                          // You can add a property like public string ErrorMessage {get;set;} to show in the UI
                Console.WriteLine($"Error fetching data: {ex.Message}");
            }
        }
    }
}