using Microsoft.AspNetCore.Mvc;
using Estacionamento.Api.Application.Services;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DisponibilidadeController : ControllerBase
{
    private readonly IDisponibilidadeService _disponibilidadeService;

    public DisponibilidadeController(IDisponibilidadeService disponibilidadeService)
    {
        _disponibilidadeService = disponibilidadeService;
    }

    [HttpGet]
    public async Task<IActionResult> ConsultarDia([FromQuery] DateTime data)
    {
        var disponibilidade = await _disponibilidadeService.ConsultarDiaAsync(data);
        return Ok(disponibilidade);
    }

    [HttpGet("periodo")]
    public async Task<IActionResult> ConsultarPeriodo([FromQuery] DateTime dataInicio, [FromQuery] DateTime dataFim)
    {
        if (dataFim < dataInicio)
            return BadRequest(new { message = "Data fim deve ser maior ou igual à data início" });

        if ((dataFim - dataInicio).TotalDays > 60)
            return BadRequest(new { message = "Período máximo de consulta é 60 dias" });

        var disponibilidade = await _disponibilidadeService.ConsultarPeriodoAsync(dataInicio, dataFim);
        return Ok(disponibilidade);
    }
}
