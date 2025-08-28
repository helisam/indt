using System;
using Xunit;
using ContratacaoService.Domain.Entities;

namespace ContratacaoService.Tests.Domain.Entities
{
    public class ContratoTests
    {
        [Fact]
        public void Construtor_DeveCriarContratoComPropriedadesCorretas()
        {
            // Arrange
            var propostaId = Guid.NewGuid();
            var nome = "João Silva";
            var cpf = "12345678900";
            var valorSeguro = 1000.50m;
            var duracaoMeses = 12;
            
            // Act
            var contrato = new Contrato(propostaId, nome, cpf, valorSeguro, duracaoMeses);
            
            // Assert
            Assert.Equal(propostaId, contrato.PropostaId);
            Assert.Equal(nome, contrato.Nome);
            Assert.Equal(cpf, contrato.CPF);
            Assert.Equal(valorSeguro, contrato.ValorSeguro);
            Assert.True(contrato.Ativo);
            Assert.Equal(DateTime.UtcNow.Date, contrato.DataInicio);
            Assert.Equal(contrato.DataInicio.AddMonths(duracaoMeses), contrato.DataFim);
            Assert.NotEqual(Guid.Empty, contrato.Id);
            Assert.True((DateTime.UtcNow - contrato.DataCriacao).TotalMinutes < 1);
        }
        
        [Fact]
        public void Cancelar_DeveDesativarContrato()
        {
            // Arrange
            var contrato = new Contrato(
                Guid.NewGuid(),
                "João Silva",
                "12345678900",
                1000.50m,
                12);
            
            // Act
            contrato.Cancelar();
            
            // Assert
            Assert.False(contrato.Ativo);
        }
    }
}