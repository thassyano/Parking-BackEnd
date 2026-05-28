# Parking-BackEnd

API de estacionamento com confirmação de reservas via WhatsApp (Evolution API).

**Deploy em produção:** veja [DEPLOY.md](./DEPLOY.md).

## Segredos

- **Não commitar** `.env.evolution`, senhas de banco ou JWT no Git.
- Evolution API Key fica no **admin** (banco) ou nas variáveis do Railway (serviço Evolution).
- Desenvolvimento local: `appsettings.Development.json.example` + `dotnet user-secrets`.

## Evolution local (Docker)

```powershell
cp .env.evolution.example .env.evolution
# edite .env.evolution
docker compose -f docker-compose.evolution.yml up -d
$env:EVOLUTION_API_KEY = "sua-chave"
.\scripts\evolution-qr.ps1 -Recreate
```

## API local

```powershell
cd Estacionamento.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "SUA_CONNECTION_STRING"
dotnet run
```

Swagger: http://localhost:5109/swagger

