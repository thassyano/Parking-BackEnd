using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
                return StatusCode(500, new { message = "Não foi possível conectar ao banco de dados" });

            await _context.Database.MigrateAsync();

            var resultado = new List<string>();

            if (!await _context.Admins.AnyAsync())
            {
                var admin = new Admin
                {
                    Usuario = dto?.UsuarioAdmin ?? "admin",
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto?.SenhaAdmin ?? "admin123"),
                    Email = dto?.EmailAdmin ?? "admin@estacionamento.com",
                    Nome = dto?.NomeAdmin ?? "Administrador"
                };
                _context.Admins.Add(admin);
                resultado.Add($"Admin '{admin.Usuario}' criado");
            }
            else
            {
                resultado.Add("Admin já existe - não recriado");
            }

            if (!await _context.Configuracoes.AnyAsync())
            {
                var config = new ConfiguracaoEstacionamento
                {
                    TotalVagasCoberta = dto?.TotalVagasCoberta ?? 20,
                    TotalVagasDescoberta = dto?.TotalVagasDescoberta ?? 30,
                    TelefoneWhatsApp = dto?.TelefoneWhatsApp,
                    MensagemWhatsApp = "Olá! Fiz uma reserva no estacionamento.\n\nNome: {nome}\nData: {data} a {dataFim}\nTipo: {tipo}\nDias: {dias}\nValor: R$ {valor}\nPlaca: {placa}",
                    HorasAntecedenciaConfirmacao = 24
                };
                _context.Configuracoes.Add(config);
                resultado.Add("Configuração inicial criada");
            }
            else
            {
                resultado.Add("Configuração já existe - não recriada");
            }

            if (!await _context.Precos.AnyAsync())
            {
                var precos = new[]
                {
                    new Preco
                    {
                        TipoVaga = TipoVaga.Coberta,
                        ValorDiaria = dto?.ValorDiariaCoberta ?? 30.00m,
                        DescontoPix = dto?.DescontoPix ?? 5.0m,
                        DataInicio = DateTime.UtcNow,
                        Ativo = true
                    },
                    new Preco
                    {
                        TipoVaga = TipoVaga.Descoberta,
                        ValorDiaria = dto?.ValorDiariaDescoberta ?? 20.00m,
                        DescontoPix = dto?.DescontoPix ?? 5.0m,
                        DataInicio = DateTime.UtcNow,
                        Ativo = true
                    }
                };
                _context.Precos.AddRange(precos);
                resultado.Add($"Preços iniciais criados - Coberta: R${precos[0].ValorDiaria}, Descoberta: R${precos[1].ValorDiaria}");
            }
            else
            {
                resultado.Add("Preços já existem - não recriados");
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Seed executado com sucesso",
                resultados = resultado
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar seed");
            return StatusCode(500, new
            {
                message = "Erro ao executar seed",
                error = ex.Message,
                inner = ex.InnerException?.Message
            });
        }
    }
}

public class SeedDto
{
    public string? UsuarioAdmin { get; set; }
    public string? SenhaAdmin { get; set; }
    public string? EmailAdmin { get; set; }
    public string? NomeAdmin { get; set; }
    public int? TotalVagasCoberta { get; set; }
    public int? TotalVagasDescoberta { get; set; }
    public decimal? ValorDiariaCoberta { get; set; }
    public decimal? ValorDiariaDescoberta { get; set; }
    public decimal? DescontoPix { get; set; }
    public string? TelefoneWhatsApp { get; set; }
}
