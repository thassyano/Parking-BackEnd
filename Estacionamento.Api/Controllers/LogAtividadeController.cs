using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Application.Services;
using Estacionamento.Api.Helpers;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LogAtividadeController : ControllerBase
{
    private readonly ILogAtividadeService _logAtividadeService;

    public LogAtividadeController(ILogAtividadeService logAtividadeService)
    {
        _logAtividadeService = logAtividadeService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] FiltroLogAtividadeDto filtro)
    {
        var resultado = await _logAtividadeService.ListarAsync(filtro);
        return Ok(resultado);
    }

    [HttpGet("acoes")]
    public IActionResult ListarAcoes()
    {
        return Ok(AcaoLog.Todas);
    }

    [HttpGet("origens")]
    public IActionResult ListarOrigens()
    {
        return Ok(new[] { OrigemLog.Admin, OrigemLog.Cliente, OrigemLog.Sistema, OrigemLog.Worker });
    }
}
