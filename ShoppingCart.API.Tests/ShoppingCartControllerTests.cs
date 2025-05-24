using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using ShoppingCart.API.Controllers;
using ShoppingCart.Data;
using ShoppingCart.Data.Entity;
using System.Net;

namespace ShoppingCart.API.Tests
{
    /// <summary>
    /// Tests for ShoppingCartController APIs
    /// </summary>
    public class ShoppingCartControllerTests
    {
      
        /// <summary>
        /// Creates a new instance of ShoppingCartDbContext created with unique InMemoryDatabase for each Test in execution.
        /// </summary>
        /// <returns></returns>
        private static ShoppingCartDbContext CreateContext()
        {
            var connection = new SqliteConnection("Datasource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ShoppingCartDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ShoppingCartDbContext(options);
            context.Database.EnsureCreated(); // Ensure the database schema is created
            SeedDatabase(context);
            return context;
        }

        /// <summary>
        /// Seeds the database with some initial data
        /// </summary>
        /// <param name="context"></param>
        private static void SeedDatabase(ShoppingCartDbContext context)
        {
            context.Products.Add(new Product { ID = 1, ProductName = "Bluetooth Headset", PricePerQuantity = 10 });
            context.Products.Add(new Product { ID = 2, ProductName = "USB Flash Drive 64 GB", PricePerQuantity = 15 });
            context.SaveChanges();
        }

        /// <summary>
        /// Test to validate that AddProductToCart API returns BadRequest when invalid parameters are passed
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData("", 0, -1)]
        [InlineData("Test User", 0, 1)]
        [InlineData("Test User", 1, 0)]
        public async Task AddProductToCart_ReturnsBadRequest_WhenInvalidParameters(string customerName, int productId, int quantity)
        {
            
            // Arrange
            var context = CreateContext();
            var controller = new ShoppingCartController(context);

            // Act
            var result = await controller.AddProductToCart(customerName, productId, quantity);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        }

        /// <summary>
        /// Test to validate that AddProductToCart API updates an existing Cart Item if the item already exist in customer's cart.<br/>
        /// Ensures the Cart and Cart Item has expected values.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddProductToCart_ReturnsInternalServerError_WhenProductDoesNotExist()
        {
            // Arrange
            var context = CreateContext();
            var controller = new ShoppingCartController(context);

            string customerName = "Thomas";
            int productId = 9999; //This product Id does not exist in the dataset
            int quantity = 2;

            // Act
            var result = await controller.AddProductToCart(customerName, productId, quantity);

            // Assert
            var badRequestResult = ((ObjectResult)result).StatusCode;
            Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult);
        }

        /// <summary>
        /// Test to validate that AddProductToCart API creates a new Cart when Cart does not exist.<br/>
        /// Ensures newly added Cart and Cart Item has expected values.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddProductToCart_CreatesNewCart_WhenCartDoesNotExist()
        {
            // Arrange
            var context = CreateContext();
            var controller = new ShoppingCartController(context);
            string customerName = "John";
            int productId = 1;
            int quantity = 2;

            // Act
            var result = await controller.AddProductToCart(customerName, productId, quantity);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var cart = Assert.IsType<Cart>(okResult.Value);
            Assert.Equal(customerName, cart.CustomerName);
            Assert.Single(cart.CartItems);
            Assert.Equal(productId, cart.CartItems.First().ProductID);
            Assert.Equal(quantity, cart.CartItems.First().Quantity);
            Assert.Equal(20, cart.CartItems.First().Amount);
        }

        /// <summary>
        /// Test to validate that AddProductToCart API updates an existing Cart Item if the item already exist in customer's cart.<br/>
        /// Ensures the Cart and Cart Item has expected values.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddProductToCart_UpdatesExistingCartItem_WhenCartItemExists()
        {
            // Arrange
            var context = CreateContext();
            var controller = new ShoppingCartController(context);

            string customerName = "Thomas";
            int productId = 1;
            int quantity = 2;

            Product? product = await context.Products.FirstOrDefaultAsync(m => m.ID.Equals(productId));
            var existingCart = new Cart
            {
                ID = 1,
                CustomerName = customerName,
                CartItems =
                [
                    new CartItem { ID = 1, CartID = 1, ProductID = productId, Quantity = 1, Product = product! }
                ]
            };

            context.Carts.Add(existingCart);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.AddProductToCart(customerName, productId, quantity);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var cart = Assert.IsType<Cart>(okResult.Value);
            Assert.Equal(customerName, cart.CustomerName);
            Assert.Single(cart.CartItems);
            Assert.Equal(productId, cart.CartItems.First().ProductID);
            Assert.Equal(3, cart.CartItems.First().Quantity);
            Assert.Equal(30, cart.CartItems.First().Amount);
        }

        /// <summary>
        /// Test to validate that AddProductToCart API creates a new Cart Item if the item doesn't exist in customer's cart.<br/>
        /// Ensures newly added Cart Item has expected values.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddProductToCart_AddsNewCartItem_WhenCartItemDoesNotExist()
        {
            // Arrange
            var context = CreateContext();
            var controller = new ShoppingCartController(context);
            string customerName = "Franck Doe";
            int productId = 1;
            int quantity = 2;

            var existingCart = new Cart
            {
                CustomerName = customerName,
                CartItems = []
            };

            context.Carts.Add(existingCart);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.AddProductToCart(customerName, productId, quantity);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var cart = Assert.IsType<Cart>(okResult.Value);
            Assert.Equal(customerName, cart.CustomerName);
            Assert.Single(cart.CartItems);
            Assert.Equal(productId, cart.CartItems.First().ProductID);
            Assert.Equal(quantity, cart.CartItems.First().Quantity);
            Assert.Equal(20, cart.CartItems.First().Amount);
        }

        /// <summary>
        /// Test to validate that AddProductToCart API adds a new Cart Item and updates the total amount of the Cart
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddProductToCart_AddsNewCartItem_ValidateTotalCartAmount()
        {
            // Arrange
            var context = CreateContext();
            var controller = new ShoppingCartController(context);

            string customerName = "Patrick";
            int productId = 1;

            Product? product = await context.Products.FirstOrDefaultAsync(m => m.ID.Equals(productId));
            var existingCart = new Cart
            {
                ID = 1,
                CustomerName = customerName,
                CartItems =
                [
                    new CartItem { ID = 1, CartID = 1, ProductID = productId, Quantity = 1, Product = product! }
                ]
            };

            context.Carts.Add(existingCart);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.AddProductToCart(customerName, 2, 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var cart = Assert.IsType<Cart>(okResult.Value);
            Assert.Equal(25, cart.TotalAmount);
        }
    }
}
