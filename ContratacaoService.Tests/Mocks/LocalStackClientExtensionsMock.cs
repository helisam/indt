using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LocalStack.Client.Extensions
{
    public static class SQSExtensions
    {
        public static Task<SendMessageResponse> SendMessageAsync(
            this IAmazonSQS client,
            string queueUrl,
            string messageBody,
            CancellationToken cancellationToken = default)
        {
            var request = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody
            };

            return client.SendMessageAsync(request, cancellationToken);
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLocalStack(this IServiceCollection services, IConfiguration configuration)
        {
            // Mock implementation that does nothing
            return services;
        }
    }
}