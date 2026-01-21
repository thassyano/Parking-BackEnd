# Troubleshooting - Swagger não aparece

Se o Swagger não está aparecendo quando você acessa `https://localhost:5001/swagger`, siga estes passos:

## 1. Verifique se a aplicação está rodando corretamente

Pare a aplicação (Ctrl+C) e execute novamente:

```bash
cd Estacionamento.Api
dotnet run
```

Procure por mensagens de erro no console. Se houver erros, eles aparecerão aqui.

## 2. Verifique a URL correta

Dependendo da configuração, a URL pode ser:
- `https://localhost:7176/swagger` (HTTPS)
- `http://localhost:5109/swagger` (HTTP)
- `https://localhost:5001/swagger` (se configurado)

Verifique no console qual URL está sendo usada quando você executa `dotnet run`.

## 3. Verifique se há erros de compilação

Execute:

```bash
dotnet build
```

Se houver erros, corrija-os antes de executar.

## 4. Verifique o navegador

- Tente em modo anônimo/privado
- Limpe o cache do navegador
- Tente outro navegador
- Verifique o console do navegador (F12) para erros JavaScript

## 5. Verifique se os controllers estão sendo registrados

Acesse diretamente o JSON do Swagger:
- `https://localhost:7176/swagger/v1/swagger.json`

Se isso funcionar, o problema está no Swagger UI, não na API.

## 6. Verifique logs

Se ainda não funcionar, verifique os logs da aplicação no console para identificar erros específicos.

## Solução rápida

Se nada funcionar, tente:

1. Pare a aplicação completamente
2. Delete as pastas `bin` e `obj`:
   ```bash
   Remove-Item -Recurse -Force bin, obj
   ```
3. Recompile e execute:
   ```bash
   dotnet clean
   dotnet build
   dotnet run
   ```

