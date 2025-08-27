using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContratacaoService.Domain.Entities;

namespace ContratacaoService.Domain.Repositories
{
    public interface IContratoRepository
    {
        Task<Contrato> ObterPorIdAsync(Guid id);
        Task<Contrato> ObterPorPropostaIdAsync(Guid propostaId);
        Task<IEnumerable<Contrato>> ListarTodosAsync();
        Task<IEnumerable<Contrato>> ListarAtivosPorCpfAsync(string cpf);
        Task<Contrato> AdicionarAsync(Contrato contrato);
        Task<Contrato> AtualizarAsync(Contrato contrato);
    }
}