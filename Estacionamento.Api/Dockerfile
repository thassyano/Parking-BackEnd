FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Estacionamento.Api/Estacionamento.Api.csproj", "Estacionamento.Api/"]
RUN dotnet restore "Estacionamento.Api/Estacionamento.Api.csproj"
COPY . .
WORKDIR "/src/Estacionamento.Api"
RUN dotnet publish "Estacionamento.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Estacionamento.Api.dll"]
