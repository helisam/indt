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
            // Validações de parâmetros
            if (propostaId == Guid.Empty)
            {
                throw new ArgumentException("ID da proposta não pode ser vazio", nameof(propostaId));
            }

            if (string.IsNullOrEmpty(nome))
            {
                throw new ArgumentException("Nome não pode ser vazio", nameof(nome));
            }

            if (string.IsNullOrEmpty(cpf) || cpf.Length < 11)
            {
                throw new ArgumentException("CPF não pode ser vazio ou inválido", nameof(cpf));
            }

            if (valorSeguro <= 0)
            {
                throw new ArgumentException("Valor do seguro deve ser maior que zero", nameof(valorSeguro));
            }

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