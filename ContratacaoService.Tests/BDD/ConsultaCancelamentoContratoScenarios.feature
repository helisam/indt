# language: pt-BR

Funcionalidade: Consulta e Cancelamento de Contratos de Seguro
  Como um operador do sistema
  Eu quero consultar e gerenciar contratos de seguro existentes
  Para atender às necessidades dos clientes

Contexto:
  Dado que estou autenticado no sistema
  E tenho acesso ao módulo de contratos

Cenário: Consultar um contrato pelo número
  Dado que existe um contrato com o número "CONT-2023-001" no sistema
  Quando eu consultar o contrato pelo número
  Então o sistema deve exibir os detalhes completos do contrato
    | Número          | Status | Cliente      | Tipo Seguro        | Valor        | Cobertura  |
    | CONT-2023-001   | Ativo  | João Silva   | Seguro de Vida     | R$ 150.000,00 | Completa   |

Cenário: Consultar contratos por CPF do cliente
  Dado que existem contratos associados ao CPF "123.456.789-00"
  Quando eu consultar contratos por CPF
  Então o sistema deve listar todos os contratos do cliente
  E a lista deve conter apenas contratos do CPF informado

Cenário: Consultar contratos por status
  Dado que existem contratos com diferentes status no sistema
  Quando eu consultar contratos com status "Ativo"
  Então o sistema deve listar apenas os contratos com status "Ativo"

Cenário: Cancelar um contrato a pedido do cliente
  Dado que existe um contrato ativo com o número "CONT-2023-002"
  Quando eu solicitar o cancelamento do contrato
  E informar o motivo "Cancelamento a pedido do cliente"
  Então o status do contrato deve ser alterado para "Cancelado"
  E o sistema deve registrar a data e o motivo do cancelamento
  E o sistema deve calcular o valor de restituição, se aplicável

Cenário: Cancelar um contrato por falta de pagamento
  Dado que existe um contrato ativo com o número "CONT-2023-003"
  E o contrato possui 1 parcela em atraso
  Quando eu solicitar o cancelamento do contrato
  E informar o motivo "Falta de pagamento"
  Então o status do contrato deve ser alterado para "Cancelado"
  E o sistema deve registrar a data e o motivo do cancelamento
  E o sistema deve gerar um registro de cobrança para as parcelas em atraso

Cenário: Renovar um contrato próximo do vencimento
  Dado que existe um contrato ativo com o número "CONT-2023-004"
  E o contrato está a 30 dias do vencimento
  Quando eu solicitar a renovação do contrato
  Então o sistema deve criar uma nova proposta de renovação
  E a proposta deve herdar as características do contrato original
  E o sistema deve notificar o cliente sobre a renovação