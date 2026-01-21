using Microsoft.AspNetCore.Mvc;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Application.Services;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DisponibilidadeController : ControllerBase
{
    private readonly IVagaService _vagaService;

    public DisponibilidadeController(IVagaService vagaService)
    {
        _vagaService = vagaService;
    }

    [HttpGet]
    public async Task<IActionResult> ObterDisponibilidade()
    {
        var disponibilidade = await _vagaService.ObterDisponibilidadeAsync();
        return Ok(disponibilidade);
    }
}

