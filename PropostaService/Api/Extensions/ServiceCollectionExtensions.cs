using System;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropostaService.Application.Services;
using PropostaService.Domain.Repositories;
using PropostaService.Domain.Services;
using PropostaService.Infrastructure.Data;
using PropostaService.Infrastructure.Messaging;
using PropostaService.Infrastructure.Repositories;

namespace PropostaService.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IPropostaService, Application.Services.PropostaService>();
            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuração do banco de dados
            services.AddDbContext<PropostaDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));
            
            // Aplicar migrações automaticamente
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PropostaDbContext>();
                dbContext.Database.Migrate();
            }

            // Repositórios
            services.AddScoped<IPropostaRepository, PropostaRepository>();

            // Serviço de mensageria
            services.AddScoped<IAmazonSQS>(provider => 
            {
                var useLocalStack = configuration.GetValue<bool>("LocalStack:UseLocalStack");
                if (useLocalStack)
                {
                    var localStackHost = configuration["LocalStack:LocalStackHost"] ?? "localhost";
                    var localStackPort = configuration.GetValue<int>("LocalStack:LocalStackPort", 4566);
                    var config = new AmazonSQSConfig
                    {
                        ServiceURL = $"http://{localStackHost}:{localStackPort}"
                    };
                    return new AmazonSQSClient("dummy", "dummy", config);
                }
                return new AmazonSQSClient();
            });
            services.AddScoped<IPropostaMessageService>(provider =>
            {
                var sqsClient = provider.GetRequiredService<IAmazonSQS>();
                var queueUrl = configuration["AWS:SQS:PropostaStatusQueue"] ?? "http://localhost:4566/000000000000/proposta-status-queue";
                if (!queueUrl.StartsWith("http"))
                {
                    var useLocalStack = configuration.GetValue<bool>("LocalStack:UseLocalStack");
                    if (useLocalStack)
                    {
                        var localStackHost = configuration["LocalStack:LocalStackHost"] ?? "localhost";
                        var localStackPort = configuration.GetValue<int>("LocalStack:LocalStackPort", 4566);
                        queueUrl = $"http://{localStackHost}:{localStackPort}/000000000000/{queueUrl}";
                    }
                }
                return new PropostaMessageService(sqsClient, queueUrl);
            });

            return services;
        }
    }
}