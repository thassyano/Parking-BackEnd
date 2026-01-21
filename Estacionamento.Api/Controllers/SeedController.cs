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
            // Verificar se já existe admin
            if (await _context.Admins.AnyAsync())
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
            _context.Admins.Add(admin);

            // Verificar se já existe preço
            if (!await _context.Precos.AnyAsync())
            {
                var preco = new Preco
                {
                    ValorHora = 10.00m,
                    ValorMinuto = 0.17m, // R$ 10.00 / 60 minutos
                    DataInicio = DateTime.UtcNow,
                    Ativo = true
                };
                _context.Precos.Add(preco);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar seed");
            return StatusCode(500, new { message = "Erro ao executar seed", error = ex.Message });
        }
    }
}

public class SeedDto
{
    public string? Usuario { get; set; }
    public string? Senha { get; set; }
    public string? Email { get; set; }
}

