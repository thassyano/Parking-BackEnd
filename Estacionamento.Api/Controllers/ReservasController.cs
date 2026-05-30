using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Application.Services;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Helpers;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IReservaRepository _reservaRepository;
    private readonly ILogAtividadeService _logAtividadeService;

    public ReservasController(
        IReservaService reservaService,
        IWhatsAppService whatsAppService,
        IReservaRepository reservaRepository,
        ILogAtividadeService logAtividadeService)
    {
        _reservaService = reservaService;
        _whatsAppService = whatsAppService;
        _reservaRepository = reservaRepository;
        _logAtividadeService = logAtividadeService;
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
            !string.IsNullOrWhiteSpace(filtro.TipoVaga) ||
            !string.IsNullOrWhiteSpace(filtro.PlacaVeiculo)))
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
            await RegistrarLogAsync(AcaoLog.ReservaOnline, $"Reserva #{reserva.Id} online — {dto.NomeCliente}", "Reserva", reserva.Id, origem: OrigemLog.Cliente);
            return CreatedAtAction(nameof(ObterPorId), new { id = reserva.Id }, reserva);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>FLUXO ONLINE EM LOTE - Cliente reserva múltiplos veículos de uma vez</summary>
    [HttpPost("online/lote")]
    public async Task<IActionResult> CriarOnlineLote([FromBody] CriarReservaLoteOnlineDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var resultado = await _reservaService.CriarOnlineLoteAsync(dto);
            await RegistrarLogAsync(
                AcaoLog.ReservaOnline,
                $"{resultado.Reservas.Count} reserva(s) online — {dto.NomeCliente}",
                "Reserva",
                origem: OrigemLog.Cliente);
            return Ok(resultado);
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
            await RegistrarLogAsync(AcaoLog.ReservaPresencial, $"Reserva #{reserva.Id} presencial — {dto.NomeCliente}", "Reserva", reserva.Id);
            return CreatedAtAction(nameof(ObterPorId), new { id = reserva.Id }, reserva);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>FLUXO PRESENCIAL EM LOTE - Admin cadastra múltiplos veículos de um mesmo cliente</summary>
    [HttpPost("presencial/lote")]
    [Authorize]
    public async Task<IActionResult> CriarPresencialLote([FromBody] CriarReservaLotePresencialDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var resultado = await _reservaService.CriarPresencialLoteAsync(dto);
            await RegistrarLogAsync(
                AcaoLog.ReservaPresencial,
                $"{resultado.Reservas.Count} reserva(s) presencial — {dto.NomeCliente}",
                "Reserva");
            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Alterar duração da reserva e recalcular valor (apenas Pendente/Confirmada)</summary>
    [HttpPatch("{id}/alterar")]
    [Authorize]
    public async Task<IActionResult> Alterar(int id, [FromBody] AtualizarReservaDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var reserva = await _reservaService.AtualizarAsync(id, dto);
            if (reserva == null)
                return NotFound(new { message = "Reserva não encontrada" });

            await RegistrarLogAsync(AcaoLog.ReservaAlterada, $"Reserva #{id} alterada", "Reserva", id);
            return Ok(reserva);
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

        await RegistrarLogAsync(AcaoLog.ReservaPlacaAssociada, $"Placa {dto.PlacaVeiculo} associada à reserva #{id}", "Reserva", id);
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

            await RegistrarLogAsync(AcaoLog.ReservaCheckin, $"Check-in reserva #{id}", "Reserva", id);
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

            await RegistrarLogAsync(AcaoLog.ReservaCheckout, $"Check-out reserva #{id} — {dto.FormaPagamento}", "Reserva", id);
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

            await RegistrarLogAsync(AcaoLog.ReservaCancelada, $"Reserva #{id} cancelada", "Reserva", id);
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

    /// <summary>Confirmar reserva via link enviado por WhatsApp (público, sem autenticação)</summary>
    [HttpGet("confirmar/{token:guid}")]
    public async Task<IActionResult> ConfirmarReserva(Guid token)
    {
        var reserva = await _reservaRepository.ObterPorTokenAsync(token);
        if (reserva == null)
            return NotFound(new { message = "Link de confirmação inválido ou expirado." });

        if (reserva.Status == StatusReserva.Cancelada)
            return BadRequest(new { message = "Esta reserva já foi cancelada.", status = "Cancelada" });

        if (reserva.Status == StatusReserva.CheckoutRealizado)
            return Ok(new { message = "Reserva já concluída.", confirmada = true, reservaId = reserva.Id, nomeCliente = reserva.NomeCliente, dataEntrada = reserva.DataEntrada });

        if (!reserva.ConfirmadaPeloCliente)
        {
            reserva.ConfirmadaPeloCliente = true;
            if (reserva.Status == StatusReserva.Pendente)
                reserva.Status = StatusReserva.Confirmada;

            await _reservaRepository.AtualizarAsync(reserva);

            await _logAtividadeService.RegistrarAsync(new RegistrarLogAtividadeDto
            {
                Acao = AcaoLog.ReservaConfirmadaCliente,
                Detalhes = $"Reserva #{reserva.Id} confirmada pelo cliente {reserva.NomeCliente}",
                Entidade = "Reserva",
                EntidadeId = reserva.Id,
                Origem = OrigemLog.Cliente
            });
        }

        return Ok(new
        {
            message = "Reserva confirmada com sucesso!",
            confirmada = true,
            reservaId = reserva.Id,
            nomeCliente = reserva.NomeCliente,
            dataEntrada = reserva.DataEntrada,
            tipoVaga = reserva.TipoVaga.ToString(),
            placa = reserva.PlacaVeiculo
        });
    }

    /// <summary>Gerar link WhatsApp consolidado para múltiplas reservas</summary>
    [HttpPost("whatsapp/lote")]
    public async Task<IActionResult> GerarLinkWhatsAppLote([FromBody] List<int> reservaIds)
    {
        if (reservaIds == null || reservaIds.Count == 0)
            return BadRequest(new { message = "Informe pelo menos um ID de reserva" });

        try
        {
            var resultado = await _whatsAppService.GerarLinkLoteAsync(reservaIds);
            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Task RegistrarLogAsync(
        string acao,
        string detalhes,
        string? entidade = null,
        int? entidadeId = null,
        bool sucesso = true,
        string origem = OrigemLog.Admin) =>
        _logAtividadeService.RegistrarAsync(
            LogAtividadeHttpExtensions.CriarRegistro(User, acao, detalhes, entidade, entidadeId, sucesso, origem));
}
