# Documentação de Cenários BDD

Este documento descreve os cenários de Behavior-Driven Development (BDD) criados para o Sistema de Gestão de Propostas de Seguro. Os cenários estão escritos em formato Gherkin e servem como documentação viva dos requisitos e comportamentos esperados do sistema.

## Estrutura dos Cenários

Os cenários BDD estão organizados por funcionalidade e serviço:

### PropostaService

1. **Criação de Propostas** ([`PropostaService.Tests/BDD/CriacaoPropostaScenarios.feature`](./PropostaService.Tests/BDD/CriacaoPropostaScenarios.feature))
   - Cenários que descrevem o processo de criação de novas propostas de seguro
   - Validações de dados do cliente e do seguro
   - Tratamento de casos especiais como clientes existentes

2. **Aprovação e Rejeição de Propostas** ([`PropostaService.Tests/BDD/AprovacaoRejeicaoPropostaScenarios.feature`](./PropostaService.Tests/BDD/AprovacaoRejeicaoPropostaScenarios.feature))
   - Cenários que descrevem o fluxo de análise de propostas
   - Aprovação, rejeição e solicitação de informações adicionais
   - Processamento automático de propostas pendentes

3. **Validações e Tratamento de Erros** ([`PropostaService.Tests/BDD/ValidacoesErrosScenarios.feature`](./PropostaService.Tests/BDD/ValidacoesErrosScenarios.feature))
   - Cenários que descrevem validações específicas de dados
   - Tratamento de erros de sistema e conexão
   - Validações de regras de negócio

### ContratacaoService

1. **Criação de Contratos** ([`ContratacaoService.Tests/BDD/CriacaoContratoScenarios.feature`](./ContratacaoService.Tests/BDD/CriacaoContratoScenarios.feature))
   - Cenários que descrevem a criação automática de contratos a partir de propostas aprovadas
   - Processamento de mensagens da fila
   - Tratamento de erros no processamento de mensagens

2. **Consulta e Cancelamento de Contratos** ([`ContratacaoService.Tests/BDD/ConsultaCancelamentoContratoScenarios.feature`](./ContratacaoService.Tests/BDD/ConsultaCancelamentoContratoScenarios.feature))
   - Cenários que descrevem a consulta de contratos por diferentes critérios
   - Cancelamento de contratos por diferentes motivos
   - Renovação de contratos

3. **Validações e Tratamento de Erros** ([`ContratacaoService.Tests/BDD/ValidacoesErrosContratoScenarios.feature`](./ContratacaoService.Tests/BDD/ValidacoesErrosContratoScenarios.feature))
   - Cenários que descrevem validações específicas de contratos
   - Tratamento de erros de sistema e comunicação
   - Validações de regras de negócio específicas para contratos

## Como Utilizar os Cenários BDD

### Documentação de Requisitos

Os cenários BDD servem como documentação viva dos requisitos do sistema, descrevendo o comportamento esperado em linguagem natural estruturada. Eles podem ser utilizados para:

- Comunicação entre equipes técnicas e de negócio
- Documentação de regras de negócio
- Referência para implementação de funcionalidades

### Base para Testes Automatizados

Os cenários BDD podem ser implementados como testes automatizados utilizando frameworks como:

- SpecFlow para .NET
- Cucumber para outras plataformas

A implementação dos testes automatizados envolve:

1. Criação de step definitions que mapeiam as frases Gherkin para código
2. Implementação da lógica de teste para cada step
3. Configuração do ambiente de teste

### Processo de Desenvolvimento

Para utilizar estes cenários no processo de desenvolvimento:

1. Revisar os cenários com stakeholders antes da implementação
2. Implementar as funcionalidades descritas nos cenários
3. Implementar os testes automatizados baseados nos cenários
4. Executar os testes para validar a implementação
5. Atualizar os cenários conforme o entendimento do domínio evolui

## Convenções Utilizadas

### Estrutura Gherkin

Todos os cenários seguem a estrutura padrão Gherkin:

- **Funcionalidade**: Descreve a funcionalidade sendo testada
- **Contexto**: Estabelece pré-condições comuns a todos os cenários
- **Cenário**: Descreve um caso de teste específico
- **Dado** (Given): Estabelece o estado inicial
- **Quando** (When): Descreve a ação sendo testada
- **Então** (Then): Descreve o resultado esperado

### Tabelas de Dados

Tabelas de dados são utilizadas para fornecer exemplos concretos e múltiplos valores de teste, tornando os cenários mais claros e específicos.

### Linguagem

Os cenários estão escritos em português do Brasil, utilizando termos de domínio consistentes com a documentação do sistema.

## Próximos Passos

Para implementar estes cenários como testes automatizados:

1. Instalar o pacote SpecFlow no projeto de testes
2. Criar as step definitions para cada cenário
3. Implementar os hooks necessários para configuração do ambiente de teste
4. Integrar os testes BDD com o pipeline de CI/CD

---

Esta documentação e os cenários BDD devem evoluir junto com o sistema, sendo atualizados conforme novas funcionalidades são implementadas ou comportamentos existentes são modificados.