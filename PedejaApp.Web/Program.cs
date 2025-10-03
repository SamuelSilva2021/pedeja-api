using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PedejaApp.Web.Extensions;
using PedejaApp.Infrastructure.Data;
using PedejaApp.Infrastructure.Configurations;
using PedejaApp.Web.Hubs;

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

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Pedeja API",
        Version = "v1",
        Description = "API do sistema Pedeja integrada com autenticação JWT"
    });

    // Configuração para autenticação JWT no Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = @"Autenticação JWT usando o esquema Bearer.
        
            **Como usar:**
            1. Faça login no saas-authentication-api
            2. Copie o `accessToken` da resposta
            3. Cole o token no campo abaixo (apenas o token, sem 'Bearer ')
            4. Clique em 'Authorize' e teste os endpoints protegidos"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.MaximumReceiveMessageSize = 32 * 1024;
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Register application services
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Register additional application services
builder.Services.AddApplicationServices();

// Configure JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

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
    app.UseCors("AllowAll");
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

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

// Map SignalR Hub
app.MapHub<OrderNotificationHub>("/hubs/notifications");

// Health check endpoint
app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow });

// Test endpoint
app.MapGet("/api/test", () => new { Message = "Test endpoint working", Timestamp = DateTime.UtcNow });

// SignalR test endpoint
app.MapGet("/signalr/test", () => new { 
    Message = "SignalR Hub configured", 
    HubPath = "/hubs/notifications",
    Timestamp = DateTime.UtcNow 
});

app.Run();
