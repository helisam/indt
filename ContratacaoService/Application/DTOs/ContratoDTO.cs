using System;

namespace ContratacaoService.Application.DTOs
{
    public class ContratoDTO
    {
        public Guid Id { get; set; }
        public Guid PropostaId { get; set; }
        public string Nome { get; set; }
        public string CPF { get; set; }
        public decimal ValorSeguro { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
    }

    public class CriarContratoDTO
    {
        public Guid PropostaId { get; set; }
        public string Nome { get; set; }
        public string CPF { get; set; }
        public decimal ValorSeguro { get; set; }
        public int DuracaoMeses { get; set; }
    }
}