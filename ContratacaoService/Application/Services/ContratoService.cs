using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContratacaoService.Application.DTOs;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Repositories;

namespace ContratacaoService.Application.Services
{
    public class ContratoService : IContratoService
    {
        private readonly IContratoRepository _contratoRepository;

        public ContratoService(IContratoRepository contratoRepository)
        {
            _contratoRepository = contratoRepository;
        }

        public async Task<ContratoDTO> CriarContratoAsync(CriarContratoDTO dto)
        {
            var contrato = new Contrato(dto.PropostaId, dto.Nome, dto.CPF, dto.ValorSeguro, dto.DuracaoMeses);
            await _contratoRepository.AdicionarAsync(contrato);
            return MapToDTO(contrato);
        }

        public async Task<ContratoDTO> ObterPorIdAsync(Guid id)
        {
            var contrato = await _contratoRepository.ObterPorIdAsync(id);
            if (contrato == null)
            {
                throw new KeyNotFoundException($"Contrato com ID {id} não encontrado");
            }

            return MapToDTO(contrato);
        }

        public async Task<ContratoDTO> ObterPorPropostaIdAsync(Guid propostaId)
        {
            var contrato = await _contratoRepository.ObterPorPropostaIdAsync(propostaId);
            if (contrato == null)
            {
                throw new KeyNotFoundException($"Contrato para proposta com ID {propostaId} não encontrado");
            }

            return MapToDTO(contrato);
        }

        public async Task<IEnumerable<ContratoDTO>> ListarTodosAsync()
        {
            var contratos = await _contratoRepository.ListarTodosAsync();
            var dtos = new List<ContratoDTO>();

            foreach (var contrato in contratos)
            {
                dtos.Add(MapToDTO(contrato));
            }

            return dtos;
        }

        public async Task<IEnumerable<ContratoDTO>> ListarAtivosPorCpfAsync(string cpf)
        {
            var contratos = await _contratoRepository.ListarAtivosPorCpfAsync(cpf);
            var dtos = new List<ContratoDTO>();

            foreach (var contrato in contratos)
            {
                dtos.Add(MapToDTO(contrato));
            }

            return dtos;
        }

        public async Task<ContratoDTO> CancelarContratoAsync(Guid id)
        {
            var contrato = await _contratoRepository.ObterPorIdAsync(id);
            if (contrato == null)
            {
                throw new KeyNotFoundException($"Contrato com ID {id} não encontrado");
            }

            contrato.Cancelar();
            await _contratoRepository.AtualizarAsync(contrato);

            return MapToDTO(contrato);
        }

        private ContratoDTO MapToDTO(Contrato contrato)
        {
            return new ContratoDTO
            {
                Id = contrato.Id,
                PropostaId = contrato.PropostaId,
                Nome = contrato.Nome,
                CPF = contrato.CPF,
                ValorSeguro = contrato.ValorSeguro,
                DataInicio = contrato.DataInicio,
                DataFim = contrato.DataFim,
                Ativo = contrato.Ativo,
                DataCriacao = contrato.DataCriacao
            };
        }
    }
}