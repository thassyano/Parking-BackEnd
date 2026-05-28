using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Helpers;
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
    [Authorize(Policy = AdminRoles.AdminMaster)]
    public async Task<IActionResult> CriarAdmin([FromBody] CriarAdminDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var usuarioExistente = await _context.Admins
                .AnyAsync(a => a.Usuario == dto.Usuario || a.Email == dto.Email);

            if (usuarioExistente)
                return BadRequest(new { message = "Usuário ou email já existe" });

            var admin = new Admin
            {
                Usuario = dto.Usuario,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
                Email = dto.Email,
                Nome = dto.Nome ?? string.Empty,
                Perfil = PerfilAdmin.Admin,
                Ativo = true,
                DataCriacao = DateTimeHelper.AgoraBrasilia()
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin criado: {Usuario}", admin.Usuario);

            return CreatedAtAction(nameof(ObterPorId), new { id = admin.Id }, MapResponse(admin));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar admin");
            return StatusCode(500, new { message = "Erro ao criar admin", error = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Policy = AdminRoles.AdminMaster)]
    public async Task<IActionResult> ListarTodos()
    {
        var admins = await _context.Admins
            .OrderBy(a => a.Usuario)
            .Select(a => new AdminResponseDto
            {
                Id = a.Id,
                Usuario = a.Usuario,
                Email = a.Email,
                Nome = a.Nome,
                Perfil = a.Perfil,
                Ativo = a.Ativo,
                DataCriacao = a.DataCriacao
            })
            .ToListAsync();

        return Ok(admins);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AdminRoles.AdminMaster)]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var admin = await _context.Admins.FindAsync(id);
        if (admin == null)
            return NotFound();

        return Ok(MapResponse(admin));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AdminRoles.AdminMaster)]
    public async Task<IActionResult> AtualizarAdmin(int id, [FromBody] AtualizarAdminDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var admin = await _context.Admins.FindAsync(id);
        if (admin == null)
            return NotFound();

        var duplicado = await _context.Admins
            .AnyAsync(a => a.Id != id && (a.Usuario == dto.Usuario || a.Email == dto.Email));

        if (duplicado)
            return BadRequest(new { message = "Usuário ou email já existe" });

        admin.Usuario = dto.Usuario;
        admin.Email = dto.Email;
        admin.Nome = dto.Nome ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(dto.Senha))
            admin.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin atualizado: {Usuario}", admin.Usuario);

        return Ok(MapResponse(admin));
    }

    [HttpPut("{id}/ativar")]
    [Authorize(Policy = AdminRoles.AdminMaster)]
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
    [Authorize(Policy = AdminRoles.AdminMaster)]
    public async Task<IActionResult> Deletar(int id)
    {
        var admin = await _context.Admins.FindAsync(id);
        if (admin == null)
            return NotFound();

        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == admin.Id.ToString())
            return BadRequest(new { message = "Não é possível deletar seu próprio usuário" });

        _context.Admins.Remove(admin);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Admin deletado com sucesso" });
    }

    private static AdminResponseDto MapResponse(Admin admin) => new()
    {
        Id = admin.Id,
        Usuario = admin.Usuario,
        Email = admin.Email,
        Nome = admin.Nome,
        Perfil = admin.Perfil,
        Ativo = admin.Ativo,
        DataCriacao = admin.DataCriacao
    };
}

public class AtivarAdminDto
{
    public bool Ativo { get; set; }
}
