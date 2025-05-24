using Microsoft.Extensions.Configuration;
using ShoppingCart.Data.Entity;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShoppingCart.Data.Utilities
{
    /// <summary>
    /// This class is responsible for seeding data into the database. (In this case, it will create a JSON file with generated synthetic data).
    /// </summary>
    public class ShoppingCartDataSeeder
    {
        private readonly string dataFilePath;
        private readonly JsonSerializerOptions jsonSerializerOptions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        public ShoppingCartDataSeeder(IConfiguration configuration)
        {
            if (configuration != null && !string.IsNullOrEmpty(configuration["DatabaseFile"]))
            {
                dataFilePath = configuration["DatabaseFile"]!;
            }
            else
            {
                throw new ArgumentException("Database file path is not provided in configuration.");
            }

            jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve };
        }

        /// <summary>
        /// Generates synthetic data for Product, Cart and CartItem entities, and saves them in a JSON file.
        /// </summary>
        public void GenerateAndSaveData()
        {
            var productFaker = new Bogus.Faker<Product>()
                .RuleFor(p => p.ID, f => f.IndexFaker + 1)
                .RuleFor(p => p.ProductName, f => f.Commerce.ProductName())
                .RuleFor(p => p.PricePerQuantity, f => decimal.Parse(f.Commerce.Price(2, 500, 2)));

            var products = productFaker.Generate(10);

            var cartFaker = new Bogus.Faker<Cart>()
                .RuleFor(c => c.ID, f => f.IndexFaker + 1)
                .RuleFor(c => c.CustomerName, f => f.Name.FullName());

            var carts = cartFaker.Generate(3);

            var cartItemFaker = new Bogus.Faker<CartItem>()
                .RuleFor(ci => ci.ID, f => f.IndexFaker + 1)
                .RuleFor(ci => ci.CartID, f => f.PickRandom(carts).ID)
                .RuleFor(ci => ci.ProductID, f => f.PickRandom(products).ID)
                .RuleFor(ci => ci.Quantity, f => f.Random.Int(1, 10));

            var cartItems = cartItemFaker.Generate(7);

            SaveData(products, carts, cartItems);
        }

        /// <summary>
        /// Serializes generated synthetic data and save them into a JSON file.
        /// </summary>
        /// <param name="products"></param>
        /// <param name="carts"></param>
        /// <param name="cartItems"></param>
        private void SaveData(List<Product> products, List<Cart> carts, List<CartItem> cartItems)
        {
            var data = new
            {
                Products = products,
                Carts = carts,
                CartItems = cartItems
            };

            var jsonData = JsonSerializer.Serialize(data, jsonSerializerOptions);
            File.WriteAllText(dataFilePath, jsonData);
        }
    }
}
