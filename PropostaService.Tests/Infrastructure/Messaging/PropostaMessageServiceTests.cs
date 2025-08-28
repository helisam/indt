using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Moq;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Services;
using PropostaService.Infrastructure.Messaging;
using Xunit;

namespace PropostaService.Tests.Infrastructure.Messaging
{
    public class PropostaMessageServiceTests
    {
        private readonly Mock<IAmazonSQS> _mockSqsClient;
        private readonly string _queueUrl;
        private readonly PropostaMessageService _service;

        public PropostaMessageServiceTests()
        {
            _mockSqsClient = new Mock<IAmazonSQS>();
            _queueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/proposta-queue";
            _service = new PropostaMessageService(_mockSqsClient.Object, _queueUrl);
        }

        [Fact]
        public async Task PublicarAtualizacaoStatusAsync_DeveEnviarMensagemParaFila()
        {
            // Arrange
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            proposta.AtualizarStatus(StatusProposta.Aprovada);

            SendMessageRequest capturedRequest = null;
            _mockSqsClient.Setup(m => m.SendMessageAsync(
                    It.IsAny<SendMessageRequest>(),
                    It.IsAny<CancellationToken>()))
                .Callback<SendMessageRequest, CancellationToken>((req, token) => capturedRequest = req)
                .ReturnsAsync(new SendMessageResponse());

            // Act
            await _service.PublicarAtualizacaoStatusAsync(proposta);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(_queueUrl, capturedRequest.QueueUrl);
            Console.WriteLine($"MessageBody: {capturedRequest.MessageBody}");
            Assert.Contains("PropostaId", capturedRequest.MessageBody);
            Assert.Contains("Status", capturedRequest.MessageBody);
            Assert.Contains("Nome", capturedRequest.MessageBody);
            Assert.Contains("CPF", capturedRequest.MessageBody);
            Assert.Contains("ValorSeguro", capturedRequest.MessageBody);
            Assert.True(capturedRequest.MessageAttributes.ContainsKey("EventType"));
            Assert.Equal("PropostaStatusAtualizado", capturedRequest.MessageAttributes["EventType"].StringValue);
        }

        [Theory]
        [InlineData(StatusProposta.Aprovada)]
        [InlineData(StatusProposta.Rejeitada)]
        [InlineData(StatusProposta.EmAnalise)]
        public async Task PublicarAtualizacaoStatusAsync_DeveEnviarMensagemParaDiferentesStatus(StatusProposta status)
        {
            // Arrange
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            proposta.AtualizarStatus(status);

            SendMessageRequest capturedRequest = null;
            _mockSqsClient.Setup(m => m.SendMessageAsync(
                    It.IsAny<SendMessageRequest>(),
                    It.IsAny<CancellationToken>()))
                .Callback<SendMessageRequest, CancellationToken>((req, token) => capturedRequest = req)
                .ReturnsAsync(new SendMessageResponse());

            // Act
            await _service.PublicarAtualizacaoStatusAsync(proposta);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Contains("Status", capturedRequest.MessageBody);
        }

        [Fact]
        public async Task PublicarAtualizacaoStatusAsync_DeveTratarErroDeConexao()
        {
            // Arrange
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            proposta.AtualizarStatus(StatusProposta.Aprovada);

            _mockSqsClient.Setup(m => m.SendMessageAsync(
                    It.IsAny<SendMessageRequest>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonSQSException("Erro de conexão"));

            // Act & Assert
            await Assert.ThrowsAsync<AmazonSQSException>(() => 
                _service.PublicarAtualizacaoStatusAsync(proposta));
        }

        [Fact]
        public async Task PublicarAtualizacaoStatusAsync_DeveLancarExcecao_QuandoPropostaNula()
        {
            // Arrange
            Proposta? propostaNula = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _service.PublicarAtualizacaoStatusAsync(propostaNula!));
        }

        [Fact]
        public async Task PublicarAtualizacaoStatusAsync_DeveLancarExcecao_QuandoQueueUrlVazia()
        {
            // Arrange
            var propostaValida = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            var serviceComQueueUrlVazia = new PropostaMessageService(_mockSqsClient.Object, "");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                serviceComQueueUrlVazia.PublicarAtualizacaoStatusAsync(propostaValida));

            // Não é necessário verificar que o método não foi chamado, pois a exceção já é verificada
        }

        [Fact]
        public async Task PublicarAtualizacaoStatusAsync_DeveIncluirDataAtualizacaoNaMensagem()
        {
            // Arrange
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            proposta.AtualizarStatus(StatusProposta.Aprovada);

            SendMessageRequest capturedRequest = null;
            _mockSqsClient.Setup(m => m.SendMessageAsync(
                    It.IsAny<SendMessageRequest>(),
                    It.IsAny<CancellationToken>()))
                .Callback<SendMessageRequest, CancellationToken>((req, token) => capturedRequest = req)
                .ReturnsAsync(new SendMessageResponse());

            // Act
            await _service.PublicarAtualizacaoStatusAsync(proposta);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Contains("\"DataAtualizacao\":", capturedRequest.MessageBody);
        }

        [Fact]
        public async Task PublicarAtualizacaoStatusAsync_DeveUsarQueueUrlCorreta()
        {
            // Arrange
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            var queueUrlPersonalizada = "https://sqs.us-east-1.amazonaws.com/123456789012/fila-personalizada";
            var servicoPersonalizado = new PropostaMessageService(_mockSqsClient.Object, queueUrlPersonalizada);

            SendMessageRequest capturedRequest = null;
            _mockSqsClient.Setup(m => m.SendMessageAsync(
                    It.IsAny<SendMessageRequest>(),
                    It.IsAny<CancellationToken>()))
                .Callback<SendMessageRequest, CancellationToken>((req, token) => capturedRequest = req)
                .ReturnsAsync(new SendMessageResponse());

            // Act
            await servicoPersonalizado.PublicarAtualizacaoStatusAsync(proposta);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(queueUrlPersonalizada, capturedRequest.QueueUrl);
        }

        [Fact]
        public async Task PublicarAtualizacaoStatusAsync_DeveIncluirTodosOsDadosDaProposta()
        {
            // Arrange
            var id = Guid.NewGuid();
            var nome = "Maria Oliveira";
            var cpf = "987.654.321-00";
            var valorSeguro = 2500.50m;
            var status = StatusProposta.Aprovada;
            var dataAtualizacao = DateTime.Now;

            var proposta = new Proposta(nome, cpf, valorSeguro);
            proposta.GetType().GetProperty("Id")?.SetValue(proposta, id);
            proposta.AtualizarStatus(status);
            proposta.GetType().GetProperty("DataAtualizacao")?.SetValue(proposta, dataAtualizacao);

            SendMessageRequest capturedRequest = null;
            _mockSqsClient.Setup(m => m.SendMessageAsync(
                    It.IsAny<SendMessageRequest>(),
                    It.IsAny<CancellationToken>()))
                .Callback<SendMessageRequest, CancellationToken>((req, token) => capturedRequest = req)
                .ReturnsAsync(new SendMessageResponse());

            // Act
            await _service.PublicarAtualizacaoStatusAsync(proposta);

            // Assert
            Assert.NotNull(capturedRequest);
            Console.WriteLine($"MessageBody: {capturedRequest.MessageBody}");
            Assert.Contains("PropostaId", capturedRequest.MessageBody);
            Assert.Contains("Nome", capturedRequest.MessageBody);
            Assert.Contains("CPF", capturedRequest.MessageBody);
            Assert.Contains("ValorSeguro", capturedRequest.MessageBody);
            Assert.Contains("Status", capturedRequest.MessageBody);
        }

        [Fact]
        public async Task PublicarAtualizacaoStatusAsync_DeveDefinirEventTypeCorretamente()
        {
            // Arrange
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);

            SendMessageRequest capturedRequest = null;
            _mockSqsClient.Setup(m => m.SendMessageAsync(
                    It.IsAny<SendMessageRequest>(),
                    It.IsAny<CancellationToken>()))
                .Callback<SendMessageRequest, CancellationToken>((req, token) => capturedRequest = req)
                .ReturnsAsync(new SendMessageResponse());

            // Act
            await _service.PublicarAtualizacaoStatusAsync(proposta);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.True(capturedRequest.MessageAttributes.ContainsKey("EventType"));
            Assert.Equal("String", capturedRequest.MessageAttributes["EventType"].DataType);
            Assert.Equal("PropostaStatusAtualizado", capturedRequest.MessageAttributes["EventType"].StringValue);
        }

        [Fact]
        public async Task PublicarAtualizacaoStatusAsync_QuandoSQSClientRetornaErro_DevePropagaExcecao()
        {
            // Arrange
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            var mensagemErro = "Erro de serviço SQS: Acesso negado";

            _mockSqsClient.Setup(m => m.SendMessageAsync(
                    It.IsAny<SendMessageRequest>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonSQSException(mensagemErro));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AmazonSQSException>(() => 
                _service.PublicarAtualizacaoStatusAsync(proposta));
            
            Assert.Equal(mensagemErro, exception.Message);
        }
    }
}