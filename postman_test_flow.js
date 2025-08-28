// Script para testar o fluxo completo de proposta e contratação
// Para usar: Cole este script na aba "Tests" de uma requisição no Postman e execute

// Configuração
const propostaServiceUrl = pm.environment.get("propostaServiceUrl") || "http://localhost:5145";
const contratacaoServiceUrl = pm.environment.get("contratacaoServiceUrl") || "http://localhost:5270";
const cpf = pm.environment.get("cpf") || "123.456.789-00";

// Função para criar uma proposta
function criarProposta() {
    console.log("Etapa 1: Criando proposta...");
    
    pm.sendRequest({
        url: `${propostaServiceUrl}/api/Propostas`,
        method: 'POST',
        header: {
            'Content-Type': 'application/json'
        },
        body: {
            mode: 'raw',
            raw: JSON.stringify({
                nome: "Cliente Teste",
                cpf: cpf,
                valorSeguro: 1500.00
            })
        }
    }, function (err, res) {
        if (err) {
            console.error("Erro ao criar proposta:", err);
            return;
        }
        
        pm.test("Proposta criada com sucesso", function () {
            pm.expect(res.code).to.be.oneOf([200, 201]);
            pm.expect(res.json()).to.have.property('id');
        });
        
        const propostaId = res.json().id;
        pm.environment.set("propostaId", propostaId);
        console.log(`Proposta criada com ID: ${propostaId}`);
        
        // Próxima etapa: aprovar a proposta
        aprovarProposta(propostaId);
    });
}

// Função para aprovar uma proposta
function aprovarProposta(propostaId) {
    console.log("Etapa 2: Aprovando proposta...");
    
    pm.sendRequest({
        url: `${propostaServiceUrl}/api/Propostas/status`,
        method: 'PUT',
        header: {
            'Content-Type': 'application/json'
        },
        body: {
            mode: 'raw',
            raw: JSON.stringify({
                id: propostaId,
                novoStatus: 1 // Aprovada
            })
        }
    }, function (err, res) {
        if (err) {
            console.error("Erro ao aprovar proposta:", err);
            return;
        }
        
        pm.test("Proposta aprovada com sucesso", function () {
            pm.expect(res.code).to.equal(200);
            pm.expect(res.json().status).to.equal(1); // Status Aprovada
        });
        
        console.log(`Proposta ${propostaId} aprovada com sucesso`);
        
        // Próxima etapa: criar contrato
        criarContrato(propostaId);
    });
}

// Função para criar um contrato
function criarContrato(propostaId) {
    console.log("Etapa 3: Criando contrato...");
    
    pm.sendRequest({
        url: `${contratacaoServiceUrl}/api/Contratos`,
        method: 'POST',
        header: {
            'Content-Type': 'application/json'
        },
        body: {
            mode: 'raw',
            raw: JSON.stringify({
                propostaId: propostaId,
                nome: "Cliente Teste",
                cpf: cpf,
                valorSeguro: 1500.00,
                duracaoMeses: 12
            })
        }
    }, function (err, res) {
        if (err) {
            console.error("Erro ao criar contrato:", err);
            return;
        }
        
        pm.test("Contrato criado com sucesso", function () {
            pm.expect(res.code).to.be.oneOf([200, 201]);
            pm.expect(res.json()).to.have.property('id');
        });
        
        const contratoId = res.json().id;
        pm.environment.set("contratoId", contratoId);
        console.log(`Contrato criado com ID: ${contratoId}`);
        
        // Verificar contrato criado
        verificarContrato(contratoId);
    });
}

// Função para verificar um contrato
function verificarContrato(contratoId) {
    console.log("Etapa 4: Verificando contrato...");
    
    pm.sendRequest({
        url: `${contratacaoServiceUrl}/api/Contratos/${contratoId}`,
        method: 'GET'
    }, function (err, res) {
        if (err) {
            console.error("Erro ao verificar contrato:", err);
            return;
        }
        
        pm.test("Contrato verificado com sucesso", function () {
            pm.expect(res.code).to.equal(200);
            pm.expect(res.json()).to.have.property('id').to.equal(contratoId);
            pm.expect(res.json()).to.have.property('ativo').to.equal(true);
        });
        
        console.log(`Contrato ${contratoId} verificado com sucesso`);
        console.log("Fluxo de teste completo executado com sucesso!");
    });
}

// Iniciar o fluxo de teste
criarProposta();