using Microsoft.Extensions.Diagnostics.HealthChecks;
using PedejaApp.Domain.Interfaces;

namespace PedejaApp.Infrastructure.HealthChecks;

public class AuthenticationHealthCheck : IHealthCheck
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationHealthCheck(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Teste básico do serviço de autenticação
            // Por enquanto, apenas verifica se o serviço está disponível
            var isHealthy = _authenticationService != null;

            if (isHealthy)
            {
                return HealthCheckResult.Healthy("Authentication service is available");
            }
            
            return HealthCheckResult.Unhealthy("Authentication service is not available");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Authentication service health check failed", 
                ex);
        }
    }
}
