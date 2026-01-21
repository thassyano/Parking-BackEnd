using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Infrastructure.Data;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticoController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DiagnosticoController> _logger;

    public DiagnosticoController(
        AppDbContext context, 
        IConfiguration configuration,
        ILogger<DiagnosticoController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Verificar()
    {
        var connString = _configuration.GetConnectionString("DefaultConnection");
        var connStringFromEnv = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        
        var diagnostico = new
        {
            timestamp = DateTime.UtcNow,
            connectionString = new
            {
                configurada = !string.IsNullOrEmpty(connString),
                temValor = !string.IsNullOrEmpty(connString),
                tamanho = connString?.Length ?? 0,
                preview = connString != null 
                    ? connString.Substring(0, Math.Min(80, connString.Length)) + "..."
                    : "não configurada",
                temCaractereEspecial = connString?.Contains("#") ?? false,
                variavelAmbiente = !string.IsNullOrEmpty(connStringFromEnv)
            },
            bancoDados = await VerificarBancoDados()
        };

        return Ok(diagnostico);
    }

    private async Task<object> VerificarBancoDados()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            
            if (!canConnect)
            {
                return new
                {
                    conectado = false,
                    erro = "Não foi possível conectar ao banco de dados"
                };
            }

            // Tentar contar registros nas tabelas
            var adminsCount = 0;
            var precosCount = 0;
            var vagasCount = 0;
            var ocupacoesCount = 0;

            try
            {
                adminsCount = await _context.Admins.CountAsync();
            }
            catch (Exception ex)
            {
                return new
                {
                    conectado = true,
                    erro = $"Erro ao acessar tabelas: {ex.Message}",
                    innerException = ex.InnerException?.Message
                };
            }

            try
            {
                precosCount = await _context.Precos.CountAsync();
                vagasCount = await _context.Vagas.CountAsync();
                ocupacoesCount = await _context.Ocupacoes.CountAsync();
            }
            catch { }

            return new
            {
                conectado = true,
                tabelas = new
                {
                    admins = adminsCount,
                    precos = precosCount,
                    vagas = vagasCount,
                    ocupacoes = ocupacoesCount
                }
            };
        }
        catch (Exception ex)
        {
            return new
            {
                conectado = false,
                erro = ex.Message,
                tipoErro = ex.GetType().Name,
                innerException = ex.InnerException?.Message
            };
        }
    }
}

