using Microsoft.EntityFrameworkCore;
using ContratacaoService.Domain.Entities;

namespace ContratacaoService.Infrastructure.Data
{
    public class ContratacaoDbContext : DbContext
    {
        public ContratacaoDbContext(DbContextOptions<ContratacaoDbContext> options) : base(options)
        {
        }

        public DbSet<Contrato> Contratos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contrato>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PropostaId).IsRequired();
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CPF).IsRequired().HasMaxLength(14);
                entity.Property(e => e.ValorSeguro).IsRequired().HasPrecision(18, 2);
                entity.Property(e => e.DataInicio).IsRequired();
                entity.Property(e => e.DataFim).IsRequired();
                entity.Property(e => e.Ativo).IsRequired();
                entity.Property(e => e.DataCriacao).IsRequired();
            });
        }
    }
}