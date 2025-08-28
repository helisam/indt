using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PropostaService.Application.DTOs;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Repositories;
using PropostaService.Domain.Services;

namespace PropostaService.Application.Services
{
    public class PropostaService : IPropostaService
    {
        private readonly IPropostaRepository _propostaRepository;
        private readonly IPropostaMessageService _messageService;

        public PropostaService(IPropostaRepository propostaRepository, IPropostaMessageService messageService)
        {
            _propostaRepository = propostaRepository;
            _messageService = messageService;
        }

        public async Task<PropostaDTO> CriarPropostaAsync(CriarPropostaDTO dto)
        {
            var proposta = new Proposta(dto.Nome, dto.CPF, dto.ValorSeguro);
            await _propostaRepository.AdicionarAsync(proposta);
            
            // Publicar mensagem de atualização de status
            await _messageService.PublicarAtualizacaoStatusAsync(proposta);
            
            return MapToDTO(proposta);
        }

        public async Task<PropostaDTO> AtualizarStatusAsync(AtualizarStatusPropostaDTO dto)
        {
            var proposta = await _propostaRepository.ObterPorIdAsync(dto.Id);
            if (proposta == null)
            {
                throw new KeyNotFoundException($"Proposta com ID {dto.Id} não encontrada");
            }

            proposta.AtualizarStatus(dto.NovoStatus);
            await _propostaRepository.AtualizarAsync(proposta);
            
            // Publicar mensagem de atualização de status
            await _messageService.PublicarAtualizacaoStatusAsync(proposta);
            
            return MapToDTO(proposta);
        }

        public async Task<PropostaDTO> ObterPorIdAsync(Guid id)
        {
            var proposta = await _propostaRepository.ObterPorIdAsync(id);
            if (proposta == null)
            {
                throw new KeyNotFoundException($"Proposta com ID {id} não encontrada");
            }

            return MapToDTO(proposta);
        }

        public async Task<IEnumerable<PropostaDTO>> ListarTodasAsync()
        {
            var propostas = await _propostaRepository.ListarTodasAsync();
            var dtos = new List<PropostaDTO>();

            foreach (var proposta in propostas)
            {
                dtos.Add(MapToDTO(proposta));
            }

            return dtos;
        }

        public async Task<IEnumerable<PropostaDTO>> ListarPorStatusAsync(StatusProposta status)
        {
            var propostas = await _propostaRepository.ListarPorStatusAsync(status);
            var dtos = new List<PropostaDTO>();

            foreach (var proposta in propostas)
            {
                dtos.Add(MapToDTO(proposta));
            }

            return dtos;
        }

        private PropostaDTO MapToDTO(Proposta proposta)
        {
            return new PropostaDTO
            {
                Id = proposta.Id,
                Nome = proposta.Nome,
                CPF = proposta.CPF,
                ValorSeguro = proposta.ValorSeguro,
                Status = proposta.Status,
                DataCriacao = proposta.DataCriacao,
                DataAtualizacao = proposta.DataAtualizacao
            };
        }
    }
}