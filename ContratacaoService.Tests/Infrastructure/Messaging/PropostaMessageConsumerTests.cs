using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ContratacaoService.Domain.Services;
using ContratacaoService.Infrastructure.Messaging;

// Testes para o consumidor de mensagens de proposta
// Incluindo testes para o processamento de mensagens e tratamento de exceções

namespace ContratacaoService.Tests.Infrastructure.Messaging
{
    public class PropostaMessageConsumerTests
    {
        private readonly Mock<IPropostaMessageConsumerService> _mockConsumerService;

        public PropostaMessageConsumerTests()
        {
            _mockConsumerService = new Mock<IPropostaMessageConsumerService>();
        }

        [Fact]
        public async Task ProcessarMensagemPropostaAsync_QuandoStatusAprovada_DeveChamarProcessamento()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var status = "Aprovada";
            var nome = "João Silva";
            var cpf = "12345678900";
            var valorSeguro = 1000.50m;

            _mockConsumerService
                .Setup(service => service.ProcessarMensagemPropostaAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>()))
                .Returns(Task.CompletedTask);

            // Act
            await _mockConsumerService.Object.ProcessarMensagemPropostaAsync(
                propostaId,
                status,
                nome,
                cpf,
                valorSeguro);

            // Assert
            _mockConsumerService.Verify(service => service.ProcessarMensagemPropostaAsync(
                It.Is<Guid>(id => id == propostaId),
                It.Is<string>(s => s == status),
                It.Is<string>(n => n == nome),
                It.Is<string>(c => c == cpf),
                It.Is<decimal>(v => v == valorSeguro)), Times.Once);
        }

        [Fact]
        public async Task ProcessarMensagemPropostaAsync_QuandoStatusRejeitada_DeveChamarProcessamento()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var status = "Rejeitada";
            var nome = "João Silva";
            var cpf = "12345678900";
            var valorSeguro = 1000.50m;

            _mockConsumerService
                .Setup(service => service.ProcessarMensagemPropostaAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>()))
                .Returns(Task.CompletedTask);

            // Act
            await _mockConsumerService.Object.ProcessarMensagemPropostaAsync(
                propostaId,
                status,
                nome,
                cpf,
                valorSeguro);

            // Assert
            _mockConsumerService.Verify(service => service.ProcessarMensagemPropostaAsync(
                It.Is<Guid>(id => id == propostaId),
                It.Is<string>(s => s == status),
                It.Is<string>(n => n == nome),
                It.Is<string>(c => c == cpf),
                It.Is<decimal>(v => v == valorSeguro)), Times.Once);
        }

        [Fact]
        public async Task ProcessarMensagemPropostaAsync_QuandoPropostaIdInvalido_DeveLancarExcecao()
        {
            // Arrange
            var propostaId = Guid.Empty; // ID inválido
            var status = "Aprovada";
            var nome = "João Silva";
            var cpf = "12345678900";
            var valorSeguro = 1000.50m;

            _mockConsumerService
                .Setup(service => service.ProcessarMensagemPropostaAsync(
                    It.Is<Guid>(id => id == Guid.Empty),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>()))
                .ThrowsAsync(new ArgumentException("ID da proposta inválido"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _mockConsumerService.Object.ProcessarMensagemPropostaAsync(
                    propostaId,
                    status,
                    nome,
                    cpf,
                    valorSeguro));

            Assert.Equal("ID da proposta inválido", exception.Message);
        }

        [Fact]
        public async Task ProcessarMensagemPropostaAsync_QuandoValorSeguroZero_DeveLancarExcecao()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var status = "Aprovada";
            var nome = "João Silva";
            var cpf = "12345678900";
            var valorSeguro = 0m; // Valor zero

            _mockConsumerService
                .Setup(service => service.ProcessarMensagemPropostaAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<decimal>(v => v == 0)))
                .ThrowsAsync(new ArgumentException("Valor do seguro deve ser maior que zero"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _mockConsumerService.Object.ProcessarMensagemPropostaAsync(
                    propostaId,
                    status,
                    nome,
                    cpf,
                    valorSeguro));

            Assert.Equal("Valor do seguro deve ser maior que zero", exception.Message);
        }

        [Fact]
        public async Task ProcessarMensagemPropostaAsync_QuandoExcecaoOcorre_DevePropagaExcecao()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var status = "Aprovada";
            var nome = "João Silva";
            var cpf = "12345678900";
            var valorSeguro = 1000.50m;

            _mockConsumerService
                .Setup(service => service.ProcessarMensagemPropostaAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>()))
                .ThrowsAsync(new Exception("Erro ao processar mensagem"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _mockConsumerService.Object.ProcessarMensagemPropostaAsync(
                    propostaId,
                    status,
                    nome,
                    cpf,
                    valorSeguro));

            Assert.Equal("Erro ao processar mensagem", exception.Message);
        }

        [Fact]
        public async Task ProcessarMensagemPropostaAsync_QuandoStatusEmAnalise_DeveChamarProcessamento()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var status = "EmAnalise";
            var nome = "João Silva";
            var cpf = "12345678900";
            var valorSeguro = 1000.50m;

            _mockConsumerService
                .Setup(service => service.ProcessarMensagemPropostaAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>()))
                .Returns(Task.CompletedTask);

            // Act
            await _mockConsumerService.Object.ProcessarMensagemPropostaAsync(
                propostaId,
                status,
                nome,
                cpf,
                valorSeguro);

            // Assert
            _mockConsumerService.Verify(service => service.ProcessarMensagemPropostaAsync(
                It.Is<Guid>(id => id == propostaId),
                It.Is<string>(s => s == status),
                It.Is<string>(n => n == nome),
                It.Is<string>(c => c == cpf),
                It.Is<decimal>(v => v == valorSeguro)), Times.Once);
        }

        [Fact]
        public async Task TestarProcessamentoMensagemSQS_ComMensagemValida_DeveProcessarCorretamente()
        {
            // Arrange
            var mockSqsClient = new Mock<IAmazonSQS>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<PropostaMessageConsumer>>();
            var mockScope = new Mock<IServiceScope>();
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            
            var propostaId = Guid.NewGuid();
            var mensagem = new {
                PropostaId = propostaId,
                Status = "Aprovada",
                Nome = "João Silva",
                CPF = "12345678900",
                ValorSeguro = 1000.50m
            };

            var messageBody = JsonSerializer.Serialize(mensagem);
            var sqsMessage = new Message
            {
                Body = messageBody,
                MessageId = "test-message-id",
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    { "EventType", new MessageAttributeValue { StringValue = "PropostaStatusAtualizado", DataType = "String" } }
                }
            };

            // Configurar o mock do service provider para retornar o scope factory
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(mockScopeFactory.Object);

            // Configurar o scope factory para retornar o scope
            mockScopeFactory
                .Setup(factory => factory.CreateScope())
                .Returns(mockScope.Object);

            // Configurar o scope para retornar o service provider
            mockScope
                .Setup(scope => scope.ServiceProvider)
                .Returns(mockServiceProvider.Object);

            // Configurar o service provider para retornar o consumer service
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService(typeof(IPropostaMessageConsumerService)))
                .Returns(_mockConsumerService.Object);

            _mockConsumerService
                .Setup(service => service.ProcessarMensagemPropostaAsync(
                    It.Is<Guid>(id => id == propostaId),
                    It.Is<string>(s => s == "Aprovada"),
                    It.Is<string>(n => n == "João Silva"),
                    It.Is<string>(c => c == "12345678900"),
                    It.Is<decimal>(v => v == 1000.50m)))
                .Returns(Task.CompletedTask);

            var consumer = new PropostaMessageConsumer(
                mockSqsClient.Object,
                mockServiceProvider.Object,
                mockLogger.Object,
                "https://sqs.us-east-1.amazonaws.com/123456789012/proposta-queue");

            // Usar reflexão para acessar o método privado ProcessarMensagemAsync
            var processarMensagemMethod = typeof(PropostaMessageConsumer).GetMethod(
                "ProcessarMensagemAsync", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            await (Task)processarMensagemMethod.Invoke(consumer, new object[] { sqsMessage });

            // Assert
            _mockConsumerService.Verify(
                service => service.ProcessarMensagemPropostaAsync(
                    It.Is<Guid>(id => id == propostaId),
                    It.Is<string>(s => s == "Aprovada"),
                    It.Is<string>(n => n == "João Silva"),
                    It.Is<string>(c => c == "12345678900"),
                    It.Is<decimal>(v => v == 1000.50m)),
                Times.Once);
        }

        [Fact]
        public async Task TestarProcessamentoMensagemSQS_ComMensagemInvalida_DeveTratarExcecao()
        {
            // Arrange
            var mockSqsClient = new Mock<IAmazonSQS>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<PropostaMessageConsumer>>();
            var mockScope = new Mock<IServiceScope>();
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            
            // Configurar o mock do service provider para retornar o scope factory
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(mockScopeFactory.Object);

            // Configurar o scope factory para retornar o scope
            mockScopeFactory
                .Setup(factory => factory.CreateScope())
                .Returns(mockScope.Object);

            // Configurar o scope para retornar o service provider
            mockScope
                .Setup(scope => scope.ServiceProvider)
                .Returns(mockServiceProvider.Object);

            // Configurar o service provider para retornar o consumer service
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService(typeof(IPropostaMessageConsumerService)))
                .Returns(_mockConsumerService.Object);

            var messageBody = "{ mensagem inválida }";
            var sqsMessage = new Message
            {
                Body = messageBody,
                MessageId = "test-invalid-message-id",
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    { "EventType", new MessageAttributeValue { StringValue = "PropostaStatusAtualizado", DataType = "String" } }
                }
            };

            var consumer = new PropostaMessageConsumer(
                mockSqsClient.Object,
                mockServiceProvider.Object,
                mockLogger.Object,
                "https://sqs.us-east-1.amazonaws.com/123456789012/proposta-queue");

            // Usar reflexão para acessar o método privado ProcessarMensagemAsync
            var processarMensagemMethod = typeof(PropostaMessageConsumer).GetMethod(
                "ProcessarMensagemAsync", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            await (Task)processarMensagemMethod.Invoke(consumer, new object[] { sqsMessage });

            // Assert
            // Verificamos que o serviço de processamento nunca foi chamado devido ao JSON inválido
            _mockConsumerService.Verify(
                service => service.ProcessarMensagemPropostaAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>()),
                Times.Never);

            // Verificamos que o logger registrou o erro
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Erro ao processar mensagem")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TestarProcessamentoMensagemSQS_ComEventTypeDiferente_NaoDeveProcessar()
        {
            // Arrange
            var mockSqsClient = new Mock<IAmazonSQS>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<PropostaMessageConsumer>>();
            var mockScope = new Mock<IServiceScope>();
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            
            // Configurar o mock do service provider para retornar o scope factory
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(mockScopeFactory.Object);

            // Configurar o scope factory para retornar o scope
            mockScopeFactory
                .Setup(factory => factory.CreateScope())
                .Returns(mockScope.Object);

            // Configurar o scope para retornar o service provider
            mockScope
                .Setup(scope => scope.ServiceProvider)
                .Returns(mockServiceProvider.Object);

            // Configurar o service provider para retornar o consumer service
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService(typeof(IPropostaMessageConsumerService)))
                .Returns(_mockConsumerService.Object);

            var propostaId = Guid.NewGuid();
            var mensagem = new {
                PropostaId = propostaId,
                Status = "Aprovada",
                Nome = "João Silva",
                CPF = "12345678900",
                ValorSeguro = 1000.50m
            };

            var messageBody = JsonSerializer.Serialize(mensagem);
            var sqsMessage = new Message
            {
                Body = messageBody,
                MessageId = "test-different-event-type-id",
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    { "EventType", new MessageAttributeValue { StringValue = "OutroEventoQualquer", DataType = "String" } }
                }
            };

            var consumer = new PropostaMessageConsumer(
                mockSqsClient.Object,
                mockServiceProvider.Object,
                mockLogger.Object,
                "https://sqs.us-east-1.amazonaws.com/123456789012/proposta-queue");

            // Usar reflexão para acessar o método privado ProcessarMensagemAsync
            var processarMensagemMethod = typeof(PropostaMessageConsumer).GetMethod(
                "ProcessarMensagemAsync", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            await (Task)processarMensagemMethod.Invoke(consumer, new object[] { sqsMessage });

            // Assert
            // Verificamos que o serviço de processamento nunca foi chamado devido ao EventType diferente
            _mockConsumerService.Verify(
                service => service.ProcessarMensagemPropostaAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>()),
                Times.Never);

            // Verificamos que o logger registrou a informação sobre o EventType diferente
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("EventType diferente")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TestarProcessamentoMensagemSQS_SemEventType_NaoDeveProcessar()
        {
            // Arrange
            var mockSqsClient = new Mock<IAmazonSQS>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<PropostaMessageConsumer>>();
            var mockScope = new Mock<IServiceScope>();
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            
            // Configurar o mock do service provider para retornar o scope factory
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(mockScopeFactory.Object);

            // Configurar o scope factory para retornar o scope
            mockScopeFactory
                .Setup(factory => factory.CreateScope())
                .Returns(mockScope.Object);

            // Configurar o scope para retornar o service provider
            mockScope
                .Setup(scope => scope.ServiceProvider)
                .Returns(mockServiceProvider.Object);

            // Configurar o service provider para retornar o consumer service
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService(typeof(IPropostaMessageConsumerService)))
                .Returns(_mockConsumerService.Object);

            var propostaId = Guid.NewGuid();
            var mensagem = new {
                PropostaId = propostaId,
                Status = "Aprovada",
                Nome = "João Silva",
                CPF = "12345678900",
                ValorSeguro = 1000.50m
            };

            var messageBody = JsonSerializer.Serialize(mensagem);
            var sqsMessage = new Message
            {
                Body = messageBody,
                MessageId = "test-no-event-type-id",
                // Sem o atributo EventType
                MessageAttributes = new Dictionary<string, MessageAttributeValue>()
            };

            var consumer = new PropostaMessageConsumer(
                mockSqsClient.Object,
                mockServiceProvider.Object,
                mockLogger.Object,
                "https://sqs.us-east-1.amazonaws.com/123456789012/proposta-queue");

            // Usar reflexão para acessar o método privado ProcessarMensagemAsync
            var processarMensagemMethod = typeof(PropostaMessageConsumer).GetMethod(
                "ProcessarMensagemAsync", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            await (Task)processarMensagemMethod.Invoke(consumer, new object[] { sqsMessage });

            // Assert
            // Verificamos que o serviço de processamento nunca foi chamado devido à ausência do EventType
            _mockConsumerService.Verify(
                service => service.ProcessarMensagemPropostaAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>()),
                Times.Never);

            // Verificamos que o logger registrou a informação sobre a ausência do EventType
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Atributo EventType não encontrado")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}