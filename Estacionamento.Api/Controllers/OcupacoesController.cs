using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Application.Services;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OcupacoesController : ControllerBase
{
    private readonly IOcupacaoService _ocupacaoService;

    public OcupacoesController(IOcupacaoService ocupacaoService)
    {
        _ocupacaoService = ocupacaoService;
    }

    [HttpPost]
    public async Task<IActionResult> CriarOcupacao([FromBody] CriarOcupacaoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var ocupacao = await _ocupacaoService.CriarOcupacaoAsync(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = ocupacao.Id }, ocupacao);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ObterOcupacoesAtivas()
    {
        var ocupacoes = await _ocupacaoService.ObterOcupacoesAtivasAsync();
        return Ok(ocupacoes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var ocupacao = await _ocupacaoService.ObterOcupacaoPorIdAsync(id);
        if (ocupacao == null)
            return NotFound();

        return Ok(ocupacao);
    }

    [HttpPost("{id}/finalizar")]
    public async Task<IActionResult> FinalizarOcupacao(int id)
    {
        try
        {
            var ocupacao = await _ocupacaoService.FinalizarOcupacaoAsync(id);
            return Ok(ocupacao);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/calcular-valor")]
    public async Task<IActionResult> CalcularValor(int id)
    {
        try
        {
            var valor = await _ocupacaoService.CalcularValorAsync(id);
            return Ok(new { valor });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

