# language: pt-BR

Funcionalidade: Aprovação e Rejeição de Propostas de Seguro
  Como um analista de seguros
  Eu quero analisar e aprovar/rejeitar propostas de seguro
  Para que apenas propostas viáveis sejam convertidas em contratos

Contexto:
  Dado que estou autenticado no sistema como analista de seguros
  E tenho acesso ao módulo de análise de propostas

Cenário: Aprovar uma proposta de seguro
  Dado que existe uma proposta com o número "PROP-2023-001" com status "Em Análise"
  E a proposta atende a todos os critérios de análise de risco
  Quando eu aprovar a proposta
  Então o status da proposta deve ser alterado para "Aprovada"
  E o sistema deve registrar a data e hora da aprovação
  E o sistema deve enviar uma mensagem para a fila de processamento de contratos

Cenário: Rejeitar uma proposta de seguro por alto risco
  Dado que existe uma proposta com o número "PROP-2023-002" com status "Em Análise"
  E a proposta apresenta alto risco
    | Motivo                   | Detalhes                                  |
    | Perfil de Alto Risco     | Histórico de sinistros frequentes nos últimos 12 meses |
  Quando eu rejeitar a proposta
  E informar o motivo da rejeição
  Então o status da proposta deve ser alterado para "Rejeitada"
  E o sistema deve registrar o motivo da rejeição
  E o sistema deve notificar o cliente sobre a rejeição

Cenário: Solicitar informações adicionais para uma proposta
  Dado que existe uma proposta com o número "PROP-2023-003" com status "Em Análise"
  E a proposta necessita de informações adicionais para análise
  Quando eu solicitar informações adicionais
    | Informação Solicitada    | Prazo para Resposta |
    | Histórico Médico         | 5 dias úteis         |
  Então o status da proposta deve ser alterado para "Pendente de Informações"
  E o sistema deve registrar quais informações foram solicitadas
  E o sistema deve notificar o cliente sobre a solicitação

Cenário: Aprovar uma proposta após recebimento de informações adicionais
  Dado que existe uma proposta com o número "PROP-2023-004" com status "Pendente de Informações"
  E o cliente forneceu todas as informações médicas solicitadas
  Quando eu analisar as informações adicionais
  E aprovar a proposta
  Então o status da proposta deve ser alterado para "Aprovada"
  E o sistema deve registrar a data e hora da aprovação
  E o sistema deve enviar uma mensagem para a fila de processamento de contratos

Cenário: Rejeitar automaticamente uma proposta por falta de resposta
  Dado que existe uma proposta com o número "PROP-2023-005" com status "Pendente de Informações"
  E o prazo para resposta expirou há mais de 5 dias
  Quando o sistema executar a verificação automática de prazos
  Então o status da proposta deve ser alterado para "Rejeitada"
  E o sistema deve registrar "Falta de resposta no prazo" como motivo da rejeição
  E o sistema deve notificar o cliente sobre a rejeição