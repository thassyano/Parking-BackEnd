using Microsoft.AspNetCore.Mvc;
using Estacionamento.Api.Application.Services;
using Estacionamento.Api.Helpers;

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
        try
        {
            DateTimeHelper.ValidarPeriodoReserva(data, data);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        var disponibilidade = await _disponibilidadeService.ConsultarDiaAsync(data);
        return Ok(disponibilidade);
    }

    [HttpGet("periodo")]
    public async Task<IActionResult> ConsultarPeriodo([FromQuery] DateTime dataInicio, [FromQuery] DateTime dataFim)
    {
        try
        {
            DateTimeHelper.ValidarPeriodoReserva(dataInicio, dataFim);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        if ((dataFim - dataInicio).TotalDays > 60)
            return BadRequest(new { message = "Período máximo de consulta é 60 dias" });

        var disponibilidade = await _disponibilidadeService.ConsultarPeriodoAsync(dataInicio, dataFim);
        return Ok(disponibilidade);
    }
}
