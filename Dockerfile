FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos do projeto
COPY Estacionamento.Api/*.csproj Estacionamento.Api/
RUN dotnet restore Estacionamento.Api/Estacionamento.Api.csproj

# Copiar todo o código e fazer build
COPY Estacionamento.Api/ Estacionamento.Api/
WORKDIR /src/Estacionamento.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Variável de ambiente para porta (Render define $PORT automaticamente)
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
EXPOSE ${PORT:-8080}

ENTRYPOINT ["dotnet", "Estacionamento.Api.dll"]

