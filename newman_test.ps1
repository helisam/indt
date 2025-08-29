# Script PowerShell para executar testes de API usando Newman

# Parâmetros
param (
    [switch]$Docker,
    [switch]$Help
)

# Exibe ajuda
if ($Help) {
    Write-Host "Uso: .\newman_test.ps1 [-Docker] [-Help]" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Opções:" -ForegroundColor Cyan
    Write-Host "  -Docker    Executa os testes usando as URLs do ambiente Docker (portas 5001/5002)" -ForegroundColor Cyan
    Write-Host "  -Help      Exibe esta mensagem de ajuda" -ForegroundColor Cyan
    exit 0
}

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

# Define as URLs dos serviços com base no parâmetro Docker
if ($Docker) {
    $propostaServiceUrl = "http://localhost:5001"
    $contratacaoServiceUrl = "http://localhost:5002"
    Write-Host "Usando URLs do ambiente Docker (portas 5001/5002)" -ForegroundColor Yellow
} else {
    $propostaServiceUrl = "http://localhost:5145"
    $contratacaoServiceUrl = "http://localhost:5270"
    Write-Host "Usando URLs do ambiente local (portas 5145/5270)" -ForegroundColor Yellow
}

$propostaServiceRunning = $false
$contratacaoServiceRunning = $false

try {
    $propostaResponse = Invoke-WebRequest -Uri "$propostaServiceUrl/api/Propostas" -Method GET -TimeoutSec 5 -ErrorAction SilentlyContinue
    if ($propostaResponse.StatusCode -eq 200) {
        $propostaServiceRunning = $true
        Write-Host "Serviço de Proposta está em execução em $propostaServiceUrl!" -ForegroundColor Green
    }
} catch {
    Write-Host "Serviço de Proposta não está em execução em $propostaServiceUrl!" -ForegroundColor Red
}

try {
    $contratacaoResponse = Invoke-WebRequest -Uri "$contratacaoServiceUrl/api/Contratos" -Method GET -TimeoutSec 5 -ErrorAction SilentlyContinue
    if ($contratacaoResponse.StatusCode -eq 200) {
        $contratacaoServiceRunning = $true
        Write-Host "Serviço de Contratação está em execução em $contratacaoServiceUrl!" -ForegroundColor Green
    }
} catch {
    Write-Host "Serviço de Contratação não está em execução em $contratacaoServiceUrl!" -ForegroundColor Red
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

# Define o arquivo de ambiente com base no parâmetro Docker
if ($Docker) {
    $environmentPath = "$PSScriptRoot\postman_environment_docker.json"
    $reportSuffix = "-docker"
} else {
    $environmentPath = "$PSScriptRoot\postman_environment.json"
    $reportSuffix = ""
}

if (-not (Test-Path $collectionPath)) {
    Write-Host "Arquivo de coleção não encontrado: $collectionPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $environmentPath)) {
    Write-Host "Arquivo de ambiente não encontrado: $environmentPath" -ForegroundColor Red
    exit 1
}

# Executa os testes
Write-Host "Usando arquivo de ambiente: $environmentPath" -ForegroundColor Cyan
newman run $collectionPath -e $environmentPath --reporters cli,htmlextra --reporter-htmlextra-export "$PSScriptRoot\newman-report$reportSuffix.html"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Testes concluídos com sucesso!" -ForegroundColor Green
    Write-Host "Relatório HTML gerado em: $PSScriptRoot\newman-report$reportSuffix.html" -ForegroundColor Cyan
} else {
    Write-Host "Alguns testes falharam. Verifique o relatório para mais detalhes." -ForegroundColor Yellow
    Write-Host "Relatório HTML gerado em: $PSScriptRoot\newman-report$reportSuffix.html" -ForegroundColor Cyan
}