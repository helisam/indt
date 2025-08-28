using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ContratacaoService.Api.Extensions
{
    public static class OpenApiExtensions
    {
        public static IServiceCollection AddOpenApi(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ContratacaoService API",
                    Version = "v1",
                    Description = "API para gerenciamento de contratações de seguros"
                });
            });

            return services;
        }
    }
}