# Estacionamento API

API backend desenvolvida em .NET 8.0 para gerenciamento de estacionamento.

## Estrutura do Projeto

```
Estacionamento.Api
 ├── Controllers
 │   ├── AuthController.cs          # Autenticação de admin
 │   ├── OcupacoesController.cs     # Gerenciamento de ocupações
 │   ├── VagasController.cs         # Gerenciamento de vagas
 │   ├── PrecosController.cs        # Gerenciamento de preços
 │   ├── DisponibilidadeController.cs # Consulta de disponibilidade (público)
 │   └── DashboardController.cs     # Dashboard administrativo
 │
 ├── Application
 │   ├── Services
 │   │   ├── OcupacaoService.cs
 │   │   ├── VagaService.cs
 │   │   └── PrecoService.cs
 │   └── DTOs
 │       ├── CriarOcupacaoDto.cs
 │       ├── DisponibilidadeDto.cs
 │       ├── DashboardDto.cs
 │       └── LoginDto.cs
 │
 ├── Domain
 │   └── Entities
 │       ├── Vaga.cs
 │       ├── Ocupacao.cs
 │       ├── Preco.cs
 │       └── Admin.cs
 │
 ├── Infrastructure
 │   ├── Data
 │   │   ├── AppDbContext.cs
 │   │   └── Mappings
 │   └── Repositories
 │       ├── VagaRepository.cs
 │       ├── OcupacaoRepository.cs
 │       └── PrecoRepository.cs
 │
 ├── Program.cs
 └── appsettings.json
```

## Funcionalidades

### Autenticação
- Login de administrador com JWT
- Credenciais padrão: `admin` / `admin123`

### Vagas
- Criar vagas
- Listar todas as vagas
- Consultar disponibilidade (endpoint público)

### Ocupações
- Criar ocupação (estacionar veículo)
- Finalizar ocupação (saída do veículo)
- Calcular valor da estadia
- Listar ocupações ativas

### Preços
- Configurar preços por hora/minuto
- Consultar preço ativo
- Histórico de preços

### Dashboard
- Estatísticas gerais
- Receita do dia e do mês
- Ocupações ativas

## Configuração

### Banco de Dados

Por padrão, a API utiliza banco de dados InMemory. Para usar SQL Server, configure a connection string no `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EstacionamentoDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### JWT

As configurações de JWT estão no `appsettings.json`. Em produção, altere a chave secreta:

```json
{
  "Jwt": {
    "Key": "SuaChaveSecretaSuperSeguraAqui",
    "Issuer": "EstacionamentoApi",
    "Audience": "EstacionamentoApi"
  }
}
```

## Como Executar

1. Restaure as dependências:
```bash
dotnet restore
```

2. Execute a aplicação:
```bash
dotnet run
```

3. Acesse o Swagger em: `https://localhost:5001/swagger` (ou a porta configurada)

## Endpoints Principais

### Autenticação
- `POST /api/auth/login` - Login do admin

### Vagas (requer autenticação)
- `GET /api/vagas` - Listar todas as vagas
- `GET /api/vagas/{id}` - Obter vaga por ID
- `POST /api/vagas` - Criar nova vaga
- `GET /api/vagas/disponibilidade` - Consultar disponibilidade

### Ocupações (requer autenticação)
- `POST /api/ocupacoes` - Criar ocupação
- `GET /api/ocupacoes` - Listar ocupações ativas
- `GET /api/ocupacoes/{id}` - Obter ocupação por ID
- `POST /api/ocupacoes/{id}/finalizar` - Finalizar ocupação
- `GET /api/ocupacoes/{id}/calcular-valor` - Calcular valor

### Preços (requer autenticação)
- `GET /api/precos` - Listar todos os preços
- `GET /api/precos/ativo` - Obter preço ativo
- `POST /api/precos` - Criar novo preço

### Disponibilidade (público)
- `GET /api/disponibilidade` - Consultar disponibilidade de vagas

### Dashboard (requer autenticação)
- `GET /api/dashboard` - Obter estatísticas do dashboard

## Uso da API

### 1. Login
```bash
POST /api/auth/login
{
  "usuario": "admin",
  "senha": "admin123"
}
```

Resposta:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "usuario": "admin",
  "expiraEm": "2024-01-20T23:33:30Z"
}
```

### 2. Usar o token
Inclua o token no header de todas as requisições autenticadas:
```
Authorization: Bearer {token}
```

### 3. Criar uma vaga
```bash
POST /api/vagas
Authorization: Bearer {token}
{
  "numero": "A01"
}
```

### 4. Criar ocupação
```bash
POST /api/ocupacoes
Authorization: Bearer {token}
{
  "vagaId": 1,
  "placaVeiculo": "ABC1234"
}
```

### 5. Finalizar ocupação
```bash
POST /api/ocupacoes/1/finalizar
Authorization: Bearer {token}
```

## Tecnologias Utilizadas

- .NET 8.0
- Entity Framework Core 8.0
- JWT Authentication
- BCrypt para hash de senhas
- Swagger/OpenAPI

## Observações

- O banco de dados InMemory é recriado a cada execução
- Um admin padrão é criado automaticamente na primeira execução
- Um preço padrão (R$ 10,00/hora) é criado automaticamente
- Para produção, configure um banco de dados real e altere as credenciais padrão

