using Microsoft.Extensions.Configuration;
using ShoppingCart.Data.Entity;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShoppingCart.Data
{
    /// <summary>
    /// This class is responsible for loading data from a JSON file into the database.
    /// This class is only required because there is no physical database and Shopping Cart data are stored in a JSON file (generated through ShoppingCartDataSeeder utility).
    /// </summary>
    public class ShoppingCartDataSet
    {
        private readonly string? DataFilePath;
        private readonly ShoppingCartDbContext context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="shoppingCartDbContext"></param>
        public ShoppingCartDataSet(IConfiguration configuration, ShoppingCartDbContext shoppingCartDbContext)
        {
            if (configuration != null && !string.IsNullOrEmpty(configuration["DatabaseFile"]))
            {
                DataFilePath = Path.Combine(AppContext.BaseDirectory, configuration["DatabaseFile"]!);
            }
            context = shoppingCartDbContext;
        }
        /// <summary>
        /// Reads data from JSON file and stores them in respective entities of ShoppingCartDbContext
        /// </summary>
        public void LoadData()
        {
            if (DataFilePath == null)
            {
                throw new InvalidOperationException("Data file path is not set.");
            }

            var jsonData = File.ReadAllText(DataFilePath);
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };
            if (IsValidJson(jsonData))
            {
                var data = JsonSerializer.Deserialize<DataContainer>(jsonData, options);
                if (data != null)
                {
                    context.Products.AddRange(data.Products);
                    context.Carts.AddRange(data.Carts);
                    context.CartItems.AddRange(data.CartItems);
                    context.SaveChanges();
                }
                else
                {
                    throw new DataException("Error processing data file.");
                }
            }
        }

        /// <summary>
        /// Validates whether the JSON data is in the correct format and structure
        /// </summary>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        private static bool IsValidJson(string jsonData)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(jsonData))
                {
                    if (doc.RootElement.TryGetProperty("Products", out _) &&
                        doc.RootElement.TryGetProperty("Carts", out _) &&
                        doc.RootElement.TryGetProperty("CartItems", out _))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
                // Log or handle the exception as needed
            }

            return false;
        }

        /// <summary>
        /// Data container class to hold the deserialized data from JSON file in the form of Product, Cart and CartItem collections.
        /// </summary>
        private sealed class DataContainer
        {
            public List<Product> Products { get; set; } = [];
            public List<Cart> Carts { get; set; } = [];
            public List<CartItem> CartItems { get; set; } = [];
        }
    }
}
