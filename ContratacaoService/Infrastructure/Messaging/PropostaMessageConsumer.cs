using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ContratacaoService.Domain.Services;

namespace ContratacaoService.Infrastructure.Messaging
{
    public class PropostaMessageConsumer : BackgroundService
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PropostaMessageConsumer> _logger;
        private readonly string _queueUrl;

        public PropostaMessageConsumer(
            IAmazonSQS sqsClient,
            IServiceProvider serviceProvider,
            ILogger<PropostaMessageConsumer> logger,
            string queueUrl)
        {
            _sqsClient = sqsClient;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _queueUrl = queueUrl;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Iniciando consumo de mensagens da fila de propostas");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var request = new ReceiveMessageRequest
                    {
                        QueueUrl = _queueUrl,
                        MaxNumberOfMessages = 10,
                        WaitTimeSeconds = 20,
                        MessageAttributeNames = new List<string> { "All" }
                    };

                    var response = await _sqsClient.ReceiveMessageAsync(request, stoppingToken);

                    foreach (var message in response.Messages)
                    {
                        await ProcessarMensagemAsync(message);
                        await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagens da fila");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

        private async Task ProcessarMensagemAsync(Message message)
        {
            try
            {
                var eventType = message.MessageAttributes.TryGetValue("EventType", out var attribute) 
                    ? attribute.StringValue 
                    : null;

                if (eventType == "PropostaStatusAtualizado")
                {
                    var mensagem = JsonSerializer.Deserialize<PropostaStatusMensagem>(message.Body);

                    using var scope = _serviceProvider.CreateScope();
                    var consumerService = scope.ServiceProvider.GetRequiredService<IPropostaMessageConsumerService>();

                    await consumerService.ProcessarMensagemPropostaAsync(
                        mensagem.PropostaId,
                        mensagem.Status,
                        mensagem.Nome,
                        mensagem.CPF,
                        mensagem.ValorSeguro);

                    _logger.LogInformation("Mensagem de proposta processada: {PropostaId}", mensagem.PropostaId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem: {MessageId}", message.MessageId);
            }
        }

        private class PropostaStatusMensagem
        {
            public Guid PropostaId { get; set; }
            public string Status { get; set; }
            public DateTime? DataAtualizacao { get; set; }
            public string Nome { get; set; }
            public string CPF { get; set; }
            public decimal ValorSeguro { get; set; }
        }
    }
}