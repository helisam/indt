using System;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Services;

namespace PropostaService.Infrastructure.Messaging
{
    public class PropostaMessageService : IPropostaMessageService
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly string _queueUrl;

        public PropostaMessageService(IAmazonSQS sqsClient, string queueUrl)
        {
            _sqsClient = sqsClient;
            _queueUrl = queueUrl;
        }

        public async Task PublicarAtualizacaoStatusAsync(Proposta proposta)
        {
            if (proposta == null)
            {
                throw new ArgumentNullException(nameof(proposta), "Proposta não pode ser nula");
            }

            if (string.IsNullOrEmpty(_queueUrl))
            {
                throw new ArgumentException("URL da fila não pode ser vazia", nameof(_queueUrl));
            }

            var mensagem = new
            {
                PropostaId = proposta.Id,
                Status = proposta.Status.ToString(),
                DataAtualizacao = proposta.DataAtualizacao,
                Nome = proposta.Nome,
                CPF = proposta.CPF,
                ValorSeguro = proposta.ValorSeguro
            };

            var request = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = JsonSerializer.Serialize(mensagem),
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "EventType", new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = "PropostaStatusAtualizado"
                        }
                    }
                }
            };

            await _sqsClient.SendMessageAsync(request);
        }
    }
}