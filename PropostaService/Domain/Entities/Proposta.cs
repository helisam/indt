using System;
using PropostaService.Domain.Enums;

namespace PropostaService.Domain.Entities
{
    public class Proposta
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public string CPF { get; private set; }
        public decimal ValorSeguro { get; private set; }
        public StatusProposta Status { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataAtualizacao { get; private set; }

        // Construtor para EF Core
        protected Proposta() { }

        public Proposta(string nome, string cpf, decimal valorSeguro)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome não pode ser nulo ou vazio", nameof(nome));
                
            if (string.IsNullOrWhiteSpace(cpf))
                throw new ArgumentException("CPF não pode ser nulo ou vazio", nameof(cpf));
            
            // Validação básica de formato de CPF (deve ter entre 11 e 14 caracteres)
            // Permite CPF com ou sem pontuação (123.456.789-00 ou 12345678900)
            if (cpf.Replace(".", "").Replace("-", "").Length != 11)
                throw new ArgumentException("CPF inválido. Deve conter 11 dígitos", nameof(cpf));
                
            if (valorSeguro <= 0)
                throw new ArgumentException("Valor do seguro deve ser maior que zero", nameof(valorSeguro));
                
            Id = Guid.NewGuid();
            Nome = nome;
            CPF = cpf;
            ValorSeguro = valorSeguro;
            Status = StatusProposta.EmAnalise;
            DataCriacao = DateTime.UtcNow;
            DataAtualizacao = DateTime.UtcNow;
        }

        public void AtualizarStatus(StatusProposta novoStatus)
        {
            if (Status == StatusProposta.Rejeitada)
            {
                throw new InvalidOperationException("Uma proposta rejeitada não pode ter seu status alterado");
            }
            
            if (Status == StatusProposta.Aprovada && novoStatus == StatusProposta.EmAnalise)
            {
                throw new InvalidOperationException("Uma proposta aprovada não pode voltar para o status Em Análise");
            }
            
            // Só atualiza a data se o status realmente mudar
            if (Status != novoStatus)
            {
                Status = novoStatus;
                DataAtualizacao = DateTime.UtcNow;
            }
        }
    }
}