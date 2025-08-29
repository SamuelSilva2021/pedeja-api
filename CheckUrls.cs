using Microsoft.EntityFrameworkCore;
using PedejaApp.Infrastructure.Data;

class Program
{
    static async Task Main(string[] args)
    {
        var connectionString = "server=localhost;Database=pedeja;port=5432;uid=postgres;pwd=admin;Encoding=UTF8;";
        
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        
        using var context = new ApplicationDbContext(optionsBuilder.Options);
        
        Console.WriteLine("Products with image URLs:");
        Console.WriteLine("========================");
        
        var products = await context.Products
            .Where(p => !string.IsNullOrEmpty(p.ImageUrl))
            .Select(p => new { p.Id, p.Name, p.ImageUrl })
            .Take(10)
            .ToListAsync();
        
        foreach (var product in products)
        {
            Console.WriteLine($"ID: {product.Id}");
            Console.WriteLine($"Name: {product.Name}");
            Console.WriteLine($"ImageUrl: {product.ImageUrl}");
            Console.WriteLine($"Contains blob: {(product.ImageUrl.Contains("blob:") ? "YES" : "NO")}");
            Console.WriteLine($"Contains localhost:4200: {(product.ImageUrl.Contains("localhost:4200") ? "YES" : "NO")}");
            Console.WriteLine("---");
        }
    }
}