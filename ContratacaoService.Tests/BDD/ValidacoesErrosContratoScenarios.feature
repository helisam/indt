# language: pt-BR

Funcionalidade: Validações e Tratamento de Erros em Contratos
  Como um desenvolvedor do sistema
  Eu quero garantir que o serviço de contratação valide corretamente os dados e trate erros adequadamente
  Para manter a integridade e confiabilidade do sistema

Contexto:
  Dado que o sistema de contratação está em execução

Cenário: Validar criação de contrato com proposta inexistente
  Dado que recebo uma mensagem com número de proposta inexistente
    | Número Proposta | CPF Cliente     | Tipo Seguro | Valor     | Vigência  |
    | PROP-9999-999   | 123.456.789-00 | Automóvel   | R$ 1.500,00 | 12 meses  |
  Quando o serviço de contratação tentar processar a mensagem
  Então o sistema deve registrar erro de validação
  E deve registrar a mensagem "Proposta não encontrada"
  E deve mover a mensagem para a fila de erros

Cenário: Validar criação de contrato com proposta já utilizada
  Dado que existe um contrato associado à proposta "PROP-2023-005"
  Quando o serviço receber uma nova mensagem para a mesma proposta
  Então o sistema deve identificar a duplicidade
  E deve registrar a mensagem "Proposta já utilizada em outro contrato"
  E deve descartar a mensagem sem criar um novo contrato

Cenário: Tratar erro de conexão com o banco de dados
  Dado que o banco de dados está indisponível
  Quando o serviço tentar criar um contrato a partir de uma mensagem válida
  Então o sistema deve capturar o erro de conexão
  E deve registrar o erro detalhado no log do sistema
  E deve manter a mensagem na fila para reprocessamento

Cenário: Validar dados inconsistentes na mensagem
  Dado que recebo uma mensagem com dados inconsistentes
    | Número Proposta | CPF Cliente     | Tipo Seguro | Valor     | Vigência  |
    | PROP-2023-006   | 123.456.789-00 | Automóvel   | -R$ 1.500,00 | 12 meses  |
  Quando o serviço de contratação tentar processar a mensagem
  Então o sistema deve registrar erro de validação
  E deve registrar a mensagem "Valor do contrato inválido"
  E deve mover a mensagem para a fila de erros

Cenário: Tratar erro de comunicação com serviço de notificação
  Dado que o serviço de notificação está indisponível
  Quando o serviço criar um contrato com sucesso
  E tentar enviar a notificação ao cliente
  Então o sistema deve capturar o erro de comunicação
  E deve registrar o erro no log do sistema
  E deve agendar uma nova tentativa de notificação
  E o contrato deve ser criado mesmo sem a notificação

Cenário: Validar limite de contratos por cliente
  Dado que um cliente com CPF "111.222.333-44" já possui o número máximo de contratos permitidos
  Quando o serviço tentar criar um novo contrato para este cliente
  Então o sistema deve rejeitar a criação do contrato
  E deve registrar a mensagem "Limite de contratos por cliente excedido"
  E deve notificar a equipe comercial sobre a situação