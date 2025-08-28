# Script PowerShell para executar testes de API usando Newman

# Verifica se o Newman está instalado
$newmanInstalled = npm list -g newman
if ($newmanInstalled -notmatch "newman") {
    Write-Host "Newman não está instalado. Instalando..." -ForegroundColor Yellow
    npm install -g newman
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Erro ao instalar Newman. Verifique se o Node.js está instalado." -ForegroundColor Red
        exit 1
    }
    Write-Host "Newman instalado com sucesso!" -ForegroundColor Green
}

# Verifica se os serviços estão em execução
Write-Host "Verificando se os serviços estão em execução..." -ForegroundColor Cyan

$propostaServiceUrl = "http://localhost:5145"
$contratacaoServiceUrl = "http://localhost:5270"

$propostaServiceRunning = $false
$contratacaoServiceRunning = $false

try {
    $propostaResponse = Invoke-WebRequest -Uri "$propostaServiceUrl/api/Propostas" -Method GET -TimeoutSec 5 -ErrorAction SilentlyContinue
    if ($propostaResponse.StatusCode -eq 200) {
        $propostaServiceRunning = $true
        Write-Host "Serviço de Proposta está em execução!" -ForegroundColor Green
    }
} catch {
    Write-Host "Serviço de Proposta não está em execução!" -ForegroundColor Red
}

try {
    $contratacaoResponse = Invoke-WebRequest -Uri "$contratacaoServiceUrl/api/Contratos" -Method GET -TimeoutSec 5 -ErrorAction SilentlyContinue
    if ($contratacaoResponse.StatusCode -eq 200) {
        $contratacaoServiceRunning = $true
        Write-Host "Serviço de Contratação está em execução!" -ForegroundColor Green
    }
} catch {
    Write-Host "Serviço de Contratação não está em execução!" -ForegroundColor Red
}

if (-not $propostaServiceRunning -or -not $contratacaoServiceRunning) {
    Write-Host "Pelo menos um dos serviços não está em execução. Deseja continuar mesmo assim? (S/N)" -ForegroundColor Yellow
    $continue = Read-Host
    if ($continue -ne "S" -and $continue -ne "s") {
        Write-Host "Execução cancelada pelo usuário." -ForegroundColor Red
        exit 1
    }
}

# Executa os testes usando Newman
Write-Host "Executando testes com Newman..." -ForegroundColor Cyan

$collectionPath = "$PSScriptRoot\postman_collection.json"
$environmentPath = "$PSScriptRoot\postman_environment.json"

if (-not (Test-Path $collectionPath)) {
    Write-Host "Arquivo de coleção não encontrado: $collectionPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $environmentPath)) {
    Write-Host "Arquivo de ambiente não encontrado: $environmentPath" -ForegroundColor Red
    exit 1
}

# Executa os testes
newman run $collectionPath -e $environmentPath --reporters cli,htmlextra --reporter-htmlextra-export "$PSScriptRoot\newman-report.html"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Testes concluídos com sucesso!" -ForegroundColor Green
    Write-Host "Relatório HTML gerado em: $PSScriptRoot\newman-report.html" -ForegroundColor Cyan
} else {
    Write-Host "Alguns testes falharam. Verifique o relatório para mais detalhes." -ForegroundColor Yellow
    Write-Host "Relatório HTML gerado em: $PSScriptRoot\newman-report.html" -ForegroundColor Cyan
}