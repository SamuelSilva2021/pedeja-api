using Microsoft.EntityFrameworkCore;
using PedejaApp.Infrastructure.Data;

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
    Console.WriteLine($"Contains path: {(product.ImageUrl.Contains('/') ? "Yes" : "No")}");
    Console.WriteLine("---");
}
