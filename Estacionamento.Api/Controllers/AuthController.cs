using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Usuario == loginDto.Usuario && a.Ativo);

        if (admin == null || !BCrypt.Net.BCrypt.Verify(loginDto.Senha, admin.SenhaHash))
        {
            return Unauthorized(new { message = "Usuário ou senha inválidos" });
        }

        var token = GenerateJwtToken(admin);

        var response = new LoginResponseDto
        {
            Token = token,
            Usuario = admin.Usuario,
            ExpiraEm = DateTime.UtcNow.AddHours(8)
        };

        return Ok(response);
    }

    private string GenerateJwtToken(Domain.Entities.Admin admin)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurada")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Name, admin.Usuario),
            new Claim(ClaimTypes.Email, admin.Email),
            new Claim("role", "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

