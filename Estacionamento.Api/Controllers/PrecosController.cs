using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Estacionamento.Api.Application.Services;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrecosController : ControllerBase
{
    private readonly IPrecoService _precoService;

    public PrecosController(IPrecoService precoService)
    {
        _precoService = precoService;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodos()
    {
        var precos = await _precoService.ObterTodosPrecosAsync();
        return Ok(precos);
    }

    [HttpGet("ativo")]
    public async Task<IActionResult> ObterPrecoAtivo()
    {
        var preco = await _precoService.ObterPrecoAtivoAsync();
        if (preco == null)
            return NotFound(new { message = "Nenhum preço ativo encontrado" });

        return Ok(preco);
    }

    [HttpPost]
    public async Task<IActionResult> CriarPreco([FromBody] CriarPrecoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var preco = await _precoService.CriarPrecoAsync(
                dto.ValorHora, 
                dto.ValorMinuto, 
                dto.DataInicio);
            return CreatedAtAction(nameof(ObterPrecoAtivo), preco);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class CriarPrecoDto
{
    public decimal ValorHora { get; set; }
    public decimal ValorMinuto { get; set; }
    public DateTime? DataInicio { get; set; }
}

