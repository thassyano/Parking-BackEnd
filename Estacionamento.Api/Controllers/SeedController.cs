using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Data;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<SeedController> _logger;

    public SeedController(AppDbContext context, ILogger<SeedController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Seed([FromBody] SeedDto? dto = null)
    {
        try
        {
            // Testar conexão com o banco primeiro
            _logger.LogInformation("Testando conexão com banco de dados...");
            var canConnect = await _context.Database.CanConnectAsync();
            
            if (!canConnect)
            {
                return StatusCode(500, new 
                { 
                    message = "Não foi possível conectar ao banco de dados",
                    error = "Verifique a connection string nas variáveis de ambiente"
                });
            }
            
            _logger.LogInformation("Conexão com banco estabelecida com sucesso");

            // Verificar se já existe admin
            var adminExists = false;
            try
            {
                adminExists = await _context.Admins.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar admins existentes");
                return StatusCode(500, new 
                { 
                    message = "Erro ao acessar tabela Admins",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }

            if (adminExists)
            {
                return BadRequest(new { message = "Admin já existe. Seed não executado." });
            }

            // Criar Admin inicial (usa valores do DTO ou padrão)
            var admin = new Admin
            {
                Usuario = dto?.Usuario ?? "admin",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto?.Senha ?? "admin123"),
                Email = dto?.Email ?? "admin@exemplo.com",
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };
            
            _logger.LogInformation("Criando admin: {Usuario}", admin.Usuario);
            _context.Admins.Add(admin);

            // Verificar se já existe preço
            var precoExists = false;
            try
            {
                precoExists = await _context.Precos.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao verificar preços, continuando sem criar preço");
            }

            if (!precoExists)
            {
                var preco = new Preco
                {
                    ValorHora = 10.00m,
                    ValorMinuto = 0.17m, // R$ 10.00 / 60 minutos
                    DataInicio = DateTime.UtcNow,
                    Ativo = true
                };
                _context.Precos.Add(preco);
                _logger.LogInformation("Preço padrão criado");
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Seed executado com sucesso");

            return Ok(new
            {
                message = "Seed executado com sucesso",
                admin = new { usuario = admin.Usuario, email = admin.Email },
                created = DateTime.UtcNow
            });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Erro de banco de dados ao executar seed");
            return StatusCode(500, new 
            { 
                message = "Erro de banco de dados",
                error = dbEx.Message,
                innerException = dbEx.InnerException?.Message,
                details = "Verifique se as migrations foram aplicadas e se a connection string está correta"
            });
        }
        catch (Npgsql.NpgsqlException npgsqlEx)
        {
            _logger.LogError(npgsqlEx, "Erro de conexão PostgreSQL");
            return StatusCode(500, new 
            { 
                message = "Erro de conexão com PostgreSQL/Supabase",
                error = npgsqlEx.Message,
                details = "Verifique a connection string e se o Supabase está acessível"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar seed");
            return StatusCode(500, new 
            { 
                message = "Erro ao executar seed",
                error = ex.Message,
                innerException = ex.InnerException?.Message,
                stackTrace = ex.StackTrace
            });
        }
    }
}

public class SeedDto
{
    public string? Usuario { get; set; }
    public string? Senha { get; set; }
    public string? Email { get; set; }
}

