using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.Services;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ContratacaoService.Tests.Application.Services
{
    public class PropostaMessageConsumerServiceTests
    {
        private readonly Mock<IContratoService> _mockContratoService;
        private readonly PropostaMessageConsumerService _service;

        public PropostaMessageConsumerServiceTests()
        {
            _mockContratoService = new Mock<IContratoService>();
            _service = new PropostaMessageConsumerService(_mockContratoService.Object);
        }

        [Fact]
        public async Task ProcessarMensagemPropostaAsync_QuandoStatusAprovada_DeveCriarContrato()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var status = "Aprovada";
            var nome = "João Silva";
            var cpf = "12345678900";
            var valorSeguro = 1000.50m;

            _mockContratoService.Setup(s => s.CriarContratoAsync(It.IsAny<CriarContratoDTO>()))
                .ReturnsAsync(new ContratoDTO());

            // Act
            await _service.ProcessarMensagemPropostaAsync(propostaId, status, nome, cpf, valorSeguro);

            // Assert
            _mockContratoService.Verify(s => s.CriarContratoAsync(It.Is<CriarContratoDTO>(dto =>
                dto.PropostaId == propostaId &&
                dto.Nome == nome &&
                dto.CPF == cpf &&
                dto.ValorSeguro == valorSeguro &&
                dto.DuracaoMeses == 12)), Times.Once);
        }

        [Theory]
        [InlineData("Rejeitada")]
        [InlineData("EmAnalise")]
        [InlineData("Pendente")]
        public async Task ProcessarMensagemPropostaAsync_QuandoStatusNaoAprovada_NaoDeveCriarContrato(string status)
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var nome = "João Silva";
            var cpf = "12345678900";
            var valorSeguro = 1000.50m;

            // Act
            await _service.ProcessarMensagemPropostaAsync(propostaId, status, nome, cpf, valorSeguro);

            // Assert
            _mockContratoService.Verify(s => s.CriarContratoAsync(It.IsAny<CriarContratoDTO>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarMensagemPropostaAsync_QuandoStatusAprovadaComCaseDiferente_DeveCriarContrato()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var status = "APROVADA"; // Maiúsculas
            var nome = "João Silva";
            var cpf = "12345678900";
            var valorSeguro = 1000.50m;

            _mockContratoService.Setup(s => s.CriarContratoAsync(It.IsAny<CriarContratoDTO>()))
                .ReturnsAsync(new ContratoDTO());

            // Act
            await _service.ProcessarMensagemPropostaAsync(propostaId, status, nome, cpf, valorSeguro);

            // Assert
            _mockContratoService.Verify(s => s.CriarContratoAsync(It.IsAny<CriarContratoDTO>()), Times.Once);
        }

        [Fact]
        public async Task ProcessarMensagemPropostaAsync_QuandoValorSeguroNegativo_DeveLancarExcecao()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var status = "Aprovada";
            var nome = "João Silva";
            var cpf = "12345678900";
            var valorSeguro = -100.50m; // Valor negativo

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.ProcessarMensagemPropostaAsync(propostaId, status, nome, cpf, valorSeguro));

            _mockContratoService.Verify(s => s.CriarContratoAsync(It.IsAny<CriarContratoDTO>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarMensagemPropostaAsync_QuandoCPFInvalido_DeveLancarExcecao()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var status = "Aprovada";
            var nome = "João Silva";
            var cpf = "123456"; // CPF inválido (muito curto)
            var valorSeguro = 1000.50m;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.ProcessarMensagemPropostaAsync(propostaId, status, nome, cpf, valorSeguro));

            _mockContratoService.Verify(s => s.CriarContratoAsync(It.IsAny<CriarContratoDTO>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarMensagemPropostaAsync_QuandoNomeVazio_DeveLancarExcecao()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var status = "Aprovada";
            var nome = ""; // Nome vazio
            var cpf = "12345678900";
            var valorSeguro = 1000.50m;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.ProcessarMensagemPropostaAsync(propostaId, status, nome, cpf, valorSeguro));

            _mockContratoService.Verify(s => s.CriarContratoAsync(It.IsAny<CriarContratoDTO>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarMensagemPropostaAsync_QuandoErroAoCriarContrato_DevePropagaExcecao()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var status = "Aprovada";
            var nome = "João Silva";
            var cpf = "12345678900";
            var valorSeguro = 1000.50m;

            _mockContratoService.Setup(s => s.CriarContratoAsync(It.IsAny<CriarContratoDTO>()))
                .ThrowsAsync(new Exception("Erro ao criar contrato"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _service.ProcessarMensagemPropostaAsync(propostaId, status, nome, cpf, valorSeguro));

            Assert.Equal("Erro ao criar contrato", exception.Message);
        }

        [Fact]
        public async Task ProcessarMensagemPropostaAsync_QuandoStatusAprovada_DeveCriarContratoComDuracaoCorreta()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var status = "Aprovada";
            var nome = "João Silva";
            var cpf = "12345678900";
            var valorSeguro = 5000.00m; // Valor alto para testar duração diferente

            _mockContratoService.Setup(s => s.CriarContratoAsync(It.IsAny<CriarContratoDTO>()))
                .ReturnsAsync(new ContratoDTO());

            // Act
            await _service.ProcessarMensagemPropostaAsync(propostaId, status, nome, cpf, valorSeguro);

            // Assert
            _mockContratoService.Verify(s => s.CriarContratoAsync(It.Is<CriarContratoDTO>(dto =>
                dto.DuracaoMeses == 12)), Times.Once);
        }

        [Fact]
        public async Task ProcessarMensagemPropostaAsync_QuandoStatusDesconhecido_NaoDeveCriarContrato()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var status = "StatusInexistente"; // Status que não existe
            var nome = "João Silva";
            var cpf = "12345678900";
            var valorSeguro = 1000.50m;

            // Act
            await _service.ProcessarMensagemPropostaAsync(propostaId, status, nome, cpf, valorSeguro);

            // Assert
            _mockContratoService.Verify(s => s.CriarContratoAsync(It.IsAny<CriarContratoDTO>()), Times.Never);
        }
    }
}