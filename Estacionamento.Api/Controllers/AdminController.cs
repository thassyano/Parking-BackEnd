using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Data;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(AppDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CriarAdmin([FromBody] CriarAdminDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Verificar se o usuário já existe
            var usuarioExistente = await _context.Admins
                .AnyAsync(a => a.Usuario == dto.Usuario || a.Email == dto.Email);

            if (usuarioExistente)
            {
                return BadRequest(new { message = "Usuário ou email já existe" });
            }

            // Criar novo admin
            var admin = new Admin
            {
                Usuario = dto.Usuario,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
                Email = dto.Email,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin criado: {Usuario}", admin.Usuario);

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = admin.Id },
                new
                {
                    id = admin.Id,
                    usuario = admin.Usuario,
                    email = admin.Email,
                    ativo = admin.Ativo,
                    dataCriacao = admin.DataCriacao
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar admin");
            return StatusCode(500, new { message = "Erro ao criar admin", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ListarTodos()
    {
        var admins = await _context.Admins
            .Select(a => new
            {
                id = a.Id,
                usuario = a.Usuario,
                email = a.Email,
                ativo = a.Ativo,
                dataCriacao = a.DataCriacao
            })
            .ToListAsync();

        return Ok(admins);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var admin = await _context.Admins
            .Where(a => a.Id == id)
            .Select(a => new
            {
                id = a.Id,
                usuario = a.Usuario,
                email = a.Email,
                ativo = a.Ativo,
                dataCriacao = a.DataCriacao
            })
            .FirstOrDefaultAsync();

        if (admin == null)
            return NotFound();

        return Ok(admin);
    }

    [HttpPut("{id}/ativar")]
    public async Task<IActionResult> AtivarDesativar(int id, [FromBody] AtivarAdminDto dto)
    {
        var admin = await _context.Admins.FindAsync(id);
        if (admin == null)
            return NotFound();

        admin.Ativo = dto.Ativo;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Admin {(dto.Ativo ? "ativado" : "desativado")} com sucesso" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id)
    {
        var admin = await _context.Admins.FindAsync(id);
        if (admin == null)
            return NotFound();

        // Não permitir deletar o próprio admin (se quiser, pode remover essa validação)
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == admin.Id.ToString())
        {
            return BadRequest(new { message = "Não é possível deletar seu próprio usuário" });
        }

        _context.Admins.Remove(admin);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Admin deletado com sucesso" });
    }
}

public class CriarAdminDto
{
    public string Usuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class AtivarAdminDto
{
    public bool Ativo { get; set; }
}

