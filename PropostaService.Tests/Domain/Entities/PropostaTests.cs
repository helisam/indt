using System;
using System.Collections.Generic;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using Xunit;
using System.ComponentModel.DataAnnotations;

namespace PropostaService.Tests.Domain.Entities
{
    public class PropostaTests
    {
        [Fact]
        public void Construtor_DeveCriarPropostaComValoresCorretos()
        {
            // Arrange
            string nome = "João Silva";
            string cpf = "123.456.789-00";
            decimal valorSeguro = 1000.00m;

            // Act
            var proposta = new Proposta(nome, cpf, valorSeguro);

            // Assert
            Assert.Equal(nome, proposta.Nome);
            Assert.Equal(cpf, proposta.CPF);
            Assert.Equal(valorSeguro, proposta.ValorSeguro);
            Assert.Equal(StatusProposta.EmAnalise, proposta.Status);
            Assert.NotEqual(Guid.Empty, proposta.Id);
            Assert.True(proposta.DataCriacao > DateTime.MinValue);
            Assert.NotNull(proposta.DataAtualizacao);
            // Verifica se a data de atualização foi definida
            Assert.NotNull(proposta.DataAtualizacao);
        }

        [Fact]
        public void AtualizarStatus_DeveAtualizarStatusEDataAtualizacao()
        {
            // Arrange
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            var novoStatus = StatusProposta.Aprovada;

            // Act
            proposta.AtualizarStatus(novoStatus);

            // Assert
            Assert.Equal(novoStatus, proposta.Status);
            Assert.NotNull(proposta.DataAtualizacao);
        }

        [Fact]
        public void AtualizarStatus_DeveManterMesmoStatus_QuandoStatusForIgual()
        {
            // Arrange
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            var statusAtual = proposta.Status;
            var dataAtualizacaoAnterior = proposta.DataAtualizacao;

            // Act
            proposta.AtualizarStatus(statusAtual);

            // Assert
            Assert.Equal(statusAtual, proposta.Status);
            Assert.Equal(dataAtualizacaoAnterior, proposta.DataAtualizacao);
        }

        [Fact]
        public void AtualizarStatus_DeveAtualizarDataAtualizacao_QuandoStatusForDiferente()
        {
            // Arrange
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            var dataAtualizacaoAnterior = proposta.DataAtualizacao;

            // Act
            proposta.AtualizarStatus(StatusProposta.Aprovada);

            // Assert
            Assert.NotEqual(dataAtualizacaoAnterior, proposta.DataAtualizacao);
        }

        [Theory]
        [InlineData(StatusProposta.Aprovada)]
        [InlineData(StatusProposta.Rejeitada)]
        public void AtualizarStatus_DeveAtualizarParaStatusEspecifico(StatusProposta novoStatus)
        {
            // Arrange
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            Assert.Equal(StatusProposta.EmAnalise, proposta.Status); // Confirma status inicial

            // Act
            proposta.AtualizarStatus(novoStatus);

            // Assert
            Assert.Equal(novoStatus, proposta.Status);
        }

        [Fact]
        public void Construtor_DeveCriarPropostaComDataCriacaoCorreta()
        {
            // Arrange
            var antes = DateTime.UtcNow.AddSeconds(-1);
            
            // Act
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            var depois = DateTime.UtcNow.AddSeconds(1);
            
            // Assert
            Assert.True(proposta.DataCriacao >= antes);
            Assert.True(proposta.DataCriacao <= depois);
        }

        [Fact]
        public void Construtor_DeveCriarPropostaComIdUnico()
        {
            // Arrange & Act
            var proposta1 = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            var proposta2 = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            
            // Assert
            Assert.NotEqual(proposta1.Id, proposta2.Id);
        }

        [Theory]
        [InlineData(0.01)]
        [InlineData(9999.99)]
        public void Construtor_DeveCriarPropostaComValoresSeguroValidos(decimal valorSeguro)
        {
            // Arrange & Act
            var proposta = new Proposta("João Silva", "123.456.789-00", valorSeguro);
            
            // Assert
            Assert.Equal(valorSeguro, proposta.ValorSeguro);
        }
        
        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Construtor_DeveLancarExcecao_QuandoValorSeguroNegativo(decimal valorSeguro)
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new Proposta("João Silva", "123.456.789-00", valorSeguro));
                
            Assert.Contains("valor do seguro", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Construtor_DeveLancarExcecao_QuandoNomeInvalido(string? nome)
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new Proposta(nome, "123.456.789-00", 1000.00m));
                
            Assert.Contains("nome", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("123")]
        [InlineData("123456789012345")]
        public void Construtor_DeveLancarExcecao_QuandoCPFInvalido(string? cpf)
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new Proposta("João Silva", cpf, 1000.00m));
                
            Assert.Contains("CPF", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AtualizarStatus_DeveManterHistoricoDeAtualizacoes()
        {
            // Arrange
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            Assert.Equal(StatusProposta.EmAnalise, proposta.Status);
            Assert.NotNull(proposta.DataAtualizacao);
            var dataCriacao = proposta.DataAtualizacao;
            
            // Act - Primeira atualização
            proposta.AtualizarStatus(StatusProposta.Aprovada);
            var primeiraAtualizacao = proposta.DataAtualizacao;
            
            // Espera um pouco para garantir timestamps diferentes
            System.Threading.Thread.Sleep(5);
            
            // Act - Segunda atualização
            proposta.AtualizarStatus(StatusProposta.Rejeitada);
            var segundaAtualizacao = proposta.DataAtualizacao;
            
            // Assert
            Assert.Equal(StatusProposta.Rejeitada, proposta.Status);
            Assert.NotNull(primeiraAtualizacao);
            Assert.NotNull(segundaAtualizacao);
            Assert.True(segundaAtualizacao > primeiraAtualizacao);
        }

        [Fact]
        public void Proposta_DeveSerImutavelExcetoPeloStatus()
        {
            // Arrange
            string nome = "João Silva";
            string cpf = "123.456.789-00";
            decimal valorSeguro = 1000.00m;
            
            // Act
            var proposta = new Proposta(nome, cpf, valorSeguro);
            var idOriginal = proposta.Id;
            var dataCriacaoOriginal = proposta.DataCriacao;
            
            // Tenta modificar o status (permitido)
            proposta.AtualizarStatus(StatusProposta.Aprovada);
            
            // Assert - Verifica que apenas o status e a data de atualização mudaram
            Assert.Equal(nome, proposta.Nome);
            Assert.Equal(cpf, proposta.CPF);
            Assert.Equal(valorSeguro, proposta.ValorSeguro);
            Assert.Equal(idOriginal, proposta.Id);
            Assert.Equal(dataCriacaoOriginal, proposta.DataCriacao);
            Assert.Equal(StatusProposta.Aprovada, proposta.Status);
            Assert.NotNull(proposta.DataAtualizacao);
        }
        
        [Fact]
        public void AtualizarStatus_DeveLancarExcecao_QuandoTentarAtualizarPropostaRejeitada()
        {
            // Arrange
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            proposta.AtualizarStatus(StatusProposta.Rejeitada);
            
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                proposta.AtualizarStatus(StatusProposta.Aprovada));
                
            Assert.Contains("rejeitada", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        
        [Fact]
        public void AtualizarStatus_DeveLancarExcecao_QuandoTentarAtualizarPropostaAprovadaParaEmAnalise()
        {
            // Arrange
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            proposta.AtualizarStatus(StatusProposta.Aprovada);
            
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                proposta.AtualizarStatus(StatusProposta.EmAnalise));
                
            Assert.Contains("aprovada", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        
        [Fact]
        public void AtualizarStatus_DevePermitirAtualizarDeAprovadaParaRejeitada()
        {
            // Arrange
            var proposta = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            proposta.AtualizarStatus(StatusProposta.Aprovada);
            
            // Act
            proposta.AtualizarStatus(StatusProposta.Rejeitada);
            
            // Assert
            Assert.Equal(StatusProposta.Rejeitada, proposta.Status);
        }
        
        [Fact]
        public void Proposta_DeveValidarFormatoCPF()
        {
            // Arrange & Act & Assert
            // CPF com formato válido
            var proposta1 = new Proposta("João Silva", "123.456.789-00", 1000.00m);
            Assert.NotNull(proposta1);
            
            // CPF sem formatação mas com comprimento correto
            var proposta2 = new Proposta("João Silva", "12345678900", 1000.00m);
            Assert.NotNull(proposta2);
            
            // CPF com formatação diferente mas válida
            var proposta3 = new Proposta("João Silva", "123456789-00", 1000.00m);
            Assert.NotNull(proposta3);
        }
    }
}