using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Helpers;
using Estacionamento.Api.Infrastructure.Data;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticoController : ControllerBase
{
    private readonly AppDbContext _context;

    public DiagnosticoController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Verificar()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();

            if (!canConnect)
                return Ok(new { conectado = false, erro = "Não foi possível conectar ao banco de dados" });

            var admins = await _context.Admins.CountAsync();
            var reservas = await _context.Reservas.CountAsync();
            var precos = await _context.Precos.CountAsync();
            var configuracoes = await _context.Configuracoes.CountAsync();

            return Ok(new
            {
                conectado = true,
                timestamp = DateTimeHelper.AgoraBrasilia(),
                tabelas = new
                {
                    admins,
                    reservas,
                    precos,
                    configuracoes
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { conectado = false, erro = ex.Message });
        }
    }
}
