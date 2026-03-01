using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Application.Services;

public interface IReservaService
{
    Task<ReservaResponseDto> CriarAsync(CriarReservaDto dto);
    Task<ReservaResponseDto> CriarPresencialAsync(CriarReservaPresencialDto dto);
    Task<IEnumerable<ReservaResponseDto>> ObterTodasAsync();
    Task<ReservaResponseDto?> ObterPorIdAsync(int id);
    Task<IEnumerable<ReservaResponseDto>> FiltrarAsync(FiltroReservaDto filtro);
    Task<ReservaResponseDto?> ConfirmarAsync(int id);
    Task<ReservaResponseDto?> CheckinAsync(int id);
    Task<ReservaResponseDto?> CheckoutAsync(int id);
    Task<ReservaResponseDto?> CancelarAsync(int id);
}

public class ReservaService : IReservaService
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IPrecoRepository _precoRepository;
    private readonly IConfiguracaoRepository _configuracaoRepository;
    private readonly IClienteService _clienteService;

    public ReservaService(
        IReservaRepository reservaRepository,
        IClienteRepository clienteRepository,
        IPrecoRepository precoRepository,
        IConfiguracaoRepository configuracaoRepository,
        IClienteService clienteService)
    {
        _reservaRepository = reservaRepository;
        _clienteRepository = clienteRepository;
        _precoRepository = precoRepository;
        _configuracaoRepository = configuracaoRepository;
        _clienteService = clienteService;
    }

    public async Task<ReservaResponseDto> CriarAsync(CriarReservaDto dto)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(dto.ClienteId)
            ?? throw new InvalidOperationException("Cliente não encontrado");

        var tipoVaga = Enum.Parse<TipoVaga>(dto.TipoVaga, true);
        var preco = await _precoRepository.ObterAtivoAsync(tipoVaga)
            ?? throw new InvalidOperationException($"Nenhum preço ativo para vaga {dto.TipoVaga}");

        await VerificarDisponibilidadeAsync(tipoVaga, dto.DataReserva, dto.QtdDias);

        FormaPagamento? formaPagamento = null;
        if (!string.IsNullOrEmpty(dto.FormaPagamento))
            formaPagamento = Enum.Parse<FormaPagamento>(dto.FormaPagamento, true);

        var valorTotal = preco.ValorDiaria * dto.QtdDias;
        decimal? desconto = null;
        var valorFinal = valorTotal;

        if (formaPagamento == FormaPagamento.Pix && preco.DescontoPix.HasValue && preco.DescontoPix.Value > 0)
        {
            desconto = valorTotal * (preco.DescontoPix.Value / 100);
            valorFinal = valorTotal - desconto.Value;
        }

        var reserva = new Reserva
        {
            ClienteId = dto.ClienteId,
            TipoVaga = tipoVaga,
            DataReserva = dto.DataReserva.Date,
            QtdDias = dto.QtdDias,
            DataFim = dto.DataReserva.Date.AddDays(dto.QtdDias - 1),
            ValorDiaria = preco.ValorDiaria,
            ValorTotal = valorTotal,
            DescontoAplicado = desconto,
            ValorFinal = valorFinal,
            FormaPagamento = formaPagamento,
            Origem = Enum.Parse<OrigemReserva>(dto.Origem, true),
            Observacoes = dto.Observacoes
        };

        var criada = await _reservaRepository.CriarAsync(reserva);

        // Recarregar com includes
        var completa = await _reservaRepository.ObterPorIdAsync(criada.Id);
        return MapToResponse(completa!);
    }

    public async Task<ReservaResponseDto> CriarPresencialAsync(CriarReservaPresencialDto dto)
    {
        var cliente = await _clienteService.ObterOuCriarPorTelefoneAsync(
            dto.NomeCliente, dto.TelefoneCliente, dto.EmailCliente,
            dto.PlacaVeiculo, dto.ModeloVeiculo, dto.CorVeiculo);

        var reservaDto = new CriarReservaDto
        {
            ClienteId = cliente.Id,
            TipoVaga = dto.TipoVaga,
            DataReserva = dto.DataReserva,
            QtdDias = dto.QtdDias,
            FormaPagamento = dto.FormaPagamento,
            Origem = "Presencial",
            Observacoes = dto.Observacoes
        };

        return await CriarAsync(reservaDto);
    }

    public async Task<IEnumerable<ReservaResponseDto>> ObterTodasAsync()
    {
        var reservas = await _reservaRepository.ObterTodasAsync();
        return reservas.Select(MapToResponse);
    }

    public async Task<ReservaResponseDto?> ObterPorIdAsync(int id)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        return reserva == null ? null : MapToResponse(reserva);
    }

    public async Task<IEnumerable<ReservaResponseDto>> FiltrarAsync(FiltroReservaDto filtro)
    {
        StatusReserva? status = null;
        if (!string.IsNullOrEmpty(filtro.Status))
            status = Enum.Parse<StatusReserva>(filtro.Status, true);

        TipoVaga? tipoVaga = null;
        if (!string.IsNullOrEmpty(filtro.TipoVaga))
            tipoVaga = Enum.Parse<TipoVaga>(filtro.TipoVaga, true);

        var reservas = await _reservaRepository.ObterFiltradoAsync(
            filtro.DataInicio, filtro.DataFim, status, tipoVaga, filtro.ClienteId);

        return reservas.Select(MapToResponse);
    }

    public async Task<ReservaResponseDto?> ConfirmarAsync(int id)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null) return null;

        reserva.Status = StatusReserva.Confirmada;
        reserva.DataConfirmacao = DateTime.UtcNow;
        reserva.ConfirmacaoEnviada = true;

        await _reservaRepository.AtualizarAsync(reserva);
        return MapToResponse(reserva);
    }

    public async Task<ReservaResponseDto?> CheckinAsync(int id)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null) return null;

        if (reserva.Status != StatusReserva.Confirmada && reserva.Status != StatusReserva.Pendente)
            throw new InvalidOperationException("Reserva não pode fazer check-in no status atual");

        reserva.Status = StatusReserva.CheckinRealizado;
        reserva.DataCheckin = DateTime.UtcNow;

        await _reservaRepository.AtualizarAsync(reserva);
        return MapToResponse(reserva);
    }

    public async Task<ReservaResponseDto?> CheckoutAsync(int id)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null) return null;

        if (reserva.Status != StatusReserva.CheckinRealizado)
            throw new InvalidOperationException("Check-in não foi realizado");

        reserva.Status = StatusReserva.CheckoutRealizado;
        reserva.DataCheckout = DateTime.UtcNow;

        await _reservaRepository.AtualizarAsync(reserva);
        return MapToResponse(reserva);
    }

    public async Task<ReservaResponseDto?> CancelarAsync(int id)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null) return null;

        if (reserva.Status == StatusReserva.CheckoutRealizado)
            throw new InvalidOperationException("Não é possível cancelar uma reserva já finalizada");

        reserva.Status = StatusReserva.Cancelada;

        await _reservaRepository.AtualizarAsync(reserva);
        return MapToResponse(reserva);
    }

    private async Task VerificarDisponibilidadeAsync(TipoVaga tipoVaga, DateTime dataReserva, int qtdDias)
    {
        var config = await _configuracaoRepository.ObterAsync()
            ?? throw new InvalidOperationException("Configuração do estacionamento não encontrada. Execute o seed primeiro.");

        var totalVagas = tipoVaga == TipoVaga.Coberta
            ? config.TotalVagasCoberta
            : config.TotalVagasDescoberta;

        for (int i = 0; i < qtdDias; i++)
        {
            var data = dataReserva.Date.AddDays(i);
            var ocupadas = await _reservaRepository.ContarVagasOcupadasAsync(tipoVaga, data);

            if (ocupadas >= totalVagas)
                throw new InvalidOperationException($"Não há vagas {tipoVaga} disponíveis para {data:dd/MM/yyyy}");
        }
    }

    private static ReservaResponseDto MapToResponse(Reserva r) => new()
    {
        Id = r.Id,
        Cliente = new ClienteResumoDto
        {
            Id = r.Cliente.Id,
            Nome = r.Cliente.Nome,
            Telefone = r.Cliente.Telefone,
            PlacaVeiculo = r.Cliente.PlacaVeiculo
        },
        TipoVaga = r.TipoVaga.ToString(),
        DataReserva = r.DataReserva,
        QtdDias = r.QtdDias,
        DataFim = r.DataFim,
        ValorDiaria = r.ValorDiaria,
        ValorTotal = r.ValorTotal,
        DescontoAplicado = r.DescontoAplicado,
        ValorFinal = r.ValorFinal,
        FormaPagamento = r.FormaPagamento?.ToString(),
        Status = r.Status.ToString(),
        Origem = r.Origem.ToString(),
        DataCheckin = r.DataCheckin,
        DataCheckout = r.DataCheckout,
        ConfirmacaoEnviada = r.ConfirmacaoEnviada,
        Observacoes = r.Observacoes,
        DataCriacao = r.DataCriacao
    };
}
