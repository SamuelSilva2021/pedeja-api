using PedejaApp.Application.Services;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Application.Mappings;
using PedejaApp.Web.Services;
using Microsoft.AspNetCore.SignalR;
using PedejaApp.Web.Hubs;

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
        
        return services;
    }
}