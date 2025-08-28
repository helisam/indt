using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PropostaService.Application.DTOs;
using PropostaService.Domain.Enums;

namespace PropostaService.Application.Services
{
    public interface IPropostaService
    {
        Task<PropostaDTO> CriarPropostaAsync(CriarPropostaDTO dto);
        Task<PropostaDTO> AtualizarStatusAsync(AtualizarStatusPropostaDTO dto);
        Task<PropostaDTO> ObterPorIdAsync(Guid id);
        Task<IEnumerable<PropostaDTO>> ListarTodasAsync();
        Task<IEnumerable<PropostaDTO>> ListarPorStatusAsync(StatusProposta status);
    }
}