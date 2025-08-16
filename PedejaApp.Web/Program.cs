using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PedejaApp.Application.Common.Interfaces;
using PedejaApp.Application.Features.Categories;
using PedejaApp.Application.Services;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Infrastructure.Data;
using PedejaApp.Infrastructure.Repositories;
using PedejaApp.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200", 
                "https://localhost:4200",
                "http://localhost:5072",
                "https://localhost:5072",
                "http://localhost:3000",
                "https://localhost:3000"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
    
    // Política mais permissiva para desenvolvimento
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register application services
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();
builder.Services.AddScoped<PedejaApp.Application.Features.Categories.ICategoryService, PedejaApp.Application.Features.Categories.CategoryService>();
builder.Services.AddScoped<IProductService, SimpleProductService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Configure Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure file upload settings
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 52428800; // 50MB
});

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao criar o banco de dados: {ex.Message}");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowAll"); // Política mais permissiva para desenvolvimento
}
else
{
    app.UseCors("AllowAngularDev"); // Política restritiva para produção
    app.UseHttpsRedirection();
}

// Temporarily disable authentication for development
// app.UseAuthentication();
// app.UseAuthorization();

// Serve static files
app.UseStaticFiles();

// Configure uploads directory specifically
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads")),
    RequestPath = "/uploads"
});

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow });

// Test endpoint
app.MapGet("/api/test", () => new { Message = "Test endpoint working", Timestamp = DateTime.UtcNow });

app.Run();
