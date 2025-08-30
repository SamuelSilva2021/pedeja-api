using PedejaApp.Application.Services;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Application.Mappings;
using PedejaApp.Web.Services;

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
        
        return services;
    }
}