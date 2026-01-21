FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Estacionamento.Api/Estacionamento.Api.csproj", "Estacionamento.Api/"]
RUN dotnet restore "Estacionamento.Api/Estacionamento.Api.csproj"
COPY . .
WORKDIR "/src/Estacionamento.Api"
RUN dotnet build "Estacionamento.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Estacionamento.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Estacionamento.Api.dll"]