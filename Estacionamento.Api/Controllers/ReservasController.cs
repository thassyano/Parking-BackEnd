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
    private readonly ILogAtividadeService _logService;

    public ReservasController(
        IReservaService reservaService,
        IWhatsAppService whatsAppService,
        IReservaRepository reservaRepository,
        ILogAtividadeService logService)
    {
        _reservaService = reservaService;
        _whatsAppService = whatsAppService;
        _reservaRepository = reservaRepository;
        _logService = logService;
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
            return Ok(resultado);
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

    /// <summary>Prévia do check-out: recalcula valores conforme forma de pagamento e traslado, sem gravar nada</summary>
    [HttpGet("{id}/previa-checkout")]
    [Authorize]
    public async Task<IActionResult> PreviaCheckout(int id, [FromQuery] string formaPagamento, [FromQuery] bool comTraslado = false)
    {
        try
        {
            var previa = await _reservaService.PreviaCheckoutAsync(id, formaPagamento, comTraslado);
            if (previa == null)
                return NotFound(new { message = "Reserva não encontrada" });

            return Ok(previa);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Corrige a forma de pagamento depois do check-out (refaz desconto e traslado)</summary>
    [HttpPatch("{id}/forma-pagamento")]
    [Authorize]
    public async Task<IActionResult> AlterarFormaPagamento(int id, [FromBody] AlterarFormaPagamentoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var antes = await _reservaService.ObterPorIdAsync(id);
            var reserva = await _reservaService.AlterarFormaPagamentoAsync(id, dto);
            if (reserva == null)
                return NotFound(new { message = "Reserva não encontrada" });

            await _logService.RegistrarAsync(LogAtividadeHttpExtensions.CriarRegistro(
                User, AcaoLog.ReservaPagamentoAlterado,
                $"Pagamento da reserva #{id} alterado de {antes?.FormaPagamento ?? "-"} para {reserva.FormaPagamento}. " +
                $"Valor final: R$ {antes?.ValorFinal:N2} -> R$ {reserva.ValorFinal:N2}",
                "Reserva", id));

            return Ok(reserva);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Altera marca, modelo e cor do veículo (qualquer perfil de admin)</summary>
    [HttpPatch("{id}/dados-veiculo")]
    [Authorize]
    public async Task<IActionResult> AtualizarDadosVeiculo(int id, [FromBody] AtualizarDadosVeiculoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var reserva = await _reservaService.AtualizarDadosVeiculoAsync(id, dto);
        if (reserva == null)
            return NotFound(new { message = "Reserva não encontrada" });

        await _logService.RegistrarAsync(LogAtividadeHttpExtensions.CriarRegistro(
            User, AcaoLog.ReservaVeiculoAlterado,
            $"Dados do veículo da reserva #{id}: marca '{reserva.MarcaVeiculo ?? "-"}', modelo '{reserva.ModeloVeiculo ?? "-"}', cor '{reserva.CorVeiculo ?? "-"}'",
            "Reserva", id));

        return Ok(reserva);
    }

    /// <summary>Procura reserva ativa da mesma placa que se cruza com o período (404 quando não há)</summary>
    [HttpGet("conflito")]
    public async Task<IActionResult> VerificarConflito(
        [FromQuery] string placa, [FromQuery] DateTime dataEntrada, [FromQuery] DateTime dataSaidaPrevista)
    {
        var conflito = await _reservaService.VerificarConflitoAsync(placa, dataEntrada, dataSaidaPrevista);
        if (conflito == null)
            return NotFound(new { message = "Nenhum conflito encontrado" });

        return Ok(conflito);
    }

    /// <summary>Admin altera o período (e opcionalmente a vaga) de uma reserva ainda não iniciada</summary>
    [HttpPatch("{id}/alterar")]
    [Authorize]
    public async Task<IActionResult> Alterar(int id, [FromBody] AlterarReservaAdminDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var reserva = await _reservaService.AlterarAdminAsync(id, dto);
            if (reserva == null)
                return NotFound(new { message = "Reserva não encontrada" });

            await _logService.RegistrarAsync(LogAtividadeHttpExtensions.CriarRegistro(
                User, AcaoLog.ReservaSobrescrita,
                $"Reserva #{id} ({reserva.PlacaVeiculo}) atualizada para {reserva.DataEntrada:dd/MM/yyyy HH:mm} - {reserva.DataSaidaPrevista:dd/MM/yyyy HH:mm}",
                "Reserva", id));

            return Ok(reserva);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Cliente altera a própria reserva (confirma com placa + telefone, sem login)</summary>
    [HttpPatch("{id}/alterar-cliente")]
    public async Task<IActionResult> AlterarCliente(int id, [FromBody] AlterarReservaClienteDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var reserva = await _reservaService.AlterarClienteAsync(id, dto);
            if (reserva == null)
                return NotFound(new { message = "Reserva não encontrada" });

            await _logService.RegistrarAsync(LogAtividadeHttpExtensions.CriarRegistro(
                User, AcaoLog.ReservaSobrescrita,
                $"Cliente atualizou a reserva #{id} ({reserva.PlacaVeiculo}) para {reserva.DataEntrada:dd/MM/yyyy HH:mm} - {reserva.DataSaidaPrevista:dd/MM/yyyy HH:mm}",
                "Reserva", id, origem: OrigemLog.Cliente));

            return Ok(reserva);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Admin edita a reserva inteira (cliente, veículo, vaga, período e observações)</summary>
    [HttpPatch("{id}/editar")]
    [Authorize]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarReservaAdminDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var antes = await _reservaService.ObterPorIdAsync(id);
            var reserva = await _reservaService.EditarAdminAsync(id, dto);
            if (reserva == null)
                return NotFound(new { message = "Reserva não encontrada" });

            await _logService.RegistrarAsync(LogAtividadeHttpExtensions.CriarRegistro(
                User, AcaoLog.ReservaEditada,
                $"Reserva #{id} editada. Vaga: {antes?.TipoVaga ?? "-"} -> {reserva.TipoVaga}; " +
                $"período: {antes?.DataEntrada:dd/MM/yyyy HH:mm} - {antes?.DataSaidaPrevista:dd/MM/yyyy HH:mm} -> " +
                $"{reserva.DataEntrada:dd/MM/yyyy HH:mm} - {reserva.DataSaidaPrevista:dd/MM/yyyy HH:mm}; " +
                $"placa: {antes?.PlacaVeiculo ?? "-"} -> {reserva.PlacaVeiculo ?? "-"}",
                "Reserva", id));

            return Ok(reserva);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cliente consulta as próprias reservas futuras pela placa + telefone (sem login).
    /// Sem o telefone conferindo nada é devolvido, e a resposta não traz dados pessoais.
    /// </summary>
    [HttpPost("consulta-cliente")]
    public async Task<IActionResult> ConsultarCliente([FromBody] ConsultaReservaClienteDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var reservas = await _reservaService.ConsultarClienteAsync(dto);
        return Ok(reservas);
    }

    /// <summary>Cliente altera marca/modelo/cor e tipo de vaga da própria reserva (até o limite de antecedência)</summary>
    [HttpPatch("{id}/alterar-dados-cliente")]
    public async Task<IActionResult> AlterarDadosCliente(int id, [FromBody] AlterarDadosClienteDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var antes = await _reservaService.ObterPorIdAsync(id);
            var reserva = await _reservaService.AlterarDadosClienteAsync(id, dto);
            if (reserva == null)
                return NotFound(new { message = "Reserva não encontrada" });

            await _logService.RegistrarAsync(LogAtividadeHttpExtensions.CriarRegistro(
                User, AcaoLog.ReservaAlteradaCliente,
                $"Cliente alterou a reserva #{id} ({reserva.PlacaVeiculo}): vaga {antes?.TipoVaga ?? "-"} -> {reserva.TipoVaga}; " +
                $"veículo {reserva.MarcaVeiculo ?? "-"} {reserva.ModeloVeiculo ?? "-"} ({reserva.CorVeiculo ?? "-"})",
                "Reserva", id, origem: OrigemLog.Cliente));

            return Ok(reserva);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Link do WhatsApp avisando que a reserva foi alterada</summary>
    [HttpGet("{id}/whatsapp-alteracao")]
    public async Task<IActionResult> GerarLinkWhatsAppAlteracao(int id)
    {
        try
        {
            return Ok(await _whatsAppService.GerarLinkAlteracaoAsync(id));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Link do WhatsApp consolidado avisando alteração de várias reservas</summary>
    [HttpPost("whatsapp/lote-alteracao")]
    public async Task<IActionResult> GerarLinkWhatsAppAlteracaoLote([FromBody] List<int> reservaIds)
    {
        if (reservaIds == null || reservaIds.Count == 0)
            return BadRequest(new { message = "Informe pelo menos um ID de reserva" });

        try
        {
            return Ok(await _whatsAppService.GerarLinkAlteracaoLoteAsync(reservaIds));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
