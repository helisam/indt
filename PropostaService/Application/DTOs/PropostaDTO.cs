using System;
using PropostaService.Domain.Enums;

namespace PropostaService.Application.DTOs
{
    public class PropostaDTO
    {
        public Guid Id { get; set; }
        public required string Nome { get; set; }
        public required string CPF { get; set; }
        public decimal ValorSeguro { get; set; }
        public StatusProposta Status { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }

    public class CriarPropostaDTO
    {
        public required string Nome { get; set; }
        public required string CPF { get; set; }
        public decimal ValorSeguro { get; set; }
    }

    public class AtualizarStatusPropostaDTO
    {
        public Guid Id { get; set; }
        public StatusProposta NovoStatus { get; set; }
    }
}