using System;
using Amazon.SQS;
using LocalStack.Client.Extensions;
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

            // Repositórios
            services.AddScoped<IContratoRepository, ContratoRepository>();

            // Configuração do LocalStack para AWS SQS
            services.AddLocalStack(configuration);
            services.AddAWSService<IAmazonSQS>();

            // Serviço de consumo de mensagens
            services.AddHostedService(provider =>
            {
                var sqsClient = provider.GetRequiredService<IAmazonSQS>();
                var serviceProvider = provider;
                var logger = provider.GetRequiredService<ILogger<PropostaMessageConsumer>>();
                var queueUrl = configuration["AWS:SQS:PropostaStatusQueue"];
                return new PropostaMessageConsumer(sqsClient, serviceProvider, logger, queueUrl);
            });

            return services;
        }
    }
}