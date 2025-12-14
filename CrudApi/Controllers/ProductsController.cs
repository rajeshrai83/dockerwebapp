using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrudApi.Data;  // ⚠️ Update this to match your actual DbContext namespace
using CrudApi.Models; // ⚠️ Update this to match your actual Product model namespace

namespace CrudApi.Controllers
{
    [Route("[controller]")] // This defines the URL as /products
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /products
        // GET: /products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products.ToListAsync();

            // Wrap the list in "Ok()" so the Test can see the 200 Status Code
            return Ok(products);
        }

        // POST: /products
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
        }
    }
}