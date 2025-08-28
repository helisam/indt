using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;

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
}