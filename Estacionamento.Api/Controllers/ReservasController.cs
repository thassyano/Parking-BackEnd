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

    /// <summary>Listar reservas (com filtros opcionais)</summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Listar([FromQuery] FiltroReservaDto? filtro)
    {
        if (filtro != null && (
            filtro.DataInicio.HasValue ||
            filtro.DataFim.HasValue ||
            !string.IsNullOrWhiteSpace(filtro.Status) ||
            !string.IsNullOrWhiteSpace(filtro.TipoVaga)))
        {
            var filtradas = await _reservaService.FiltrarAsync(filtro);
            return Ok(filtradas);
        }

        var reservas = await _reservaService.ObterTodasAsync();
        return Ok(reservas);
    }

    /// <summary>Buscar reserva por ID</summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var reserva = await _reservaService.ObterPorIdAsync(id);
        if (reserva == null)
            return NotFound(new { message = "Reserva não encontrada" });

        return Ok(reserva);
    }

    /// <summary>FLUXO ONLINE - Cliente reserva pelo site (sem placa)</summary>
    [HttpPost("online")]
    public async Task<IActionResult> CriarOnline([FromBody] CriarReservaOnlineDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var reserva = await _reservaService.CriarOnlineAsync(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = reserva.Id }, reserva);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>FLUXO ONLINE - Cliente reserva vários carros em um único envio</summary>
    [HttpPost("online/lote")]
    public async Task<IActionResult> CriarOnlineLote([FromBody] CriarReservaOnlineLoteDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var resultado = await _reservaService.CriarOnlineLoteAsync(dto);
            resultado.WhatsApp = await _whatsAppService.GerarLinkLoteAsync(resultado.Reservas.Select(r => r.Id));
            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Reservas.First().Id }, resultado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>FLUXO PRESENCIAL - Admin cadastra cliente que chegou (com placa e cor)</summary>
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

    /// <summary>Associar placa ao cliente online quando ele chega no estacionamento</summary>
    [HttpPatch("{id}/placa")]
    [Authorize]
    public async Task<IActionResult> AssociarPlaca(int id, [FromBody] AssociarPlacaDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var reserva = await _reservaService.AssociarPlacaAsync(id, dto);
        if (reserva == null)
            return NotFound(new { message = "Reserva não encontrada" });

        return Ok(reserva);
    }

    /// <summary>Check-in (marca entrada do veículo)</summary>
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

    /// <summary>Check-out + pagamento (cliente retira o carro e paga)</summary>
    [HttpPatch("{id}/checkout")]
    [Authorize]
    public async Task<IActionResult> Checkout(int id, [FromBody] CheckoutDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var reserva = await _reservaService.CheckoutAsync(id, dto);
            if (reserva == null)
                return NotFound(new { message = "Reserva não encontrada" });

            return Ok(reserva);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Cancelar reserva</summary>
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

    /// <summary>Gerar cupom de entrada</summary>
    [HttpGet("{id}/cupom-entrada")]
    [Authorize]
    public async Task<IActionResult> CupomEntrada(int id)
    {
        var cupom = await _reservaService.GerarCupomEntradaAsync(id);
        if (cupom == null)
            return NotFound(new { message = "Reserva não encontrada" });

        return Ok(cupom);
    }

    /// <summary>Gerar cupom de saída (comprovante de pagamento)</summary>
    [HttpGet("{id}/cupom-saida")]
    [Authorize]
    public async Task<IActionResult> CupomSaida(int id)
    {
        var cupom = await _reservaService.GerarCupomSaidaAsync(id);
        if (cupom == null)
            return NotFound(new { message = "Reserva não encontrada ou checkout não realizado" });

        return Ok(cupom);
    }

    /// <summary>Gerar link WhatsApp pós-reserva online</summary>
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
