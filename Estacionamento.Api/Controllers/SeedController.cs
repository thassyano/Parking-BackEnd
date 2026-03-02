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
    public async Task<IActionResult> Seed([FromBody] SeedDto dto)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
                return StatusCode(500, new { message = "Não foi possível conectar ao banco de dados" });

            if (await _context.Admins.AnyAsync())
                return BadRequest(new { message = "Admin já existe" });

            var admin = new Admin
            {
                Usuario = dto.Usuario,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
                Email = dto.Email,
                Nome = dto.Usuario
            };
            _context.Admins.Add(admin);

            if (!await _context.Configuracoes.AnyAsync())
            {
                _context.Configuracoes.Add(new ConfiguracaoEstacionamento
                {
                    NomeEstacionamento = "Estacionamento DF Park",
                    Endereco = "Quadra SMPW QD. 6 CJ. 1 LT 1-B, Núcleo Bandeirante",
                    Contato = "61 99572-9976",
                    Cnpj = "51.904.295/0001-61",
                    TotalVagasCoberta = 20,
                    TotalVagasDescoberta = 30,
                    TelefoneWhatsApp = "5561995729976",
                    MensagemWhatsApp = "Olá! Fiz uma reserva no estacionamento.\n\nID: {id}\nNome: {nome}\nEntrada: {entrada}\nSaída prevista: {saida}\nTipo: {tipo}\nDias: {dias}\nValor diária: R$ {valorDiaria}",
                    HorasAntecedenciaConfirmacao = 24
                });
            }

            if (!await _context.Precos.AnyAsync())
            {
                _context.Precos.AddRange(
                    new Preco { TipoVaga = TipoVaga.Coberta, ValorDiaria = 30.00m, DescontoPixDinheiro = 5.00m, DataInicio = DateTime.UtcNow, Ativo = true },
                    new Preco { TipoVaga = TipoVaga.Descoberta, ValorDiaria = 20.00m, DescontoPixDinheiro = 5.00m, DataInicio = DateTime.UtcNow, Ativo = true }
                );
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Seed executado com sucesso" });
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
    public string Usuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
