using Microsoft.AspNetCore.Http;

namespace PedejaApp.Web.Services
{
    public interface IUrlService
    {
        string BuildImageUrl(string? relativePath);
    }

    public class UrlService : IUrlService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UrlService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string BuildImageUrl(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return string.Empty;
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                return relativePath;
            }

            var scheme = request.Scheme;
            var host = request.Host.Value;
            
            // Remove leading slash if present
            var cleanPath = relativePath.TrimStart('/');
            
            return $"{scheme}://{host}/uploads/{cleanPath}";
        }
    }
}
