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

        public async Task<Proposta> ObterPorIdAsync(Guid id)
        {
            return await _context.Propostas.FindAsync(id);
        }

        public async Task<IEnumerable<Proposta>> ListarTodasAsync()
        {
            return await _context.Propostas.ToListAsync();
        }

        public async Task<Proposta> AdicionarAsync(Proposta proposta)
        {
            await _context.Propostas.AddAsync(proposta);
            await _context.SaveChangesAsync();
            return proposta;
        }

        public async Task<Proposta> AtualizarAsync(Proposta proposta)
        {
            _context.Propostas.Update(proposta);
            await _context.SaveChangesAsync();
            return proposta;
        }

        public async Task<IEnumerable<Proposta>> ListarPorStatusAsync(StatusProposta status)
        {
            return await _context.Propostas
                .Where(p => p.Status == status)
                .ToListAsync();
        }
    }
}