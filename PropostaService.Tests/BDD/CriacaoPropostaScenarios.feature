# language: pt-BR

Funcionalidade: Criação de Proposta de Seguro
  Como um corretor de seguros
  Eu quero criar uma nova proposta de seguro
  Para que o cliente possa contratar um seguro

Contexto:
  Dado que estou autenticado no sistema
  E tenho acesso ao módulo de propostas

Cenário: Criar uma proposta de seguro com sucesso
  Dado que tenho os dados do cliente
    | Nome      | CPF           | Email               | Telefone      |
    | João Silva | 123.456.789-00 | joao@email.com.br | (11) 98765-4321 |
  E tenho os dados do seguro
    | Tipo                | Valor        | Cobertura    |
    | Seguro de Vida     | R$ 150.000,00 | Completa     |
  Quando eu submeter a proposta
  Então a proposta deve ser criada com sucesso
  E o sistema deve retornar o número da proposta
  E o status inicial da proposta deve ser "Em Análise"

Cenário: Criar uma proposta de seguro para cliente existente
  Dado que existe um cliente com CPF "987.654.321-00" no sistema
  E tenho os dados do seguro
    | Tipo                | Valor        | Cobertura    |
    | Seguro Residencial  | R$ 80.000,00  | Básica      |
  Quando eu submeter a proposta para este cliente
  Então a proposta deve ser criada com sucesso
  E o sistema deve vincular a proposta ao cliente existente
  E o status inicial da proposta deve ser "Em Análise"

Cenário: Tentar criar uma proposta sem informar dados obrigatórios
  Dado que tenho dados incompletos do cliente
    | Nome      | CPF           | Email               | Telefone      |
    | Maria Santos |              | maria@email.com.br | (11) 91234-5678 |
  E tenho os dados do seguro
    | Tipo      | Valor        | Cobertura    |
    | Seguro Auto | R$ 12.000,00  | Básica    |
  Quando eu tentar submeter a proposta
  Então o sistema deve rejeitar a criação da proposta
  E deve exibir a mensagem "CPF é um campo obrigatório"

Cenário: Criar uma proposta com dados de pagamento
  Dado que tenho os dados completos do cliente
  E tenho os dados do seguro
    | Tipo                | Valor        | Cobertura    |
    | Seguro Viagem       | R$ 5.000,00  | Internacional |
  E tenho os dados de pagamento
    | Forma          | Parcelas    | Vencimento  |
    | Transferência Bancária | 3          | Dia 15      |
  Quando eu submeter a proposta com dados de pagamento
  Então a proposta deve ser criada com sucesso
  E o sistema deve registrar as informações de pagamento
  E o status inicial da proposta deve ser "Em Análise"