using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Data;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var admins = await _context.Admins
            .Select(a => new AdminResponseDto
            {
                Id = a.Id,
                Usuario = a.Usuario,
                Email = a.Email,
                Nome = a.Nome,
                DataCriacao = a.DataCriacao,
                Ativo = a.Ativo
            })
            .ToListAsync();

        return Ok(admins);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarAdminDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _context.Admins.AnyAsync(a => a.Usuario == dto.Usuario))
            return BadRequest(new { message = "Usuário já existe" });

        if (await _context.Admins.AnyAsync(a => a.Email == dto.Email))
            return BadRequest(new { message = "Email já cadastrado" });

        var admin = new Admin
        {
            Usuario = dto.Usuario,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            Email = dto.Email,
            Nome = dto.Nome ?? dto.Usuario
        };

        _context.Admins.Add(admin);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Listar), new AdminResponseDto
        {
            Id = admin.Id,
            Usuario = admin.Usuario,
            Email = admin.Email,
            Nome = admin.Nome,
            DataCriacao = admin.DataCriacao,
            Ativo = admin.Ativo
        });
    }

    [HttpPatch("{id}/desativar")]
    public async Task<IActionResult> Desativar(int id)
    {
        var admin = await _context.Admins.FindAsync(id);
        if (admin == null)
            return NotFound(new { message = "Admin não encontrado" });

        admin.Ativo = false;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Admin desativado" });
    }

    [HttpPatch("{id}/ativar")]
    public async Task<IActionResult> Ativar(int id)
    {
        var admin = await _context.Admins.FindAsync(id);
        if (admin == null)
            return NotFound(new { message = "Admin não encontrado" });

        admin.Ativo = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Admin ativado" });
    }
}
