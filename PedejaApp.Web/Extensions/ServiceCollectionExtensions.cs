using PedejaApp.Application.Services;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Application.Mappings;
using PedejaApp.Web.Services;
using Microsoft.AspNetCore.SignalR;
using PedejaApp.Web.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Infrastructure.Authentication;

namespace PedejaApp.Web.Extensions;

/// <summary>
/// Extensões para configuração de injeção de dependência
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra serviços específicos que não são cobertos pelo Scrutor
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Configurar AutoMapper com profiles da aplicação
        services.AddAutoMapper(typeof(ProductMappingProfile));
        
        // Registrar NotificationService com Hub específico (substitui o registro do Scrutor)
        services.AddScoped<INotificationService, SignalRNotificationServiceWrapper>();
        
        // Registrar serviços de autenticação
        services.AddScoped<IAuthenticationService, ExternalAuthenticationService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        return services;
    }

    /// <summary>
    /// Configura autenticação JWT usando tokens do saas-authentication-api
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecret = configuration["Authentication:JwtSecret"];
        var jwtIssuer = configuration["Authentication:JwtIssuer"];
        var jwtAudience = configuration["Authentication:JwtAudience"];

        if (string.IsNullOrEmpty(jwtSecret))
        {
            throw new InvalidOperationException("JWT Secret não configurado");
        }

        var key = Encoding.UTF8.GetBytes(jwtSecret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Para desenvolvimento
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
                ValidIssuer = jwtIssuer,
                ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("Falha na autenticação JWT: {Error}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    var userId = context.Principal?.FindFirst("user_id")?.Value ?? 
                                context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    logger.LogInformation("Token JWT validado para usuário: {UserId}", userId);
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}