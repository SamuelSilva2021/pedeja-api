using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PedejaApp.Web.Extensions;
using PedejaApp.Infrastructure.Data;
using PedejaApp.Infrastructure.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScrutorServices();

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
                "https://localhost:3000",
                "http://localhost:8080",
                "https://localhost:8080"
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
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Register additional application services
builder.Services.AddApplicationServices();

// Configure Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        }));

// Configure file upload settings
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 52428800; // 50MB
});

var app = builder.Build();

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.Migrate(); // Apply any pending migrations
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao aplicar migrações: {ex.Message}");
        // Fallback to EnsureCreated if migrations fail
        try
        {
            context.Database.EnsureCreated();
        }
        catch (Exception ex2)
        {
            Console.WriteLine($"Erro ao criar o banco de dados: {ex2.Message}");
        }
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

// Configure uploads directory specifically with cache headers
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads")),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        // Cache images for 1 year in production, 1 hour in development
        var cacheDuration = app.Environment.IsDevelopment() ? 3600 : 31536000;
        ctx.Context.Response.Headers.Append("Cache-Control", $"public,max-age={cacheDuration}");
        ctx.Context.Response.Headers.Append("Expires", DateTime.UtcNow.AddSeconds(cacheDuration).ToString("R"));
        
        // Security headers for images
        ctx.Context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        ctx.Context.Response.Headers.Append("Content-Security-Policy", "default-src 'none'; img-src 'self'");
    }
});

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow });

// Test endpoint
app.MapGet("/api/test", () => new { Message = "Test endpoint working", Timestamp = DateTime.UtcNow });

app.Run();
