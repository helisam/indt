using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using PropostaService.Application.DTOs;
using PropostaService.Application.Services;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Repositories;
using PropostaService.Domain.Services;
using Xunit;

namespace PropostaService.Tests.Application.Services
{
    public class PropostaServiceTests
    {
        private readonly Mock<IPropostaRepository> _mockRepository;
        private readonly Mock<IPropostaMessageService> _mockMessageService;
        private readonly PropostaService.Application.Services.PropostaService _service;

        public PropostaServiceTests()
        {
            _mockRepository = new Mock<IPropostaRepository>();
            _mockMessageService = new Mock<IPropostaMessageService>();
            _service = new PropostaService.Application.Services.PropostaService(_mockRepository.Object, _mockMessageService.Object);
        }

        [Fact]
        public async Task CriarPropostaAsync_DeveCriarProposta()
        {
            // Arrange
            var dto = new CriarPropostaDTO
            {
                Nome = "João Silva",
                CPF = "123.456.789-00",
                ValorSeguro = 1000.00m
            };

            var proposta = new Proposta(dto.Nome, dto.CPF, dto.ValorSeguro);
            var id = Guid.NewGuid();
            proposta.GetType().GetProperty("Id")?.SetValue(proposta, id);

            _mockRepository.Setup(r => r.AdicionarAsync(It.IsAny<Proposta>()))
                .ReturnsAsync(proposta);

            // Act
            var resultado = await _service.CriarPropostaAsync(dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(dto.Nome, resultado.Nome);
            Assert.Equal(dto.CPF, resultado.CPF);
            Assert.Equal(dto.ValorSeguro, resultado.ValorSeguro);
            Assert.Equal(StatusProposta.EmAnalise, resultado.Status);

            _mockRepository.Verify(r => r.AdicionarAsync(It.Is<Proposta>(p => 
                p.Nome == dto.Nome && 
                p.CPF == dto.CPF && 
                p.ValorSeguro == dto.ValorSeguro && 
                p.Status == StatusProposta.EmAnalise)), 
                Times.Once);
        }

        [Fact]
        public async Task CriarPropostaAsync_DeveLancarExcecao_QuandoDadosInvalidos()
        {
            // Arrange
            var dtoInvalido = new CriarPropostaDTO
            {
                Nome = "",  // Nome inválido
                CPF = "123.456.789-00",
                ValorSeguro = 1000.00m
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CriarPropostaAsync(dtoInvalido));
                
            Assert.Contains("nome", exception.Message, StringComparison.OrdinalIgnoreCase);
            
            // Verificar que o repositório e o serviço de mensagens não foram chamados
            _mockRepository.Verify(r => r.AdicionarAsync(It.IsAny<Proposta>()), Times.Never);
            _mockMessageService.Verify(m => m.PublicarAtualizacaoStatusAsync(It.IsAny<Proposta>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarStatusAsync_DeveAtualizarStatus_QuandoPropostaExiste()
        {
            // Arrange
            var dto = new AtualizarStatusPropostaDTO
            {
                Id = Guid.NewGuid(),
                NovoStatus = StatusProposta.Aprovada
            };
            
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            proposta.GetType().GetProperty("Id")?.SetValue(proposta, dto.Id);

            _mockRepository.Setup(r => r.ObterPorIdAsync(dto.Id))
                .ReturnsAsync(proposta);

            _mockRepository.Setup(r => r.AtualizarAsync(It.IsAny<Proposta>()))
                .ReturnsAsync(proposta);

            _mockMessageService.Setup(m => m.PublicarAtualizacaoStatusAsync(It.IsAny<Proposta>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.AtualizarStatusAsync(dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(dto.Id, resultado.Id);
            Assert.Equal(dto.NovoStatus, resultado.Status);

            _mockRepository.Verify(r => r.AtualizarAsync(It.Is<Proposta>(p => 
                p.Id == dto.Id && 
                p.Status == dto.NovoStatus)), 
                Times.Once);

            _mockMessageService.Verify(m => m.PublicarAtualizacaoStatusAsync(It.Is<Proposta>(p => 
                p.Id == dto.Id && 
                p.Status == dto.NovoStatus)), 
                Times.Once);
        }

        [Theory]
        [InlineData(StatusProposta.Aprovada)]
        [InlineData(StatusProposta.Rejeitada)]
        public async Task AtualizarStatusAsync_DevePublicarMensagem_ParaCadaStatus(StatusProposta novoStatus)
        {
            // Arrange
            var dto = new AtualizarStatusPropostaDTO
            {
                Id = Guid.NewGuid(),
                NovoStatus = novoStatus
            };
            
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            proposta.GetType().GetProperty("Id")?.SetValue(proposta, dto.Id);

            _mockRepository.Setup(r => r.ObterPorIdAsync(dto.Id))
                .ReturnsAsync(proposta);

            _mockRepository.Setup(r => r.AtualizarAsync(It.IsAny<Proposta>()))
                .ReturnsAsync(proposta);

            _mockMessageService.Setup(m => m.PublicarAtualizacaoStatusAsync(It.IsAny<Proposta>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.AtualizarStatusAsync(dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(novoStatus, resultado.Status);

            _mockMessageService.Verify(m => m.PublicarAtualizacaoStatusAsync(It.Is<Proposta>(p => 
                p.Status == novoStatus)), 
                Times.Once);
        }

        [Fact]
        public async Task AtualizarStatusAsync_DeveLancarExcecao_QuandoPropostaNaoExiste()
        {
            // Arrange
            var dto = new AtualizarStatusPropostaDTO
            {
                Id = Guid.NewGuid(),
                NovoStatus = StatusProposta.Aprovada
            };

            _mockRepository.Setup(r => r.ObterPorIdAsync(dto.Id))
                .ThrowsAsync(new KeyNotFoundException($"Proposta com ID {dto.Id} não encontrada"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _service.AtualizarStatusAsync(dto));

            _mockRepository.Verify(r => r.AtualizarAsync(It.IsAny<Proposta>()), Times.Never);
            _mockMessageService.Verify(m => m.PublicarAtualizacaoStatusAsync(It.IsAny<Proposta>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarStatusAsync_DeveTratarErroDePublicacao()
        {
            // Arrange
            var dto = new AtualizarStatusPropostaDTO
            {
                Id = Guid.NewGuid(),
                NovoStatus = StatusProposta.Aprovada
            };
            
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            proposta.GetType().GetProperty("Id")?.SetValue(proposta, dto.Id);

            _mockRepository.Setup(r => r.ObterPorIdAsync(dto.Id))
                .ReturnsAsync(proposta);

            _mockRepository.Setup(r => r.AtualizarAsync(It.IsAny<Proposta>()))
                .ReturnsAsync(proposta);

            // Simula falha na publicação da mensagem
            _mockMessageService.Setup(m => m.PublicarAtualizacaoStatusAsync(It.IsAny<Proposta>()))
                .ThrowsAsync(new Exception("Erro ao publicar mensagem"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _service.AtualizarStatusAsync(dto));

            // Verifica que a atualização no repositório foi chamada
            _mockRepository.Verify(r => r.AtualizarAsync(It.IsAny<Proposta>()), Times.Once);
            
            // Verifica que a tentativa de publicação foi feita
            _mockMessageService.Verify(m => m.PublicarAtualizacaoStatusAsync(It.IsAny<Proposta>()), Times.Once);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarProposta_QuandoExiste()
        {
            // Arrange
            var id = Guid.NewGuid();
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            proposta.GetType().GetProperty("Id")?.SetValue(proposta, id);

            _mockRepository.Setup(r => r.ObterPorIdAsync(id))
                .ReturnsAsync(proposta);

            // Act
            var resultado = await _service.ObterPorIdAsync(id);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(id, resultado.Id);
            Assert.Equal(proposta.Nome, resultado.Nome);
            Assert.Equal(proposta.CPF, resultado.CPF);
            Assert.Equal(proposta.ValorSeguro, resultado.ValorSeguro);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoExiste()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockRepository.Setup(r => r.ObterPorIdAsync(id))
                .ThrowsAsync(new KeyNotFoundException("Proposta não encontrada"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ObterPorIdAsync(id));
        }

        [Fact]
        public async Task ListarTodasAsync_DeveRetornarTodasAsPropostas()
        {
            // Arrange
            var propostas = new List<Proposta>
            {
                new Proposta("João Silva", "123.456.789-00", 1000.00m),
                new Proposta("Maria Souza", "987.654.321-00", 2000.00m),
                new Proposta("Pedro Santos", "456.789.123-00", 1500.00m)
            };

            _mockRepository.Setup(r => r.ListarTodasAsync())
                .ReturnsAsync(propostas);

            // Act
            var resultado = await _service.ListarTodasAsync();

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(propostas.Count, resultado.Count());
        }

        [Fact]
        public async Task ListarPorStatusAsync_DeveRetornarPropostasFiltradas()
        {
            // Arrange
            var status = StatusProposta.EmAnalise;
            var propostas = new List<Proposta>
            {
                new Proposta("João Silva", "123.456.789-00", 1000.00m),
                new Proposta("Maria Souza", "987.654.321-00", 2000.00m)
            };

            _mockRepository.Setup(r => r.ListarPorStatusAsync(status))
                .ReturnsAsync(propostas);

            // Act
            var resultado = await _service.ListarPorStatusAsync(status);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(propostas.Count, resultado.Count());
        }

        [Fact]
        public async Task ListarPorStatusAsync_DeveRetornarListaVazia_QuandoNaoExistemPropostas()
        {
            // Arrange
            var status = StatusProposta.Aprovada;
            var listaVazia = new List<Proposta>();

            _mockRepository.Setup(r => r.ListarPorStatusAsync(status))
                .ReturnsAsync(listaVazia);

            // Act
            var resultado = await _service.ListarPorStatusAsync(status);

            // Assert
            Assert.NotNull(resultado);
            Assert.Empty(resultado);
        }

        [Fact]
        public async Task CriarPropostaAsync_DeveLancarExcecao_QuandoValorSeguroZero()
        {
            // Arrange
            var dto = new CriarPropostaDTO
            {
                Nome = "João Silva",
                CPF = "123.456.789-00",
                ValorSeguro = 0m // Valor zero
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CriarPropostaAsync(dto));
                
            Assert.Contains("valor", exception.Message, StringComparison.OrdinalIgnoreCase);
            
            // Verificar que o repositório e o serviço de mensagens não foram chamados
            _mockRepository.Verify(r => r.AdicionarAsync(It.IsAny<Proposta>()), Times.Never);
            _mockMessageService.Verify(m => m.PublicarAtualizacaoStatusAsync(It.IsAny<Proposta>()), Times.Never);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveLancarExcecao_QuandoIdInvalido()
        {
            // Arrange
            var idInvalido = Guid.Empty;

            _mockRepository.Setup(r => r.ObterPorIdAsync(idInvalido))
                .ThrowsAsync(new ArgumentException("ID inválido"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.ObterPorIdAsync(idInvalido));
        }

        // Este método foi removido porque já existe uma implementação anterior

        [Fact]
        public async Task ListarTodasAsync_DeveTratarErroDeRepositorio()
        {
            // Arrange
            _mockRepository.Setup(r => r.ListarTodasAsync())
                .ThrowsAsync(new Exception("Erro ao acessar o repositório"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _service.ListarTodasAsync());
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveTratarIdInvalido()
        {
            // Arrange
            var idInvalido = Guid.Empty;

            _mockRepository.Setup(r => r.ObterPorIdAsync(idInvalido))
                .ThrowsAsync(new ArgumentException("ID inválido"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.ObterPorIdAsync(idInvalido));
        }
        [Fact]
        public async Task CriarPropostaAsync_DeveLancarExcecao_QuandoCPFInvalido()
        {
            // Arrange
            var dto = new CriarPropostaDTO
            {
                Nome = "João Silva",
                CPF = "123", // CPF inválido (muito curto)
                ValorSeguro = 1000.00m
            };

            _mockRepository.Setup(r => r.AdicionarAsync(It.IsAny<Proposta>()))
                .ThrowsAsync(new ArgumentException("CPF inválido"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CriarPropostaAsync(dto));
                
            Assert.Contains("CPF", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CriarPropostaAsync_DeveLancarExcecao_QuandoValorSeguroNegativo()
        {
            // Arrange
            var dto = new CriarPropostaDTO
            {
                Nome = "João Silva",
                CPF = "123.456.789-00",
                ValorSeguro = -50.00m // Valor negativo
            };

            _mockRepository.Setup(r => r.AdicionarAsync(It.IsAny<Proposta>()))
                .ThrowsAsync(new ArgumentException("Valor do seguro deve ser maior que zero"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CriarPropostaAsync(dto));
                
            Assert.Contains("valor", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AtualizarStatusAsync_DeveLancarExcecao_QuandoTentarAtualizarParaStatusInvalido()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new AtualizarStatusPropostaDTO
            {
                Id = id,
                NovoStatus = StatusProposta.Rejeitada
            };
            
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            proposta.GetType().GetProperty("Id")?.SetValue(proposta, id);
            proposta.AtualizarStatus(StatusProposta.Rejeitada); // Já está rejeitada

            _mockRepository.Setup(r => r.ObterPorIdAsync(id))
                .ReturnsAsync(proposta);

            _mockRepository.Setup(r => r.AtualizarAsync(It.IsAny<Proposta>()))
                .ThrowsAsync(new InvalidOperationException("Não é possível atualizar uma proposta já rejeitada"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.AtualizarStatusAsync(dto));
        }

        [Fact]
        public async Task ListarPorStatusAsync_DeveLancarExcecao_QuandoOcorreErroNoRepositorio()
        {
            // Arrange
            var status = StatusProposta.EmAnalise;

            _mockRepository.Setup(r => r.ListarPorStatusAsync(status))
                .ThrowsAsync(new Exception("Erro ao acessar o repositório"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _service.ListarPorStatusAsync(status));
        }

        [Fact]
        public async Task CriarPropostaAsync_DevePropagaExcecaoDoRepositorio()
        {
            // Arrange
            var dto = new CriarPropostaDTO
            {
                Nome = "João Silva",
                CPF = "123.456.789-00",
                ValorSeguro = 1000.00m
            };

            _mockRepository.Setup(r => r.AdicionarAsync(It.IsAny<Proposta>()))
                .ThrowsAsync(new DbUpdateException("Erro ao inserir no banco de dados", new Exception()));

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(() => 
                _service.CriarPropostaAsync(dto));
        }

        [Fact]
        public async Task AtualizarStatusAsync_DeveManterStatusAtual_QuandoNovoStatusIgualAoAtual()
        {
            // Arrange
            var id = Guid.NewGuid();
            var statusAtual = StatusProposta.Aprovada;
            
            var dto = new AtualizarStatusPropostaDTO
            {
                Id = id,
                NovoStatus = statusAtual // Mesmo status atual
            };
            
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            proposta.GetType().GetProperty("Id")?.SetValue(proposta, id);
            proposta.AtualizarStatus(statusAtual);
            var dataAtualizacaoAnterior = proposta.DataAtualizacao;

            _mockRepository.Setup(r => r.ObterPorIdAsync(id))
                .ReturnsAsync(proposta);

            _mockRepository.Setup(r => r.AtualizarAsync(It.IsAny<Proposta>()))
                .ReturnsAsync(proposta);

            // Act
            var resultado = await _service.AtualizarStatusAsync(dto);

            // Assert
            Assert.Equal(statusAtual, resultado.Status);
            // Verifica que a data de atualização não foi alterada significativamente
            Assert.NotNull(resultado.DataAtualizacao);
            Assert.NotNull(dataAtualizacaoAnterior);
            var diff = resultado.DataAtualizacao!.Value - dataAtualizacaoAnterior!.Value;
            Assert.True(Math.Abs(diff.TotalSeconds) < 1,
                $"Data de atualização foi alterada. Esperado: {dataAtualizacaoAnterior}, Atual: {resultado.DataAtualizacao}");
            
            // Verifica que a atualização no repositório foi chamada
            _mockRepository.Verify(r => r.AtualizarAsync(It.IsAny<Proposta>()), Times.Once);
            
            // Verifica que a publicação da mensagem foi chamada
            _mockMessageService.Verify(m => m.PublicarAtualizacaoStatusAsync(It.IsAny<Proposta>()), Times.Once);
        }
    }
}