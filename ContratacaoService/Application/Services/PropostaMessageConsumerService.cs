using System;
using System.Threading.Tasks;
using ContratacaoService.Application.DTOs;
using ContratacaoService.Domain.Services;

namespace ContratacaoService.Application.Services
{
    public class PropostaMessageConsumerService : IPropostaMessageConsumerService
    {
        private readonly IContratoService _contratoService;

        public PropostaMessageConsumerService(IContratoService contratoService)
        {
            _contratoService = contratoService;
        }

        public async Task ProcessarMensagemPropostaAsync(Guid propostaId, string status, string nome, string cpf, decimal valorSeguro)
        {
            // Apenas criar contrato se a proposta foi aprovada
            if (status.Equals("Aprovada", StringComparison.OrdinalIgnoreCase))
            {
                var dto = new CriarContratoDTO
                {
                    PropostaId = propostaId,
                    Nome = nome,
                    CPF = cpf,
                    ValorSeguro = valorSeguro,
                    DuracaoMeses = 12 // Duração padrão de 12 meses
                };

                await _contratoService.CriarContratoAsync(dto);
            }
        }
    }
}