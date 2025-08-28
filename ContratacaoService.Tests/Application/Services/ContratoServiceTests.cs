using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.Services;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Repositories;

namespace ContratacaoService.Tests.Application.Services
{
    public class ContratoServiceTests
    {
        private readonly Mock<IContratoRepository> _mockRepository;
        private readonly ContratoService _service;

        public ContratoServiceTests()
        {
            _mockRepository = new Mock<IContratoRepository>();
            _service = new ContratoService(_mockRepository.Object);
        }

        [Fact]
        public async Task CriarContratoAsync_DeveCriarERetornarContrato()
        {
            // Arrange
            var dto = new CriarContratoDTO
            {
                PropostaId = Guid.NewGuid(),
                Nome = "João Silva",
                CPF = "12345678900",
                ValorSeguro = 1000.50m,
                DuracaoMeses = 12
            };

            _mockRepository.Setup(r => r.AdicionarAsync(It.IsAny<Contrato>()))
                .ReturnsAsync((Contrato c) => c);

            // Act
            var result = await _service.CriarContratoAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.PropostaId, result.PropostaId);
            Assert.Equal(dto.Nome, result.Nome);
            Assert.Equal(dto.CPF, result.CPF);
            Assert.Equal(dto.ValorSeguro, result.ValorSeguro);
            Assert.True(result.Ativo);
            _mockRepository.Verify(r => r.AdicionarAsync(It.IsAny<Contrato>()), Times.Once);
        }

        [Fact]
        public async Task CriarContratoAsync_QuandoDadosInvalidos_DeveLancarExcecao()
        {
            // Arrange
            var dto = new CriarContratoDTO
            {
                PropostaId = Guid.NewGuid(),
                Nome = "", // Nome inválido (vazio)
                CPF = "12345678900",
                ValorSeguro = 1000.50m,
                DuracaoMeses = 12
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarContratoAsync(dto));
            _mockRepository.Verify(r => r.AdicionarAsync(It.IsAny<Contrato>()), Times.Never);
        }

        [Theory]
        [InlineData("12345", "CPF inválido")] // CPF muito curto
        [InlineData("", "CPF não pode ser vazio")]
        [InlineData(null, "CPF não pode ser vazio")]
        public async Task CriarContratoAsync_QuandoCPFInvalido_DeveLancarExcecao(string cpf, string mensagemEsperada)
        {
            // Arrange
            var dto = new CriarContratoDTO
            {
                PropostaId = Guid.NewGuid(),
                Nome = "João Silva",
                CPF = cpf,
                ValorSeguro = 1000.50m,
                DuracaoMeses = 12
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarContratoAsync(dto));
            Assert.Contains(mensagemEsperada, exception.Message, StringComparison.OrdinalIgnoreCase);
            _mockRepository.Verify(r => r.AdicionarAsync(It.IsAny<Contrato>()), Times.Never);
        }

        [Theory]
        [InlineData(0)]  // Valor zero
        [InlineData(-1)] // Valor negativo
        public async Task CriarContratoAsync_QuandoValorSeguroInvalido_DeveLancarExcecao(decimal valorSeguro)
        {
            // Arrange
            var dto = new CriarContratoDTO
            {
                PropostaId = Guid.NewGuid(),
                Nome = "João Silva",
                CPF = "12345678900",
                ValorSeguro = valorSeguro,
                DuracaoMeses = 12
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarContratoAsync(dto));
            _mockRepository.Verify(r => r.AdicionarAsync(It.IsAny<Contrato>()), Times.Never);
        }

        [Fact]
        public async Task ObterPorIdAsync_QuandoContratoExiste_DeveRetornarContrato()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contrato = new Contrato(
                Guid.NewGuid(),
                "João Silva",
                "12345678900",
                1000.50m,
                12);

            _mockRepository.Setup(r => r.ObterPorIdAsync(id))
                .ReturnsAsync(contrato);

            // Act
            var result = await _service.ObterPorIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(contrato.Id, result.Id);
            Assert.Equal(contrato.Nome, result.Nome);
            _mockRepository.Verify(r => r.ObterPorIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task ObterPorIdAsync_QuandoContratoNaoExiste_DeveLancarKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepository.Setup(r => r.ObterPorIdAsync(id))
                .ReturnsAsync((Contrato)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ObterPorIdAsync(id));
            _mockRepository.Verify(r => r.ObterPorIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task ObterPorPropostaIdAsync_QuandoContratoExiste_DeveRetornarContrato()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var contrato = new Contrato(
                propostaId,
                "João Silva",
                "12345678900",
                1000.50m,
                12);

            _mockRepository.Setup(r => r.ObterPorPropostaIdAsync(propostaId))
                .ReturnsAsync(contrato);

            // Act
            var result = await _service.ObterPorPropostaIdAsync(propostaId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(contrato.Id, result.Id);
            Assert.Equal(propostaId, result.PropostaId);
            _mockRepository.Verify(r => r.ObterPorPropostaIdAsync(propostaId), Times.Once);
        }

        [Fact]
        public async Task ObterPorPropostaIdAsync_QuandoContratoNaoExiste_DeveLancarKeyNotFoundException()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            _mockRepository.Setup(r => r.ObterPorPropostaIdAsync(propostaId))
                .ReturnsAsync((Contrato)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ObterPorPropostaIdAsync(propostaId));
            _mockRepository.Verify(r => r.ObterPorPropostaIdAsync(propostaId), Times.Once);
        }

        [Fact]
        public async Task ListarTodosAsync_DeveRetornarTodosContratos()
        {
            // Arrange
            var contratos = new List<Contrato>
            {
                new Contrato(Guid.NewGuid(), "João", "11111111111", 1000m, 12),
                new Contrato(Guid.NewGuid(), "Maria", "22222222222", 2000m, 24)
            };

            _mockRepository.Setup(r => r.ListarTodosAsync())
                .ReturnsAsync(contratos);

            // Act
            var result = await _service.ListarTodosAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.Equal(contratos[0].Id, resultList[0].Id);
            Assert.Equal(contratos[1].Id, resultList[1].Id);
            _mockRepository.Verify(r => r.ListarTodosAsync(), Times.Once);
        }

        [Fact]
        public async Task ListarAtivosPorCpfAsync_DeveRetornarContratosAtivosDoCpf()
        {
            // Arrange
            var cpf = "12345678900";
            var contratos = new List<Contrato>
            {
                new Contrato(Guid.NewGuid(), "João", cpf, 1000m, 12)
            };

            _mockRepository.Setup(r => r.ListarAtivosPorCpfAsync(cpf))
                .ReturnsAsync(contratos);

            // Act
            var result = await _service.ListarAtivosPorCpfAsync(cpf);

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal(cpf, resultList[0].CPF);
            _mockRepository.Verify(r => r.ListarAtivosPorCpfAsync(cpf), Times.Once);
        }

        [Fact]
        public async Task CancelarContratoAsync_QuandoContratoExiste_DeveCancelarERetornarContrato()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contrato = new Contrato(
                Guid.NewGuid(),
                "João Silva",
                "12345678900",
                1000.50m,
                12);

            _mockRepository.Setup(r => r.ObterPorIdAsync(id))
                .ReturnsAsync(contrato);

            _mockRepository.Setup(r => r.AtualizarAsync(It.IsAny<Contrato>()))
                .ReturnsAsync((Contrato c) => c);

            // Act
            var result = await _service.CancelarContratoAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Ativo);
            _mockRepository.Verify(r => r.ObterPorIdAsync(id), Times.Once);
            _mockRepository.Verify(r => r.AtualizarAsync(It.IsAny<Contrato>()), Times.Once);
        }

        [Fact]
        public async Task CancelarContratoAsync_QuandoContratoNaoExiste_DeveLancarKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepository.Setup(r => r.ObterPorIdAsync(id))
                .ReturnsAsync((Contrato)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CancelarContratoAsync(id));
            _mockRepository.Verify(r => r.ObterPorIdAsync(id), Times.Once);
            _mockRepository.Verify(r => r.AtualizarAsync(It.IsAny<Contrato>()), Times.Never);
        }

        [Fact]
        public async Task CancelarContratoAsync_QuandoContratoJaEstaCancelado_DeveLancarInvalidOperationException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contrato = new Contrato(
                Guid.NewGuid(),
                "João Silva",
                "12345678900",
                1000.50m,
                12);
            contrato.Cancelar(); // Já está cancelado

            _mockRepository.Setup(r => r.ObterPorIdAsync(id))
                .ReturnsAsync(contrato);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelarContratoAsync(id));
            _mockRepository.Verify(r => r.ObterPorIdAsync(id), Times.Once);
            _mockRepository.Verify(r => r.AtualizarAsync(It.IsAny<Contrato>()), Times.Never);
        }

        [Fact]
        public async Task CriarContratoAsync_QuandoErroNoRepositorio_DevePropagaExcecao()
        {
            // Arrange
            var dto = new CriarContratoDTO
            {
                PropostaId = Guid.NewGuid(),
                Nome = "João Silva",
                CPF = "12345678900",
                ValorSeguro = 1000.50m,
                DuracaoMeses = 12
            };

            _mockRepository.Setup(r => r.AdicionarAsync(It.IsAny<Contrato>()))
                .ThrowsAsync(new Exception("Erro de banco de dados"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.CriarContratoAsync(dto));
            Assert.Equal("Erro de banco de dados", exception.Message);
        }

        [Fact]
        public async Task ListarTodosAsync_QuandoErroNoRepositorio_DevePropagaExcecao()
        {
            // Arrange
            _mockRepository.Setup(r => r.ListarTodosAsync())
                .ThrowsAsync(new Exception("Erro ao listar contratos"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.ListarTodosAsync());
            Assert.Equal("Erro ao listar contratos", exception.Message);
        }

        [Fact]
        public async Task ListarAtivosPorCpfAsync_QuandoCPFInvalido_DeveLancarArgumentException()
        {
            // Arrange
            string cpfInvalido = "123";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ListarAtivosPorCpfAsync(cpfInvalido));
            _mockRepository.Verify(r => r.ListarAtivosPorCpfAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ListarAtivosPorCpfAsync_QuandoNenhumContratoEncontrado_DeveRetornarListaVazia()
        {
            // Arrange
            var cpf = "12345678900";
            _mockRepository.Setup(r => r.ListarAtivosPorCpfAsync(cpf))
                .ReturnsAsync(new List<Contrato>());

            // Act
            var result = await _service.ListarAtivosPorCpfAsync(cpf);

            // Assert
            Assert.Empty(result);
            _mockRepository.Verify(r => r.ListarAtivosPorCpfAsync(cpf), Times.Once);
        }

        [Fact]
        public async Task CriarContratoAsync_QuandoDuracaoMesesInvalida_DeveLancarExcecao()
        {
            // Arrange
            var dto = new CriarContratoDTO
            {
                PropostaId = Guid.NewGuid(),
                Nome = "João Silva",
                CPF = "12345678900",
                ValorSeguro = 1000.50m,
                DuracaoMeses = 0 // Duração inválida
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarContratoAsync(dto));
            _mockRepository.Verify(r => r.AdicionarAsync(It.IsAny<Contrato>()), Times.Never);
        }

        [Fact]
        public async Task CriarContratoAsync_QuandoPropostaIdInvalido_DeveLancarExcecao()
        {
            // Arrange
            var dto = new CriarContratoDTO
            {
                PropostaId = Guid.Empty, // ID inválido
                Nome = "João Silva",
                CPF = "12345678900",
                ValorSeguro = 1000.50m,
                DuracaoMeses = 12
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarContratoAsync(dto));
            _mockRepository.Verify(r => r.AdicionarAsync(It.IsAny<Contrato>()), Times.Never);
        }

        [Fact]
        public async Task ObterPorIdAsync_QuandoIdInvalido_DeveLancarArgumentException()
        {
            // Arrange
            var id = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ObterPorIdAsync(id));
            _mockRepository.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ObterPorPropostaIdAsync_QuandoIdInvalido_DeveLancarArgumentException()
        {
            // Arrange
            var propostaId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ObterPorPropostaIdAsync(propostaId));
            _mockRepository.Verify(r => r.ObterPorPropostaIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CancelarContratoAsync_QuandoIdInvalido_DeveLancarArgumentException()
        {
            // Arrange
            var id = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CancelarContratoAsync(id));
            _mockRepository.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}