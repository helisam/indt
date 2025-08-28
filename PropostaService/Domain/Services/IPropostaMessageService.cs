using System.Threading.Tasks;
using PropostaService.Domain.Entities;

namespace PropostaService.Domain.Services
{
    public interface IPropostaMessageService
    {
        Task PublicarAtualizacaoStatusAsync(Proposta proposta);
    }
}