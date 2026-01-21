using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Application.Services;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VagasController : ControllerBase
{
    private readonly IVagaService _vagaService;

    public VagasController(IVagaService vagaService)
    {
        _vagaService = vagaService;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodas()
    {
        var vagas = await _vagaService.ObterTodasVagasAsync();
        return Ok(vagas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var vaga = await _vagaService.ObterVagaPorIdAsync(id);
        if (vaga == null)
            return NotFound();

        return Ok(vaga);
    }

    [HttpPost]
    public async Task<IActionResult> CriarVaga([FromBody] CriarVagaDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var vaga = await _vagaService.CriarVagaAsync(dto.Numero);
            return CreatedAtAction(nameof(ObterPorId), new { id = vaga.Id }, vaga);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("disponibilidade")]
    public async Task<IActionResult> ObterDisponibilidade()
    {
        var disponibilidade = await _vagaService.ObterDisponibilidadeAsync();
        return Ok(disponibilidade);
    }
}

public class CriarVagaDto
{
    public string Numero { get; set; } = string.Empty;
}

