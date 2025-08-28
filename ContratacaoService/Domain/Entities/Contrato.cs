using System;

namespace ContratacaoService.Domain.Entities
{
    public class Contrato
    {
        public Guid Id { get; private set; }
        public Guid PropostaId { get; private set; }
        public string Nome { get; private set; }
        public string CPF { get; private set; }
        public decimal ValorSeguro { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        public bool Ativo { get; private set; }
        public DateTime DataCriacao { get; private set; }

        // Construtor para EF Core
        protected Contrato() { }

        public Contrato(Guid propostaId, string nome, string cpf, decimal valorSeguro, int duracaoMeses)
        {
            Id = Guid.NewGuid();
            PropostaId = propostaId;
            Nome = nome;
            CPF = cpf;
            ValorSeguro = valorSeguro;
            DataInicio = DateTime.UtcNow.Date;
            DataFim = DataInicio.AddMonths(duracaoMeses);
            Ativo = true;
            DataCriacao = DateTime.UtcNow;
        }

        public void Cancelar()
        {
            if (!Ativo)
            {
                throw new InvalidOperationException("Contrato já está cancelado");
            }
            
            Ativo = false;
        }
    }
}