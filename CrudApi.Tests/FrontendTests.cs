using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace CrudApi.Tests
{
    // Make sure your Docker containers are RUNNING before running this test!
    [Parallelizable(ParallelScope.Self)]
    public class FrontendTests : PageTest
    {
        [Test]
        public async Task Save_Button_Should_Add_Product()
        {
            // 1. Go to your Frontend URL
            await Page.GotoAsync("http://localhost:5002");

            // 2. Fill in the input fields (Match these IDs to your HTML)
            // Example: <input id="productName">
            await Page.FillAsync("#productName", "Integration Test Bike");
            await Page.FillAsync("#productPrice", "5000");

            // 3. Click the Save Button
            await Page.ClickAsync("#saveButton");

            // 4. Wait for the list to update (verify the new item appears)
            // This checks if the text "Integration Test Bike" appears on screen
            await Expect(Page.GetByText("Integration Test Bike")).ToBeVisibleAsync();
        }
    }
}