# language: pt-BR

Funcionalidade: Criação de Contratos a partir de Propostas de Seguro Aprovadas
  Como um sistema de contratação
  Eu quero criar contratos automaticamente a partir de propostas de seguro aprovadas
  Para que o cliente possa ter seu seguro ativado rapidamente

Contexto:
  Dado que o serviço de contratação está em execução
  E está conectado à fila de mensagens

Cenário: Criar contrato automaticamente a partir de uma proposta de seguro aprovada
  Dado que existe uma mensagem na fila com os dados de uma proposta aprovada
    | Número Proposta | CPF Cliente        | Tipo Seguro        | Valor        | Cobertura  |
    | PROP-2023-001   | 123.456.789-00    | Seguro de Vida     | R$ 150.000,00 | Completa   |
  Quando o serviço de contratação processar a mensagem
  Então um novo contrato deve ser criado com sucesso
  E o contrato deve ter status "Ativo"
  E o contrato deve estar vinculado à proposta original
  E o sistema deve gerar um número de contrato único

Cenário: Criar contrato com dados de pagamento da proposta
  Dado que existe uma mensagem na fila com os dados de uma proposta aprovada
  E a proposta contém informações de pagamento
    | Forma Pagamento         | Parcelas | Vencimento |
    | Transferência Bancária  | 3        | Dia 15     |
  Quando o serviço de contratação processar a mensagem
  Então um novo contrato deve ser criado com sucesso
  E o contrato deve incluir as informações de pagamento da proposta
  E o sistema deve gerar as parcelas conforme configuração da proposta

Cenário: Criar contrato para cliente com outros contratos ativos
  Dado que existe uma mensagem na fila com os dados de uma proposta aprovada
  E o cliente já possui outros contratos ativos no sistema
  Quando o serviço de contratação processar a mensagem
  Então um novo contrato deve ser criado com sucesso
  E o contrato deve ser associado ao perfil existente do cliente
  E o sistema deve atualizar o histórico de contratos do cliente

Cenário: Falha ao criar contrato por dados incompletos na mensagem
  Dado que existe uma mensagem na fila com dados incompletos de uma proposta
    | Número Proposta  | CPF Cliente        | Tipo Seguro    | Valor     |
    | PROP-2023-002    | 987.654.321-00    | Seguro Auto    |           |
  Quando o serviço de contratação tentar processar a mensagem
  Então o sistema deve registrar um erro de processamento
  E a mensagem deve ser movida para a fila de mensagens com erro
  E o sistema deve notificar a equipe de suporte

Cenário: Reprocessamento de mensagem com erro
  Dado que existe uma mensagem na fila de erros
  E os dados da proposta foram corrigidos
  Quando a mensagem for reprocessada
  Então um novo contrato deve ser criado com sucesso
  E o sistema deve registrar que a mensagem foi reprocessada com sucesso