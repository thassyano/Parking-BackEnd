# Configuração do Supabase (PostgreSQL)

## Passo 1: Criar projeto no Supabase

1. Acesse [https://supabase.com](https://supabase.com)
2. Crie uma conta (se ainda não tiver)
3. Clique em "New Project"
4. Preencha:
   - **Name**: Nome do seu projeto (ex: `estacionamento-db`)
   - **Database Password**: Escolha uma senha forte (anote ela!)
   - **Region**: Escolha a região mais próxima
   - **Pricing Plan**: Free

## Passo 2: Obter Connection String

**⚠️ IMPORTANTE:** Para .NET com Entity Framework Core, use apenas **Connection String** (não precisa da Data API).

1. No dashboard do Supabase, vá em **Settings** (ícone de engrenagem)
2. Clique em **Database**
3. Role até a seção **Connection string**
4. Você verá duas opções:
   - **URI** - Conexão direta (use para desenvolvimento)
   - **Connection pooling** - Conexão com pool (recomendado para produção)
5. **Selecione "Connection pooling"** (recomendado) ou **"URI"** (mais simples)
6. Copie a connection string

### Formato da Connection String

**Connection Pooling (Recomendado)** - Melhor performance e mais conexões simultâneas:
```
postgresql://postgres.[PROJECT_REF]:[SENHA]@aws-0-[REGION].pooler.supabase.com:6543/postgres
```

**URI (Direto)** - Mais simples, mas limitado a poucas conexões:
```
postgresql://postgres:[SENHA]@db.[PROJECT_REF].supabase.co:5432/postgres
```

**💡 Dica:** Use **Connection Pooling** para produção e desenvolvimento. É mais eficiente e permite mais conexões simultâneas.

## Passo 3: Configurar no appsettings.json

Edite o arquivo `appsettings.Development.json` e cole a connection string que você copiou do Supabase:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "postgresql://postgres.[PROJECT_REF]:[SENHA]@aws-0-[REGION].pooler.supabase.com:6543/postgres"
  }
}
```

**⚠️ IMPORTANTE:** 
- Cole a connection string **exatamente como aparece** no Supabase (ela já vem com todos os valores preenchidos)
- Não precisa substituir nada manualmente - apenas copie e cole
- A connection string já inclui a senha e o project reference

## Passo 4: Criar as Migrations

Execute os seguintes comandos para criar as tabelas no banco:

```bash
cd Estacionamento.Api

# Criar migration inicial
dotnet ef migrations add InitialCreate

# Aplicar migration no banco
dotnet ef database update
```

## Passo 5: Verificar no Supabase

1. No dashboard do Supabase, vá em **Table Editor**
2. Você deve ver as tabelas criadas:
   - `Admins`
   - `Vagas`
   - `Ocupacoes`
   - `Precos`

## Dica: Usar Variáveis de Ambiente (Segurança)

Para não expor a senha no código, você pode usar variáveis de ambiente:

### Windows (PowerShell):
```powershell
$env:ConnectionStrings__DefaultConnection = "postgresql://postgres:[SENHA]@db.[PROJECT_REF].supabase.co:5432/postgres"
```

### Windows (CMD):
```cmd
set ConnectionStrings__DefaultConnection=postgresql://postgres:[SENHA]@db.[PROJECT_REF].supabase.co:5432/postgres
```

### Linux/Mac:
```bash
export ConnectionStrings__DefaultConnection="postgresql://postgres:[SENHA]@db.[PROJECT_REF].supabase.co:5432/postgres"
```

## Troubleshooting

### Erro de conexão
- Verifique se a connection string está correta
- Confirme que o projeto está ativo no Supabase
- Verifique se o IP está liberado (Settings > Database > Connection pooling)

### Erro de SSL
Se houver erro de SSL, adicione `;SSL Mode=Require` na connection string:
```
postgresql://...?sslmode=require
```

### Erro de migration
Certifique-se de ter o Entity Framework Tools instalado:
```bash
dotnet tool install --global dotnet-ef
```

