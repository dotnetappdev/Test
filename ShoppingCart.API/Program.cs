using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using ShoppingCart.Data;
using System.Reflection;

namespace ShoppingCart.API
{
    /// <summary>
    /// Entry point of the application
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Load configuration based on the environment
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // Set DbContext according to the connection string specified in appsettings file.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<ShoppingCartDbContext>(
                options => options.UseSqlite(connectionString));

            // Add Swagger services
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "ShoppingCart API",
                    Description = "An API to manage shopping cart operations"
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            // Register ShoppingCartDataSet
            builder.Services.AddTransient<ShoppingCartDataSet>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ShoppingCartDbContext>();

                if (context.Database.IsSqlite())
                {
                    context.Database.EnsureCreated();
                    var dataSet = services.GetRequiredService<ShoppingCartDataSet>();
                    if (!context.Products.Any())
                    {
                        dataSet.LoadData();
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger(u =>
                {
                    u.RouteTemplate = "swagger/{documentName}/swagger.json";
                });
                app.UseSwaggerUI(c =>
                {
                    c.RoutePrefix = "swagger";
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShoppingCart API V1");
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
