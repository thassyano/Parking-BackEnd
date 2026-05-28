# Deploy em produção (Railway)

Guia para subir **Back**, **Front** e **Evolution API** sem expor chaves no Git.

## O que NUNCA vai para o Git

| Segredo | Onde configurar |
|---------|-----------------|
| Senha do Postgres (Supabase/Railway) | Variável `ConnectionStrings__DefaultConnection` ou `DATABASE_URL` no Railway |
| JWT (`Jwt__Key`) | Variável no Railway (mín. 32 caracteres) |
| Evolution API Key | **Admin → Configuração** (salvo no banco) ou variáveis do serviço Evolution no Railway |
| CORS (URL do front) | `Cors__AllowedOrigins` no Railway |
| `.env.evolution` (Docker local) | Arquivo local, copiar de `.env.evolution.example` |

URLs públicas (API, front, Evolution) **podem** ir no código — não são segredo.

> **Importante:** se a senha do banco já foi commitada no passado, **troque a senha** no Supabase/Railway antes do deploy.

---

## 1. Backend (Parking API) — Railway

### Variáveis de ambiente

| Variável | Exemplo |
|----------|---------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | connection string do Postgres |
| `Jwt__Key` | string aleatória longa (32+ chars) |
| `Jwt__Issuer` | `EstacionamentoApi` |
| `Jwt__Audience` | `EstacionamentoApi` |
| `Cors__AllowedOrigins` | `https://SEU-FRONT.up.railway.app` |

Railway também aceita `DATABASE_URL` — o `Program.cs` já converte.

### Migrations

Rodar uma vez após deploy (local apontando para prod ou job Railway):

```bash
dotnet ef database update --project Estacionamento.Api
```

Migration necessária: `AdicionaConfirmacaoReserva` (campos Evolution + confirmação).

### Deploy

- Push na branch → Railway faz build pelo `Dockerfile` / `railway.toml`.
- `/health` deve responder 200.
- `/api/seed` **desabilitado em Production** (404).

---

## 2. Frontend — Railway (ou host estático)

Build de produção:

```bash
cd Parking-FrontEnd
# Em src/app/core/environment.ts: production = true
npm ci
npm run build -- --configuration=production
```

A URL da API fica em `environment.ts` (`production = true` → Railway).

---

## 3. Evolution API — Railway (serviço separado)

Serviço **independente** do Parking API.

### Recursos no Railway

1. **PostgreSQL** (plugin) — obrigatório na v2.
2. **Volume** montado em `/evolution/instances` — mantém sessão WhatsApp entre deploys.
3. **Domínio público** — ex. `https://evolution-xxxx.up.railway.app`

### Variáveis (serviço Evolution)

| Variável | Valor |
|----------|--------|
| `AUTHENTICATION_API_KEY` | chave forte (guarde no password manager) |
| `SERVER_URL` | `https://SUA-URL.up.railway.app` (sem `/manager`) |
| `SERVER_TYPE` | `http` |
| `SERVER_PORT` | `8080` ou `$PORT` conforme imagem |
| `DATABASE_ENABLED` | `true` |
| `DATABASE_PROVIDER` | `postgresql` |
| `DATABASE_CONNECTION_URI` | URL do Postgres do Railway |
| `CACHE_REDIS_ENABLED` | `false` |

Imagem recomendada: `evoapicloud/evolution-api:v2.3.6`

### Conectar WhatsApp (uma vez)

```powershell
$env:EVOLUTION_API_KEY = "sua-chave-do-railway"
.\scripts\evolution-qr.ps1 -BaseUrl "https://SUA-EVOLUTION.up.railway.app" -Recreate
```

Cliente escaneia com o WhatsApp do estacionamento. Status deve ficar **open**.

---

## 4. Configuração no Admin (banco de dados)

Após deploy, login no admin de **produção** → **Configuração**:

| Campo | Valor |
|--------|--------|
| Evolution API URL | `https://SUA-EVOLUTION.up.railway.app` |
| Evolution API Key | mesma do Railway (não aparece de volta no GET) |
| Nome da instância | `estacionamento` |
| URL confirmação (front) | `https://SEU-FRONT.up.railway.app` |
| Horas antecedência | ex. `48` |

**Testar envio WhatsApp** → deve chegar no celular de teste.

---

## 5. Checklist final

- [ ] Nenhum `.env` ou senha real no Git
- [ ] `Jwt__Key` forte no Railway (backend)
- [ ] Migrations aplicadas no banco de prod
- [ ] CORS com URL exata do front
- [ ] Evolution com Postgres + volume + `SERVER_URL`
- [ ] WhatsApp conectado (QR)
- [ ] Admin configurado + teste OK
- [ ] Reserva online de teste → mensagem automática (worker roda a cada 2h em Production)
- [ ] Link `/confirmar?token=...` abre o front e confirma

---

## Desenvolvimento local

```powershell
# Backend — connection string via User Secrets (não commitar):
cd Estacionamento.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "SUA_CONNECTION_STRING"

# Evolution local:
cp .env.evolution.example .env.evolution
# edite .env.evolution
docker compose -f docker-compose.evolution.yml up -d
$env:EVOLUTION_API_KEY = "sua-chave-local"
.\scripts\evolution-qr.ps1 -Recreate
```
