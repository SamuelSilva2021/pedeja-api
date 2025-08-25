using Microsoft.Extensions.DependencyInjection;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Application.Common.Interfaces;
using System.Reflection;
using System.Linq;

namespace PedejaApp.Infrastructure.Configurations
{
    /// <summary>
    /// Configuração do Scrutor para registro automático de dependências seguindo Clean Architecture
    /// </summary>
    public static class ScrutorConfig
    {
        public static IServiceCollection AddScrutorServices(this IServiceCollection services)
        {
            // Obter assemblies das camadas da aplicação
            var domainAssembly = Assembly.GetAssembly(typeof(IRepository<>));
            var applicationAssembly = Assembly.GetAssembly(typeof(IProductService));
            var infrastructureAssembly = Assembly.GetExecutingAssembly();

            // Registrar Repositories (Infrastructure -> Domain)
            services.Scan(scan => scan
                .FromAssemblies(infrastructureAssembly)
                .AddClasses(classes => classes
                    .InNamespaces("PedejaApp.Infrastructure.Repositories")
                    .AssignableTo(typeof(IRepository<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Registrar repositórios específicos (Infrastructure -> Domain)
            services.Scan(scan => scan
                .FromAssemblies(infrastructureAssembly)
                .AddClasses(classes => classes
                    .InNamespaces("PedejaApp.Infrastructure.Repositories")
                    .Where(type => type.Name.EndsWith("Repository") && !type.IsAbstract))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Registrar Services da camada Application
            services.Scan(scan => scan
                .FromAssemblies(applicationAssembly)
                .AddClasses(classes => classes
                    .InNamespaces(
                        "PedejaApp.Application.Services",
                        "PedejaApp.Application.Features.Categories")
                    .Where(type => type.Name.EndsWith("Service") && !type.IsAbstract))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Registrar Services da camada Infrastructure
            services.Scan(scan => scan
                .FromAssemblies(infrastructureAssembly)
                .AddClasses(classes => classes
                    .InNamespaces("PedejaApp.Infrastructure.Services")
                    .Where(type => type.Name.EndsWith("Service") && !type.IsAbstract))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Registrar Mappers e Validators
            services.Scan(scan => scan
                .FromAssemblies(applicationAssembly)
                .AddClasses(classes => classes
                    .InNamespaces(
                        "PedejaApp.Application.Services",
                        "PedejaApp.Application.Mappers",
                        "PedejaApp.Application.Validators")
                    .Where(type => type.Name.EndsWith("Mapper") || 
                                   type.Name.EndsWith("Validator") ||
                                   type.Name.Contains("Validation")))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Registrar handlers e outros serviços específicos
            services.Scan(scan => scan
                .FromAssemblies(applicationAssembly)
                .AddClasses(classes => classes
                    .InNamespaces(
                        "PedejaApp.Application.Handlers",
                        "PedejaApp.Application.Commands",
                        "PedejaApp.Application.Queries")
                    .Where(type => type.Name.EndsWith("Handler") && !type.IsAbstract))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }

        /// <summary>
        /// Método alternativo para registro mais específico por convenção de nomenclatura
        /// </summary>
        public static IServiceCollection AddScrutorServicesByConvention(this IServiceCollection services)
        {
            var assemblies = new[]
            {
                Assembly.GetAssembly(typeof(IRepository<>)), // Domain
                Assembly.GetAssembly(typeof(IProductService)), // Application
                Assembly.GetExecutingAssembly() // Infrastructure
            }.Where(a => a != null).ToArray();

            // Registro por convenção de nomenclatura
            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes
                    .Where(type => 
                        (type.Name.EndsWith("Repository") ||
                         type.Name.EndsWith("Service") ||
                         type.Name.EndsWith("Mapper") ||
                         type.Name.EndsWith("Validator") ||
                         type.Name.Contains("Validation")) &&
                        !type.IsAbstract &&
                        !type.IsInterface))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }
    }
}
