# Estágio de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar os arquivos de projeto e restaurar as dependências
COPY ["SegurosApp.sln", "./"]
COPY ["PropostaService/Domain/PropostaService.Domain.csproj", "PropostaService/Domain/"]
COPY ["PropostaService/Application/PropostaService.Application.csproj", "PropostaService/Application/"]
COPY ["PropostaService/Infrastructure/PropostaService.Infrastructure.csproj", "PropostaService/Infrastructure/"]
COPY ["PropostaService/Api/PropostaService.Api.csproj", "PropostaService/Api/"]
COPY ["ContratacaoService/Domain/ContratacaoService.Domain.csproj", "ContratacaoService/Domain/"]
COPY ["ContratacaoService/Application/ContratacaoService.Application.csproj", "ContratacaoService/Application/"]
COPY ["ContratacaoService/Infrastructure/ContratacaoService.Infrastructure.csproj", "ContratacaoService/Infrastructure/"]
COPY ["ContratacaoService/Api/ContratacaoService.Api.csproj", "ContratacaoService/Api/"]

RUN dotnet restore

# Copiar o restante do código fonte
COPY . .

# Build do PropostaService
RUN dotnet build "PropostaService/Api/PropostaService.Api.csproj" -c Release -o /app/proposta/build

# Build do ContratacaoService
RUN dotnet build "ContratacaoService/Api/ContratacaoService.Api.csproj" -c Release -o /app/contratacao/build

# Estágio de publicação do PropostaService
FROM build AS publish-proposta
RUN dotnet publish "PropostaService/Api/PropostaService.Api.csproj" -c Release -o /app/proposta/publish

# Estágio de publicação do ContratacaoService
FROM build AS publish-contratacao
RUN dotnet publish "ContratacaoService/Api/ContratacaoService.Api.csproj" -c Release -o /app/contratacao/publish

# Estágio final para PropostaService
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS proposta
WORKDIR /app
COPY --from=publish-proposta /app/proposta/publish .

# Instalar ferramentas EF Core para migrações
RUN apt-get update && apt-get install -y curl
RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0 --install-dir /usr/share/dotnet
ENV PATH="$PATH:/usr/share/dotnet"

ENTRYPOINT ["dotnet", "PropostaService.Api.dll"]

# Estágio final para ContratacaoService
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS contratacao
WORKDIR /app
COPY --from=publish-contratacao /app/contratacao/publish .

# Instalar ferramentas EF Core para migrações
RUN apt-get update && apt-get install -y curl
RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0 --install-dir /usr/share/dotnet
ENV PATH="$PATH:/usr/share/dotnet"

ENTRYPOINT ["dotnet", "ContratacaoService.Api.dll"]