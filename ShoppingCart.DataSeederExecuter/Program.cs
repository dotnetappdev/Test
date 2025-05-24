using Microsoft.Extensions.Configuration;
using ShoppingCart.Data;
using ShoppingCart.Data.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace ShoppingCart.DataSeederExecuter
{

    /// <summary>
    /// A program to invoke DataSeeder utility to generate synthetic data and save it in a JSON file
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point of a program to invoke DataSeeder utility program
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Initiate Data Seeding program");

                var databaseFile = System.Configuration.ConfigurationManager.AppSettings["DatabaseFile"];

                if (String.IsNullOrEmpty(databaseFile))
                {
                    throw new ArgumentException("DatabaseFile cannot be null or empty.");
                }

                // Get the current directory
                var currentDirectory = Directory.GetCurrentDirectory();

                // Get the absolute path to two directories up
                var absolutePath = Path.GetFullPath(Path.Combine(currentDirectory, "..", "..", "..", "..", databaseFile));

                var configValues = new List<KeyValuePair<string, string>>
                    {
                        new("DatabaseFile", absolutePath)
                    };

                var configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues!).Build();

                ShoppingCartDataSeeder shoppingCartDataSeeder = new(configuration);
                shoppingCartDataSeeder.GenerateAndSaveData();

                Console.WriteLine("Completed Data Seeding program");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                Environment.Exit(1); // Exit with a non-zero code to indicate an error
            }
        }
    }
}
