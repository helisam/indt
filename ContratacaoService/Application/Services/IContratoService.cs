using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContratacaoService.Application.DTOs;

namespace ContratacaoService.Application.Services
{
    public interface IContratoService
    {
        Task<ContratoDTO> CriarContratoAsync(CriarContratoDTO dto);
        Task<ContratoDTO> ObterPorIdAsync(Guid id);
        Task<ContratoDTO> ObterPorPropostaIdAsync(Guid propostaId);
        Task<IEnumerable<ContratoDTO>> ListarTodosAsync();
        Task<IEnumerable<ContratoDTO>> ListarAtivosPorCpfAsync(string cpf);
        Task<ContratoDTO> CancelarContratoAsync(Guid id);
    }
}