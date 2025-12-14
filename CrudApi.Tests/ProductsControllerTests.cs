using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using CrudApi.Controllers;
using CrudApi.Data; // Update with your actual namespace
using CrudApi.Models; // Update with your actual namespace
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrudApi.Tests
{
    public class ProductsControllerTests
    {
        // Helper method to create a temporary "Fake" Database
        private AppDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // Unique name per test
                .Options;

            var databaseContext = new AppDbContext(options);
            databaseContext.Database.EnsureCreated();

            return databaseContext;
        }

        [Fact]
        public async Task GetProducts_Returns_EmptyList_When_No_Products()
        {
            // Arrange
            var dbContext = GetDatabaseContext();
            var controller = new ProductsController(dbContext);

            // Act
            var result = await controller.GetProducts();

            // Assert
            // Depending on how your controller is written, it returns ActionResult<IEnumerable<Product>>
            // If you return Ok(list), use this:
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProducts = Assert.IsType<List<Product>>(actionResult.Value);

            Assert.Empty(returnedProducts);
        }

        [Fact]
        public async Task SaveProduct_Adds_Product_To_Database()
        {
            // Arrange
            var dbContext = GetDatabaseContext();
            var controller = new ProductsController(dbContext);
            var newProduct = new Product { Name = "Test Bike", Price = 999 };

            // Act
            var result = await controller.CreateProduct(newProduct);

            // Assert
            // 1. Check response is "Created" (HTTP 201)
            Assert.IsType<CreatedAtActionResult>(result.Result);

            // 2. Verify it was actually saved to the fake DB
            var productInDb = dbContext.Products.FirstOrDefault(p => p.Name == "Test Bike");
            Assert.NotNull(productInDb);
            Assert.Equal(999, productInDb.Price);
        }
    }
}