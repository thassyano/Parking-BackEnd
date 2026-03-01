using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Application.Services;
using Estacionamento.Api.Domain.Entities;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        var precos = await _precoService.ObterTodosAsync();
        return Ok(precos);
    }

    [HttpGet("ativos")]
    public async Task<IActionResult> ObterAtivos()
    {
        var precos = await _precoService.ObterAtivosAsync();
        return Ok(precos);
    }

    [HttpGet("ativos/{tipoVaga}")]
    public async Task<IActionResult> ObterAtivoPorTipo(string tipoVaga)
    {
        if (!Enum.TryParse<TipoVaga>(tipoVaga, true, out var tipo))
            return BadRequest(new { message = "Tipo de vaga inválido. Use 'Coberta' ou 'Descoberta'" });

        var preco = await _precoService.ObterAtivoAsync(tipo);
        if (preco == null)
            return NotFound(new { message = $"Nenhum preço ativo para vaga {tipoVaga}" });

        return Ok(preco);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Criar([FromBody] CriarPrecoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!Enum.TryParse<TipoVaga>(dto.TipoVaga, true, out var tipo))
            return BadRequest(new { message = "Tipo de vaga inválido. Use 'Coberta' ou 'Descoberta'" });

        try
        {
            var preco = await _precoService.CriarAsync(tipo, dto.ValorDiaria, dto.DescontoPixDinheiro, dto.DataInicio);
            return CreatedAtAction(nameof(ObterAtivoPorTipo), new { tipoVaga = tipo.ToString() }, preco);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
