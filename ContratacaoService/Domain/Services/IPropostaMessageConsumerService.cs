using System;
using System.Threading.Tasks;

namespace ContratacaoService.Domain.Services
{
    public interface IPropostaMessageConsumerService
    {
        Task ProcessarMensagemPropostaAsync(Guid propostaId, string status, string nome, string cpf, decimal valorSeguro);
    }
}