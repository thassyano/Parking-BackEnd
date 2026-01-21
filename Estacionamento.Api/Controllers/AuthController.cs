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
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar conexão com banco
            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                return StatusCode(500, new 
                { 
                    message = "Erro de conexão com banco de dados",
                    error = "Não foi possível conectar ao banco. Verifique a connection string."
                });
            }

            // Verificar se existe algum admin
            var totalAdmins = await _context.Admins.CountAsync();
            if (totalAdmins == 0)
            {
                return BadRequest(new 
                { 
                    message = "Nenhum admin cadastrado",
                    hint = "Execute o endpoint /api/seed primeiro para criar o admin inicial"
                });
            }

            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Usuario == loginDto.Usuario && a.Ativo);

            if (admin == null)
            {
                return Unauthorized(new { message = "Usuário não encontrado ou inativo" });
            }

            // Verificar senha
            bool senhaValida;
            try
            {
                senhaValida = BCrypt.Net.BCrypt.Verify(loginDto.Senha, admin.SenhaHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar senha");
                return StatusCode(500, new { message = "Erro ao verificar senha", error = ex.Message });
            }

            if (!senhaValida)
            {
                return Unauthorized(new { message = "Senha inválida" });
            }

            // Gerar token
            string token;
            try
            {
                token = GenerateJwtToken(admin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar token JWT");
                return StatusCode(500, new { message = "Erro ao gerar token", error = ex.Message });
            }

            var response = new LoginResponseDto
            {
                Token = token,
                Usuario = admin.Usuario,
                ExpiraEm = DateTime.UtcNow.AddHours(8)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no login");
            return StatusCode(500, new 
            { 
                message = "Erro interno no servidor",
                error = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
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

