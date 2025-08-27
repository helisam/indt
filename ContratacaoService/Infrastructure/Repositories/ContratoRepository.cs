using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Repositories;
using ContratacaoService.Infrastructure.Data;

namespace ContratacaoService.Infrastructure.Repositories
{
    public class ContratoRepository : IContratoRepository
    {
        private readonly ContratacaoDbContext _context;

        public ContratoRepository(ContratacaoDbContext context)
        {
            _context = context;
        }

        public async Task<Contrato> ObterPorIdAsync(Guid id)
        {
            return await _context.Contratos.FindAsync(id);
        }

        public async Task<Contrato> ObterPorPropostaIdAsync(Guid propostaId)
        {
            return await _context.Contratos
                .FirstOrDefaultAsync(c => c.PropostaId == propostaId);
        }

        public async Task<IEnumerable<Contrato>> ListarTodosAsync()
        {
            return await _context.Contratos.ToListAsync();
        }

        public async Task<IEnumerable<Contrato>> ListarAtivosPorCpfAsync(string cpf)
        {
            return await _context.Contratos
                .Where(c => c.CPF == cpf && c.Ativo)
                .ToListAsync();
        }

        public async Task<Contrato> AdicionarAsync(Contrato contrato)
        {
            await _context.Contratos.AddAsync(contrato);
            await _context.SaveChangesAsync();
            return contrato;
        }

        public async Task<Contrato> AtualizarAsync(Contrato contrato)
        {
            _context.Contratos.Update(contrato);
            await _context.SaveChangesAsync();
            return contrato;
        }
    }
}