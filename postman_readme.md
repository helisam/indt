# Coleção Postman para APIs de Seguros

Este documento descreve como utilizar a coleção Postman criada para testar as APIs de Proposta e Contratação de Seguros.

## Pré-requisitos

1. [Postman](https://www.postman.com/downloads/) instalado
2. Serviços de API em execução:
   - PropostaService: http://localhost:5145
   - ContratacaoService: http://localhost:5270

## Arquivos Disponíveis

- `postman_collection.json`: Coleção com todos os endpoints das APIs
- `postman_environment.json`: Variáveis de ambiente para a coleção
- `postman_test_flow.js`: Script para testar o fluxo completo automaticamente
- `newman_test.ps1`: Script PowerShell para executar testes via linha de comando

## Importando a Coleção e o Ambiente

1. Abra o Postman
2. Clique em "Import" no canto superior esquerdo
3. Selecione os arquivos `postman_collection.json` e `postman_environment.json`
4. Clique em "Import"
5. Após a importação, selecione o ambiente "Seguros API Environment" no seletor de ambientes no canto superior direito

## Estrutura da Coleção

A coleção está organizada em duas pastas principais:

### 1. Proposta Service

Endpoints para gerenciamento de propostas de seguro:

- **Listar todas as propostas**: `GET /api/Propostas`
- **Obter proposta por ID**: `GET /api/Propostas/{id}`
- **Listar propostas por status**: `GET /api/Propostas/status/{status}`
- **Criar proposta**: `POST /api/Propostas`
- **Atualizar status da proposta**: `PUT /api/Propostas/status`

### 2. Contratacao Service

Endpoints para gerenciamento de contratos de seguro:

- **Listar todos os contratos**: `GET /api/Contratos`
- **Obter contrato por ID**: `GET /api/Contratos/{id}`
- **Obter contrato por ID da proposta**: `GET /api/Contratos/proposta/{propostaId}`
- **Listar contratos ativos por CPF**: `GET /api/Contratos/cpf/{cpf}`
- **Criar contrato**: `POST /api/Contratos`
- **Cancelar contrato**: `PUT /api/Contratos/{id}/cancelar`

## Variáveis de Ambiente

A coleção utiliza as seguintes variáveis de ambiente que já estão configuradas no arquivo `postman_environment.json`:

- `propostaServiceUrl`: URL base do serviço de propostas (padrão: "http://localhost:5145")
- `contratacaoServiceUrl`: URL base do serviço de contratação (padrão: "http://localhost:5270")
- `propostaId`: ID de uma proposta existente (será preenchido automaticamente ao criar uma proposta)
- `contratoId`: ID de um contrato existente (será preenchido automaticamente ao criar um contrato)
- `cpf`: CPF para filtrar contratos (padrão: "123.456.789-00")
- `status`: Status da proposta (0 = EmAnalise, 1 = Aprovada, 2 = Rejeitada)

## Fluxo de Teste Manual

Para testar o fluxo completo do sistema manualmente, siga estes passos:

1. **Criar uma proposta**:
   - Use o endpoint `Criar proposta`
   - O ID retornado será automaticamente salvo na variável `propostaId`

2. **Aprovar a proposta**:
   - Use o endpoint `Atualizar status da proposta` com `novoStatus: 1`

3. **Criar um contrato**:
   - Use o endpoint `Criar contrato` (já está configurado para usar o `propostaId` salvo)
   - O ID retornado será automaticamente salvo na variável `contratoId`

4. **Verificar o contrato**:
   - Use o endpoint `Obter contrato por ID` para verificar os detalhes

5. **Cancelar o contrato** (opcional):
   - Use o endpoint `Cancelar contrato`

## Teste Automatizado de Fluxo Completo

Para executar o teste automatizado do fluxo completo:

1. Crie uma nova requisição no Postman (ou use uma existente)
2. Vá para a aba "Tests"
3. Cole o conteúdo do arquivo `postman_test_flow.js`
4. Clique em "Send"

O script executará automaticamente as seguintes etapas:
1. Criar uma proposta
2. Aprovar a proposta
3. Criar um contrato baseado na proposta
4. Verificar o contrato criado

Os resultados serão exibidos na aba "Test Results" e no console do Postman.

## Testes Automatizados via Newman (Linha de Comando)

Para executar os testes via linha de comando usando Newman:

1. Certifique-se de ter o [Node.js](https://nodejs.org/) instalado
2. Execute o script PowerShell `newman_test.ps1`

O script irá:
1. Verificar se o Newman está instalado (e instalá-lo se necessário)
2. Verificar se os serviços de API estão em execução
3. Executar todos os testes da coleção
4. Gerar um relatório HTML com os resultados

Benefícios do Newman:
- Execução de testes em ambientes de CI/CD
- Geração de relatórios detalhados
- Automação de testes sem a interface gráfica do Postman

## Testes Automáticos

A coleção inclui testes automáticos básicos:

- Verificação de códigos de status de sucesso (200, 201, 202, 204)
- Scripts para salvar automaticamente IDs de propostas e contratos criados

## Dicas

- Você pode visualizar os valores das variáveis de ambiente a qualquer momento clicando no ícone de olho no seletor de ambientes
- Para adicionar mais testes, você pode editar os scripts na aba "Tests" de cada requisição
- Lembre-se de que os serviços precisam estar em execução para que os testes funcionem corretamente

## Solução de Problemas

Se encontrar problemas ao executar as requisições:

1. Verifique se os serviços estão em execução nas portas corretas
2. Certifique-se de que o ambiente está selecionado no Postman
3. Verifique se as variáveis de ambiente estão configuradas corretamente
4. Consulte os logs dos serviços para identificar possíveis erros