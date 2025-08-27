using System;
using PropostaService.Domain.Enums;

namespace PropostaService.Domain.Entities
{
    public class Proposta
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public string CPF { get; private set; }
        public decimal ValorSeguro { get; private set; }
        public StatusProposta Status { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataAtualizacao { get; private set; }

        // Construtor para EF Core
        protected Proposta() { }

        public Proposta(string nome, string cpf, decimal valorSeguro)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            CPF = cpf;
            ValorSeguro = valorSeguro;
            Status = StatusProposta.EmAnalise;
            DataCriacao = DateTime.UtcNow;
        }

        public void AtualizarStatus(StatusProposta novoStatus)
        {
            Status = novoStatus;
            DataAtualizacao = DateTime.UtcNow;
        }
    }
}