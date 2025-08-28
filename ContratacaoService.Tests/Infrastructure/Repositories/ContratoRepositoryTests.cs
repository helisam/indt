using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Moq;
using Xunit;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Repositories;
using ContratacaoService.Infrastructure.Data;
using ContratacaoService.Infrastructure.Repositories;

namespace ContratacaoService.Tests.Infrastructure.Repositories
{
    public class ContratoRepositoryTests
    {
        private readonly Mock<IContratoRepository> _mockRepository;
        private readonly DbContextOptions<ContratacaoDbContext> _dbContextOptions;

        public ContratoRepositoryTests()
        {
            _mockRepository = new Mock<IContratoRepository>();
            
            // Configuração para testes com banco de dados em memória
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            
            _dbContextOptions = new DbContextOptionsBuilder<ContratacaoDbContext>()
                .UseSqlite(connection)
                .Options;
            
            // Cria o esquema do banco de dados
            using var context = new ContratacaoDbContext(_dbContextOptions);
            context.Database.EnsureCreated();
        }

        [Fact]
        public async Task ObterPorIdAsync_QuandoContratoExiste_DeveRetornarContrato()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contrato = new Contrato(Guid.NewGuid(), "João Silva", "12345678900", 1000.50m, 12);
            
            _mockRepository.Setup(r => r.ObterPorIdAsync(id))
                .ReturnsAsync(contrato);

            // Act
            var result = await _mockRepository.Object.ObterPorIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(contrato.Id, result.Id);
            _mockRepository.Verify(r => r.ObterPorIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task ObterPorIdAsync_QuandoContratoNaoExiste_DeveRetornarNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            
            _mockRepository.Setup(r => r.ObterPorIdAsync(id))
                .ReturnsAsync((Contrato)null);

            // Act
            var result = await _mockRepository.Object.ObterPorIdAsync(id);

            // Assert
            Assert.Null(result);
            _mockRepository.Verify(r => r.ObterPorIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task ObterPorPropostaIdAsync_QuandoContratoExiste_DeveRetornarContrato()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var contrato = new Contrato(propostaId, "João Silva", "12345678900", 1000.50m, 12);
            
            _mockRepository.Setup(r => r.ObterPorPropostaIdAsync(propostaId))
                .ReturnsAsync(contrato);

            // Act
            var result = await _mockRepository.Object.ObterPorPropostaIdAsync(propostaId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(propostaId, result.PropostaId);
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
            var result = await _mockRepository.Object.ListarTodosAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
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
            var result = await _mockRepository.Object.ListarAtivosPorCpfAsync(cpf);

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal(cpf, resultList[0].CPF);
            _mockRepository.Verify(r => r.ListarAtivosPorCpfAsync(cpf), Times.Once);
        }

        [Fact]
        public async Task AdicionarAsync_DeveAdicionarERetornarContrato()
        {
            // Arrange
            var contrato = new Contrato(Guid.NewGuid(), "João Silva", "12345678900", 1000.50m, 12);
            
            _mockRepository.Setup(r => r.AdicionarAsync(contrato))
                .ReturnsAsync(contrato);

            // Act
            var result = await _mockRepository.Object.AdicionarAsync(contrato);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(contrato.Id, result.Id);
            _mockRepository.Verify(r => r.AdicionarAsync(contrato), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_DeveAtualizarERetornarContrato()
        {
            // Arrange
            var contrato = new Contrato(Guid.NewGuid(), "João Silva", "12345678900", 1000.50m, 12);
            contrato.Cancelar(); // Modificar o contrato
            
            _mockRepository.Setup(r => r.AtualizarAsync(contrato))
                .ReturnsAsync(contrato);

            // Act
            var result = await _mockRepository.Object.AtualizarAsync(contrato);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Ativo);
            _mockRepository.Verify(r => r.AtualizarAsync(contrato), Times.Once);
        }

        [Fact]
        public async Task AdicionarAsync_QuandoContratoComMesmoCPFJaExiste_DeveAdicionarCorretamente()
        {
            // Arrange
            using var context = new ContratacaoDbContext(_dbContextOptions);
            var repository = new ContratoRepository(context);
            
            var cpf = "12345678900";
            var contrato1 = new Contrato(Guid.NewGuid(), "João Silva", cpf, 1000.50m, 12);
            var contrato2 = new Contrato(Guid.NewGuid(), "João Silva", cpf, 2000.00m, 24);
            
            // Act
            await repository.AdicionarAsync(contrato1);
            await repository.AdicionarAsync(contrato2);
            
            // Assert
            var contratos = await repository.ListarAtivosPorCpfAsync(cpf);
            Assert.Equal(2, contratos.Count());
        }

        [Fact]
        public async Task ListarAtivosPorCpfAsync_QuandoExistemContratosAtivosEInativos_DeveRetornarApenasAtivos()
        {
            // Arrange
            using var context = new ContratacaoDbContext(_dbContextOptions);
            var repository = new ContratoRepository(context);
            
            var cpf = "12345678900";
            var contratoAtivo = new Contrato(Guid.NewGuid(), "João Silva", cpf, 1000.50m, 12);
            var contratoInativo = new Contrato(Guid.NewGuid(), "João Silva", cpf, 2000.00m, 24);
            contratoInativo.Cancelar();
            
            await repository.AdicionarAsync(contratoAtivo);
            await repository.AdicionarAsync(contratoInativo);
            
            // Act
            var result = await repository.ListarAtivosPorCpfAsync(cpf);
            
            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.True(resultList[0].Ativo);
        }

        [Fact]
        public async Task ObterPorPropostaIdAsync_QuandoPropostaIdInvalido_DeveRetornarNull()
        {
            // Arrange
            using var context = new ContratacaoDbContext(_dbContextOptions);
            var repository = new ContratoRepository(context);
            var propostaIdInvalido = Guid.NewGuid();
            
            // Act
            var result = await repository.ObterPorPropostaIdAsync(propostaIdInvalido);
            
            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ListarTodosAsync_QuandoNaoExistemContratos_DeveRetornarListaVazia()
        {
            // Arrange
            using var context = new ContratacaoDbContext(_dbContextOptions);
            var repository = new ContratoRepository(context);
            
            // Act
            var result = await repository.ListarTodosAsync();
            
            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task AtualizarAsync_QuandoContratoNaoExiste_DeveLancarException()
        {
            // Arrange
            _mockRepository.Setup(r => r.AtualizarAsync(It.IsAny<Contrato>()))
                .ThrowsAsync(new KeyNotFoundException("Contrato não encontrado"));
            
            var contrato = new Contrato(Guid.NewGuid(), "João Silva", "12345678900", 1000.50m, 12);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _mockRepository.Object.AtualizarAsync(contrato));
            
            Assert.Equal("Contrato não encontrado", exception.Message);
        }
    }
}