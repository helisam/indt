# language: pt-BR

Funcionalidade: Validações e Tratamento de Erros em Propostas de Seguro
  Como um desenvolvedor do sistema
  Eu quero garantir que o sistema valide corretamente os dados e trate erros adequadamente
  Para manter a integridade e confiabilidade do sistema

Contexto:
  Dado que o sistema de propostas está em execução

Cenário: Validar CPF inválido na criação de proposta
  Dado que tenho os dados do cliente com CPF inválido
    | Nome        | CPF              | Email               | Telefone      |
    | João Silva  | 111.222.333-44   | joao@email.com.br   | (11) 97777-8888 |
  Quando eu tentar submeter a proposta
  Então o sistema deve rejeitar a criação da proposta
  E deve exibir a mensagem "CPF inválido"

Cenário: Validar e-mail inválido na criação de proposta
  Dado que tenho os dados do cliente com e-mail inválido
    | Nome        | CPF              | Email               | Telefone      |
    | Maria Santos | 444.555.666-77   | maria@invalido     | (11) 96666-7777 |
  Quando eu tentar submeter a proposta
  Então o sistema deve rejeitar a criação da proposta
  E deve exibir a mensagem "Formato de e-mail inválido"

Cenário: Validar valor mínimo do seguro
  Dado que tenho os dados completos do cliente
  E tenho os dados do seguro com valor abaixo do mínimo permitido
    | Tipo                | Valor        | Cobertura   |
    | Seguro Auto         | R$ 500,00    | Básica      |
  Quando eu tentar submeter a proposta
  Então o sistema deve rejeitar a criação da proposta
  E deve exibir a mensagem "Valor do seguro abaixo do mínimo permitido"

Cenário: Tratar erro de conexão com o banco de dados
  Dado que o banco de dados está indisponível
  Quando eu tentar submeter uma proposta válida
  Então o sistema deve capturar o erro de conexão
  E deve exibir a mensagem "Erro de conexão com o banco de dados. Tente novamente mais tarde."
  E deve registrar o erro detalhado no log do sistema

Cenário: Tratar timeout na comunicação com serviço externo
  Dado que o serviço de análise de risco está lento
  Quando eu submeter uma proposta que requer análise de risco
  E o tempo de resposta exceder o timeout configurado
  Então o sistema deve capturar o erro de timeout
  E deve colocar a proposta em status "Pendente de Análise Manual"
  E deve notificar a equipe técnica sobre a necessidade de verificação manual

Cenário: Validar proposta duplicada
  Dado que existe uma proposta ativa para o cliente com CPF "888.999.000-11"
  E para o mesmo tipo de seguro "Seguro de Vida"
  Quando eu tentar submeter uma nova proposta para o mesmo cliente e tipo de seguro
  Então o sistema deve identificar a duplicidade
  E deve exibir a mensagem "Já existe uma proposta ativa para este cliente e tipo de seguro"