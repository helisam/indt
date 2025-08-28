using System;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ContratacaoService.Application.Services;
using ContratacaoService.Domain.Repositories;
using ContratacaoService.Domain.Services;
using ContratacaoService.Infrastructure.Data;
using ContratacaoService.Infrastructure.Messaging;
using ContratacaoService.Infrastructure.Repositories;

namespace ContratacaoService.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IContratoService, ContratoService>();
            services.AddScoped<IPropostaMessageConsumerService, PropostaMessageConsumerService>();
            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuração do banco de dados
            services.AddDbContext<ContratacaoDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));
            
            // Aplicar migrações automaticamente
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ContratacaoDbContext>();
                dbContext.Database.Migrate();
            }

            // Repositórios
            services.AddScoped<IContratoRepository, ContratoRepository>();

            // Configuração do AWS SQS
            if (configuration.GetValue<bool>("AWS:Enabled"))
            {
                // Configuração do cliente SQS
                var useLocalStack = configuration.GetValue<bool>("LocalStack:UseLocalStack");
                if (useLocalStack)
                {
                    var localStackHost = configuration["LocalStack:LocalStackHost"] ?? "localhost";
                    var localStackPort = configuration.GetValue<int>("LocalStack:LocalStackPort", 4566);
                    var serviceUrl = $"http://{localStackHost}:{localStackPort}";
                    
                    services.AddSingleton<IAmazonSQS>(provider => 
                    {
                        var sqsConfig = new Amazon.SQS.AmazonSQSConfig
                        {
                            ServiceURL = serviceUrl,
                            UseHttp = true
                        };
                        return new Amazon.SQS.AmazonSQSClient("dummy", "dummy", sqsConfig);
                    });
                }
                else
                {
                    services.AddAWSService<IAmazonSQS>();
                }

                // Serviço de consumo de mensagens
                services.AddHostedService(provider =>
                {
                    var sqsClient = provider.GetRequiredService<IAmazonSQS>();
                    var serviceProvider = provider;
                    var logger = provider.GetRequiredService<ILogger<PropostaMessageConsumer>>();
                    var queueUrl = configuration["AWS:SQS:PropostaStatusQueue"] ?? "proposta-status-queue";
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
                    return new PropostaMessageConsumer(sqsClient, serviceProvider, logger, queueUrl);
                });
            }

            return services;
        }
    }
}