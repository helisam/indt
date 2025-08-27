using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;

namespace PropostaService.Domain.Repositories
{
    public interface IPropostaRepository
    {
        Task<Proposta> ObterPorIdAsync(Guid id);
        Task<IEnumerable<Proposta>> ListarTodasAsync();
        Task<Proposta> AdicionarAsync(Proposta proposta);
        Task<Proposta> AtualizarAsync(Proposta proposta);
        Task<IEnumerable<Proposta>> ListarPorStatusAsync(StatusProposta status);
    }
}