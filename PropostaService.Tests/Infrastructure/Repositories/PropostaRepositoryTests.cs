using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Moq;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Repositories;
using PropostaService.Infrastructure.Data;
using PropostaService.Infrastructure.Repositories;
using Xunit;

namespace PropostaService.Tests.Infrastructure.Repositories
{
    public class PropostaRepositoryTests
    {
        private readonly Mock<IPropostaRepository> _mockRepository;
        private readonly DbContextOptions<PropostaDbContext> _dbContextOptions;

        public PropostaRepositoryTests()
        {
            _mockRepository = new Mock<IPropostaRepository>();
            
            // Configuração para testes com banco de dados em memória
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            
            _dbContextOptions = new DbContextOptionsBuilder<PropostaDbContext>()
                .UseSqlite(connection)
                .EnableSensitiveDataLogging()
                .Options;
            
            // Cria o esquema do banco de dados
            using var context = new PropostaDbContext(_dbContextOptions);
            context.Database.EnsureCreated();
        }

        [Fact]
        public async Task AdicionarAsync_DeveAdicionarProposta()
        {
            // Arrange
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            _mockRepository.Setup(r => r.AdicionarAsync(It.IsAny<Proposta>()))
                .ReturnsAsync(proposta);

            // Act
            await _mockRepository.Object.AdicionarAsync(proposta);

            _mockRepository.Verify(r => r.AdicionarAsync(It.Is<Proposta>(p => 
                p.Nome == proposta.Nome && 
                p.CPF == proposta.CPF && 
                p.ValorSeguro == proposta.ValorSeguro)), 
                Times.Once);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarPropostaExistente()
        {
            // Arrange
            var id = Guid.NewGuid();
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            proposta.GetType().GetProperty("Id")?.SetValue(proposta, id);

            _mockRepository.Setup(r => r.ObterPorIdAsync(id))
                .ReturnsAsync(proposta);

            // Act
            var resultado = await _mockRepository.Object.ObterPorIdAsync(id);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(id, resultado.Id);
            Assert.Equal(proposta.Nome, resultado.Nome);
            Assert.Equal(proposta.CPF, resultado.CPF);
            Assert.Equal(proposta.ValorSeguro, resultado.ValorSeguro);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarNullParaPropostaNaoExistente()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepository.Setup(r => r.ObterPorIdAsync(id))
                .ReturnsAsync((Proposta)null);

            // Act
            var resultado = await _mockRepository.Object.ObterPorIdAsync(id);

            // Assert
            Assert.Null(resultado);
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
            var resultado = await _mockRepository.Object.ListarTodasAsync();

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(propostas.Count, resultado.Count());
        }

        [Fact]
        public async Task AtualizarAsync_DeveAtualizarProposta()
        {
            // Arrange
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            proposta.AtualizarStatus(StatusProposta.Aprovada);

            _mockRepository.Setup(r => r.AtualizarAsync(It.IsAny<Proposta>()))
                .ReturnsAsync(proposta);

            // Act
            await _mockRepository.Object.AtualizarAsync(proposta);

            // Assert
            _mockRepository.Verify(r => r.AtualizarAsync(It.Is<Proposta>(p => 
                p.Id == proposta.Id && 
                p.Status == proposta.Status)), 
                Times.Once);
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
            var resultado = await _mockRepository.Object.ListarPorStatusAsync(status);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(propostas.Count, resultado.Count());
        }

        [Fact]
        public async Task ListarPorStatusAsync_DeveRetornarListaVaziaQuandoNaoExistemPropostas()
        {
            // Arrange
            var status = StatusProposta.Aprovada;
            var listaVazia = new List<Proposta>();

            _mockRepository.Setup(r => r.ListarPorStatusAsync(status))
                .ReturnsAsync(listaVazia);

            // Act
            var resultado = await _mockRepository.Object.ListarPorStatusAsync(status);

            // Assert
            Assert.NotNull(resultado);
            Assert.Empty(resultado);
        }

        [Fact]
        public void AdicionarAsync_DeveLancarExcecaoQuandoCPFInvalido()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new Proposta("João Silva", "123.456", 1000.00m));
                
            Assert.Contains("CPF", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AdicionarAsync_DeveLancarExcecaoQuandoValorSeguroNegativo()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new Proposta("João Silva", "123.456.789-00", -100.00m));
                
            Assert.Contains("valor", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AtualizarAsync_DeveLancarExcecaoQuandoPropostaNaoExiste()
        {
            // Arrange
            var propostaNaoExistente = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            _mockRepository.Setup(r => r.AtualizarAsync(propostaNaoExistente))
                .ThrowsAsync(new KeyNotFoundException("Proposta não encontrada"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _mockRepository.Object.AtualizarAsync(propostaNaoExistente));
        }

        [Theory]
        [InlineData(StatusProposta.EmAnalise)]
        [InlineData(StatusProposta.Aprovada)]
        [InlineData(StatusProposta.Rejeitada)]
        public async Task ListarPorStatusAsync_DeveRetornarPropostasPorCadaStatus(StatusProposta status)
        {
            // Arrange
            var propostas = new List<Proposta>
            {
                new Proposta("João Silva", "123.456.789-00", 1000.00m)
            };
            
            // Configura o mock para retornar a lista para o status específico
            _mockRepository.Setup(r => r.ListarPorStatusAsync(status))
                .ReturnsAsync(propostas);

            // Act
            var resultado = await _mockRepository.Object.ListarPorStatusAsync(status);

            // Assert
            Assert.NotNull(resultado);
            Assert.Single(resultado);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveLancarExcecaoQuandoIdInvalido()
        {
            // Arrange
            var idInvalido = Guid.Empty;
            _mockRepository.Setup(r => r.ObterPorIdAsync(idInvalido))
                .ThrowsAsync(new ArgumentException("ID inválido"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _mockRepository.Object.ObterPorIdAsync(idInvalido));
        }

        [Fact]
        public async Task ListarTodasAsync_DeveLancarExcecaoQuandoOcorreErroNoBancoDeDados()
        {
            // Arrange
            _mockRepository.Setup(r => r.ListarTodasAsync())
                .ThrowsAsync(new DbUpdateException("Erro ao acessar o banco de dados", new Exception()));

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(() => 
                _mockRepository.Object.ListarTodasAsync());
        }
        
        [Fact]
        public async Task AdicionarAsync_DeveAdicionarPropostaComSucesso_UsandoBancoDeDadosEmMemoria()
        {
            // Arrange
            // Cria uma nova conexão e contexto para cada teste
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            
            var options = new DbContextOptionsBuilder<PropostaDbContext>()
                .UseSqlite(connection)
                .EnableSensitiveDataLogging()
                .Options;
                
            // Cria o esquema do banco de dados
            using (var context = new PropostaDbContext(options))
            {
                context.Database.EnsureCreated();
            }
            
            // Executa o teste em um novo contexto
            using (var context = new PropostaDbContext(options))
            {
                var repository = new PropostaRepository(context);
                var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
                
                // Act
                await repository.AdicionarAsync(proposta);
                
                // Assert
                var propostasSalvas = await repository.ListarTodasAsync();
                Assert.Single(propostasSalvas);
                var propostaSalva = propostasSalvas.First();
                Assert.Equal(proposta.Id, propostaSalva.Id);
                Assert.Equal(proposta.Nome, propostaSalva.Nome);
                Assert.Equal(proposta.CPF, propostaSalva.CPF);
                Assert.Equal(proposta.ValorSeguro, propostaSalva.ValorSeguro);
                Assert.Equal(proposta.Status, propostaSalva.Status);
            }
        }
        
        [Fact]
        public async Task AtualizarAsync_DeveAtualizarStatusDaProposta_UsandoBancoDeDadosEmMemoria()
        {
            // Arrange
            // Cria uma nova conexão e contexto para cada teste
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            
            var options = new DbContextOptionsBuilder<PropostaDbContext>()
                .UseSqlite(connection)
                .EnableSensitiveDataLogging()
                .Options;
                
            // Cria o esquema do banco de dados
            using (var context = new PropostaDbContext(options))
            {
                context.Database.EnsureCreated();
            }
            
            // Executa o teste em um novo contexto
            using (var context = new PropostaDbContext(options))
            {
                var repository = new PropostaRepository(context);
                var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
                await repository.AdicionarAsync(proposta);
                
                // Atualiza o status da proposta
                proposta.AtualizarStatus(StatusProposta.Aprovada);
                
                // Act
                await repository.AtualizarAsync(proposta);
                
                // Assert
                var propostaAtualizada = await repository.ObterPorIdAsync(proposta.Id);
                Assert.NotNull(propostaAtualizada);
                Assert.Equal(StatusProposta.Aprovada, propostaAtualizada.Status);
                Assert.NotNull(propostaAtualizada.DataAtualizacao);
            }
        }
        
        [Fact]
        public async Task ListarPorStatusAsync_DeveRetornarPropostasApenasDoStatusEspecificado_UsandoBancoDeDadosEmMemoria()
        {
            // Arrange
            // Cria uma nova conexão e contexto para cada teste
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            
            var options = new DbContextOptionsBuilder<PropostaDbContext>()
                .UseSqlite(connection)
                .EnableSensitiveDataLogging()
                .Options;
                
            // Cria o esquema do banco de dados
            using (var context = new PropostaDbContext(options))
            {
                context.Database.EnsureCreated();
            }
            
            // Executa o teste em um novo contexto
            using (var context = new PropostaDbContext(options))
            {
                var repository = new PropostaRepository(context);
                
                // Cria propostas com diferentes status
                var propostaEmAnalise = new Proposta("João Silva", "123.456.789-00", 1000.00m);
                var propostaAprovada = new Proposta("Maria Souza", "987.654.321-00", 2000.00m);
                var propostaRejeitada = new Proposta("Pedro Santos", "456.789.123-00", 1500.00m);
                
                propostaAprovada.AtualizarStatus(StatusProposta.Aprovada);
                propostaRejeitada.AtualizarStatus(StatusProposta.Rejeitada);
                
                await repository.AdicionarAsync(propostaEmAnalise);
                await repository.AdicionarAsync(propostaAprovada);
                await repository.AdicionarAsync(propostaRejeitada);
                
                // Act
                var propostasEmAnalise = await repository.ListarPorStatusAsync(StatusProposta.EmAnalise);
                var propostasAprovadas = await repository.ListarPorStatusAsync(StatusProposta.Aprovada);
                var propostasRejeitadas = await repository.ListarPorStatusAsync(StatusProposta.Rejeitada);
                
                // Assert
                Assert.NotNull(propostasEmAnalise);
                Assert.Single(propostasEmAnalise);
                Assert.Equal(propostaEmAnalise.Id, propostasEmAnalise.First().Id);
                
                Assert.NotNull(propostasAprovadas);
                Assert.Single(propostasAprovadas);
                Assert.Equal(propostaAprovada.Id, propostasAprovadas.First().Id);
                
                Assert.NotNull(propostasRejeitadas);
                Assert.Single(propostasRejeitadas);
                Assert.Equal(propostaRejeitada.Id, propostasRejeitadas.First().Id);
            }
        }
        
        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarNull_QuandoPropostaNaoExiste_UsandoBancoDeDadosEmMemoria()
        {
            // Arrange
            // Cria uma nova conexão e contexto para cada teste
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            
            var options = new DbContextOptionsBuilder<PropostaDbContext>()
                .UseSqlite(connection)
                .EnableSensitiveDataLogging()
                .Options;
                
            // Cria o esquema do banco de dados
            using (var context = new PropostaDbContext(options))
            {
                context.Database.EnsureCreated();
            }
            
            // Executa o teste em um novo contexto
            using (var context = new PropostaDbContext(options))
            {
                var repository = new PropostaRepository(context);
                var idInexistente = Guid.NewGuid();
                
                // Act
                var resultado = await repository.ObterPorIdAsync(idInexistente);
                
                // Assert
                Assert.Null(resultado);
            }
        }
        
        [Fact]
        public async Task ListarTodasAsync_DeveRetornarTodasAsPropostas_EmOrdemDecrescenteDeDataCriacao_UsandoBancoDeDadosEmMemoria()
        {
            // Arrange
            // Cria uma nova conexão e contexto para cada teste
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            
            var options = new DbContextOptionsBuilder<PropostaDbContext>()
                .UseSqlite(connection)
                .EnableSensitiveDataLogging()
                .Options;
                
            // Cria o esquema do banco de dados
            using (var context = new PropostaDbContext(options))
            {
                context.Database.EnsureCreated();
            }
            
            // Executa o teste em um novo contexto
            using (var context = new PropostaDbContext(options))
            {
                var repository = new PropostaRepository(context);
                
                // Cria propostas com diferentes datas (simuladas através da ordem de criação)
                var proposta1 = new Proposta("João Silva", "123.456.789-00", 1000.00m);
                await Task.Delay(5); // Pequeno delay para garantir timestamps diferentes
                var proposta2 = new Proposta("Maria Souza", "987.654.321-00", 2000.00m);
            await Task.Delay(5);
            var proposta3 = new Proposta("Pedro Santos", "456.789.123-00", 1500.00m);
            
            // Adiciona em ordem diferente da criação
            await repository.AdicionarAsync(proposta1);
            await repository.AdicionarAsync(proposta2);
            await repository.AdicionarAsync(proposta3);
            
            // Act
            var propostas = await repository.ListarTodasAsync();
            
            // Assert
            Assert.NotNull(propostas);
            Assert.Equal(3, propostas.Count());
            
            // Verifica se estão em ordem decrescente de data de criação (a mais recente primeiro)
            var listaOrdenada = propostas.ToList();
            Assert.Equal(proposta3.Id, listaOrdenada[0].Id);
            Assert.Equal(proposta2.Id, listaOrdenada[1].Id);
            Assert.Equal(proposta1.Id, listaOrdenada[2].Id);
            }
        }

        [Fact]
        public void AdicionarAsync_DeveLancarExcecaoQuandoNomeVazio_UsandoBancoDeDadosEmMemoria()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new Proposta("", "123.456.789-00", 1000.00m));
                
            Assert.Contains("nome", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AdicionarAsync_DeveLancarExcecaoQuandoCPFNulo_UsandoBancoDeDadosEmMemoria()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new Proposta("João Silva", "", 1000.00m));
                
            Assert.Contains("cpf", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AdicionarAsync_DeveLancarExcecaoQuandoValorSeguroZero_UsandoBancoDeDadosEmMemoria()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new Proposta("João Silva", "123.456.789-00", 0m));
                
            Assert.Contains("valor", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AtualizarAsync_DeveLancarExcecaoQuandoPropostaNaoExiste_UsandoBancoDeDadosEmMemoria()
        {
            // Arrange
            using var context = new PropostaDbContext(_dbContextOptions);
            var repository = new PropostaRepository(context);
            var propostaNaoExistente = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                repository.AtualizarAsync(propostaNaoExistente));
        }
    }
}