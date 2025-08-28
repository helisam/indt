using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Repositories;
using PropostaService.Infrastructure.Data;

namespace PropostaService.Infrastructure.Repositories
{
    public class PropostaRepository : IPropostaRepository
    {
        private readonly PropostaDbContext _context;

        public PropostaRepository(PropostaDbContext context)
        {
            _context = context;
        }

        public async Task<Proposta?> ObterPorIdAsync(Guid id)
        {
            return await _context.Propostas.FindAsync(id);
        }

        public async Task<IEnumerable<Proposta>> ListarTodasAsync()
        {
            return await _context.Propostas
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
        }

        public async Task<Proposta> AdicionarAsync(Proposta proposta)
        {
            if (proposta.ValorSeguro <= 0)
            {
                throw new ArgumentException("O valor do seguro deve ser maior que zero.", nameof(proposta));
            }
            
            if (string.IsNullOrWhiteSpace(proposta.Nome))
            {
                throw new ArgumentException("O nome não pode ser vazio.", nameof(proposta));
            }
            
            if (string.IsNullOrWhiteSpace(proposta.CPF))
            {
                throw new ArgumentException("O CPF não pode ser nulo ou vazio.", nameof(proposta));
            }
            
            await _context.Propostas.AddAsync(proposta);
            await _context.SaveChangesAsync();
            return proposta;
        }

        public async Task<Proposta> AtualizarAsync(Proposta proposta)
        {
            // Busca a entidade existente para evitar problemas com o SQLite
            var propostaExistente = await _context.Propostas.FindAsync(proposta.Id);
            if (propostaExistente == null)
            {
                throw new KeyNotFoundException($"Proposta com ID {proposta.Id} não encontrada.");
            }
            
            // Atualiza o status usando o método da entidade
            propostaExistente.AtualizarStatus(proposta.Status);
            
            await _context.SaveChangesAsync();
            return propostaExistente;
        }

        public async Task<IEnumerable<Proposta>> ListarPorStatusAsync(StatusProposta status)
        {
            return await _context.Propostas
                .Where(p => p.Status == status)
                .ToListAsync();
        }
    }
}