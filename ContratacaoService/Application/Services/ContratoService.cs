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
            if (dto.PropostaId == Guid.Empty)
            {
                throw new ArgumentException("ID da proposta não pode ser vazio", nameof(dto.PropostaId));
            }
            
            if (string.IsNullOrWhiteSpace(dto.Nome))
            {
                throw new ArgumentException("Nome não pode ser vazio", nameof(dto.Nome));
            }
            
            if (string.IsNullOrWhiteSpace(dto.CPF))
            {
                throw new ArgumentException("CPF não pode ser vazio", nameof(dto.CPF));
            }
            
            if (dto.CPF.Length < 11)
            {
                throw new ArgumentException("CPF inválido", nameof(dto.CPF));
            }
            
            if (dto.ValorSeguro <= 0)
            {
                throw new ArgumentException("Valor do seguro deve ser maior que zero", nameof(dto.ValorSeguro));
            }
            
            if (dto.DuracaoMeses <= 0)
            {
                throw new ArgumentException("Duração em meses deve ser maior que zero", nameof(dto.DuracaoMeses));
            }
            
            var contrato = new Contrato(dto.PropostaId, dto.Nome, dto.CPF, dto.ValorSeguro, dto.DuracaoMeses);
            await _contratoRepository.AdicionarAsync(contrato);
            return MapToDTO(contrato);
        }

        public async Task<ContratoDTO> ObterPorIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("ID do contrato não pode ser vazio", nameof(id));
            }
            
            var contrato = await _contratoRepository.ObterPorIdAsync(id);
            if (contrato == null)
            {
                throw new KeyNotFoundException($"Contrato com ID {id} não encontrado");
            }

            return MapToDTO(contrato);
        }

        public async Task<ContratoDTO> ObterPorPropostaIdAsync(Guid propostaId)
        {
            if (propostaId == Guid.Empty)
            {
                throw new ArgumentException("ID da proposta não pode ser vazio", nameof(propostaId));
            }
            
            var contrato = await _contratoRepository.ObterPorPropostaIdAsync(propostaId);
            if (contrato == null)
            {
                throw new KeyNotFoundException($"Contrato com Proposta ID {propostaId} não encontrado");
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
            if (string.IsNullOrWhiteSpace(cpf))
            {
                throw new ArgumentException("CPF não pode ser vazio", nameof(cpf));
            }
            
            if (cpf.Length < 11)
            {
                throw new ArgumentException("CPF inválido. Deve conter 11 dígitos", nameof(cpf));
            }
            
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
            if (id == Guid.Empty)
            {
                throw new ArgumentException("ID do contrato não pode ser vazio", nameof(id));
            }
            
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