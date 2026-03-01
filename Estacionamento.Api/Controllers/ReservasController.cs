using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Application.Services;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;
    private readonly IWhatsAppService _whatsAppService;

    public ReservasController(IReservaService reservaService, IWhatsAppService whatsAppService)
    {
        _reservaService = reservaService;
        _whatsAppService = whatsAppService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Listar([FromQuery] FiltroReservaDto? filtro)
    {
        if (filtro != null && (filtro.DataInicio.HasValue || filtro.DataFim.HasValue || !string.IsNullOrEmpty(filtro.Status)))
        {
            var filtradas = await _reservaService.FiltrarAsync(filtro);
            return Ok(filtradas);
        }

        var reservas = await _reservaService.ObterTodasAsync();
        return Ok(reservas);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var reserva = await _reservaService.ObterPorIdAsync(id);
        if (reserva == null)
            return NotFound(new { message = "Reserva não encontrada" });

        return Ok(reserva);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarReservaDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var reserva = await _reservaService.CriarAsync(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = reserva.Id }, reserva);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("presencial")]
    [Authorize]
    public async Task<IActionResult> CriarPresencial([FromBody] CriarReservaPresencialDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var reserva = await _reservaService.CriarPresencialAsync(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = reserva.Id }, reserva);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/confirmar")]
    [Authorize]
    public async Task<IActionResult> Confirmar(int id)
    {
        var reserva = await _reservaService.ConfirmarAsync(id);
        if (reserva == null)
            return NotFound(new { message = "Reserva não encontrada" });

        return Ok(reserva);
    }

    [HttpPatch("{id}/checkin")]
    [Authorize]
    public async Task<IActionResult> Checkin(int id)
    {
        try
        {
            var reserva = await _reservaService.CheckinAsync(id);
            if (reserva == null)
                return NotFound(new { message = "Reserva não encontrada" });

            return Ok(reserva);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/checkout")]
    [Authorize]
    public async Task<IActionResult> Checkout(int id)
    {
        try
        {
            var reserva = await _reservaService.CheckoutAsync(id);
            if (reserva == null)
                return NotFound(new { message = "Reserva não encontrada" });

            return Ok(reserva);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/cancelar")]
    [Authorize]
    public async Task<IActionResult> Cancelar(int id)
    {
        try
        {
            var reserva = await _reservaService.CancelarAsync(id);
            if (reserva == null)
                return NotFound(new { message = "Reserva não encontrada" });

            return Ok(reserva);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/whatsapp")]
    public async Task<IActionResult> GerarLinkWhatsApp(int id)
    {
        try
        {
            var resultado = await _whatsAppService.GerarLinkAsync(id);
            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
