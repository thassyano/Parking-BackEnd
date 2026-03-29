using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Helpers;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Application.Services;

public interface IReservaService
{
    Task<ReservaResponseDto> CriarOnlineAsync(CriarReservaOnlineDto dto);
    Task<ReservaLoteResponseDto> CriarOnlineLoteAsync(CriarReservaOnlineLoteDto dto);
    Task<ReservaResponseDto> CriarPresencialAsync(CriarReservaPresencialDto dto);
    Task<IEnumerable<ReservaResponseDto>> ObterTodasAsync();
    Task<ReservaResponseDto?> ObterPorIdAsync(int id);
    Task<IEnumerable<ReservaResponseDto>> FiltrarAsync(FiltroReservaDto filtro);
    Task<ReservaResponseDto?> AssociarPlacaAsync(int id, AssociarPlacaDto dto);
    Task<ReservaResponseDto?> CheckinAsync(int id);
    Task<ReservaResponseDto?> CheckoutAsync(int id, CheckoutDto dto);
    Task<ReservaResponseDto?> CancelarAsync(int id);
    Task<CupomEntradaDto?> GerarCupomEntradaAsync(int id);
    Task<CupomSaidaDto?> GerarCupomSaidaAsync(int id);
}

public class ReservaService : IReservaService
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IPrecoRepository _precoRepository;
    private readonly IConfiguracaoRepository _configuracaoRepository;

    public ReservaService(
        IReservaRepository reservaRepository,
        IPrecoRepository precoRepository,
        IConfiguracaoRepository configuracaoRepository)
    {
        _reservaRepository = reservaRepository;
        _precoRepository = precoRepository;
        _configuracaoRepository = configuracaoRepository;
    }

    public async Task<ReservaResponseDto> CriarOnlineAsync(CriarReservaOnlineDto dto)
    {
        var tipoVaga = Enum.Parse<TipoVaga>(dto.TipoVaga, true);
        var preco = await _precoRepository.ObterAtivoAsync(tipoVaga)
            ?? throw new InvalidOperationException($"Nenhum preço ativo para vaga {dto.TipoVaga}");

        await VerificarDisponibilidadeAsync(tipoVaga, dto.DataEntrada, dto.QtdDias);

        var valorTotal = preco.ValorDiaria * dto.QtdDias;

        var reserva = new Reserva
        {
            NomeCliente = dto.NomeCliente,
            TelefoneCliente = dto.TelefoneCliente,
            CpfCliente = dto.CpfCliente,
            PlacaVeiculo = NormalizarPlaca(dto.PlacaVeiculo),
            TipoVaga = tipoVaga,
            DataEntrada = dto.DataEntrada,
            QtdDias = dto.QtdDias,
            DataSaidaPrevista = dto.DataSaidaPrevista,
            ValorDiaria = preco.ValorDiaria,
            ValorTotal = valorTotal,
            ValorFinal = valorTotal,
            Origem = OrigemReserva.Online,
            Status = StatusReserva.Pendente,
            Observacoes = dto.Observacoes
        };

        var criada = await _reservaRepository.CriarAsync(reserva);
        return MapToResponse(criada);
    }

    public async Task<ReservaLoteResponseDto> CriarOnlineLoteAsync(CriarReservaOnlineLoteDto dto)
    {
        var placasNormalizadas = dto.PlacasVeiculos
            .Select(NormalizarPlaca)
            .Where(placa => !string.IsNullOrWhiteSpace(placa))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (placasNormalizadas.Count == 0)
            throw new InvalidOperationException("Informe ao menos uma placa válida");

        var tipoVaga = Enum.Parse<TipoVaga>(dto.TipoVaga, true);
        var preco = await _precoRepository.ObterAtivoAsync(tipoVaga)
            ?? throw new InvalidOperationException($"Nenhum preço ativo para vaga {dto.TipoVaga}");

        await VerificarDisponibilidadeAsync(tipoVaga, dto.DataEntrada, dto.QtdDias, placasNormalizadas.Count);

        var valorTotalPorReserva = preco.ValorDiaria * dto.QtdDias;
        var descontoPixDinheiroPorReserva = preco.DescontoPixDinheiro * dto.QtdDias;

        var reservas = placasNormalizadas.Select(placa => new Reserva
        {
            NomeCliente = dto.NomeCliente,
            TelefoneCliente = dto.TelefoneCliente,
            CpfCliente = dto.CpfCliente,
            PlacaVeiculo = placa,
            TipoVaga = tipoVaga,
            DataEntrada = dto.DataEntrada,
            QtdDias = dto.QtdDias,
            DataSaidaPrevista = dto.DataSaidaPrevista,
            ValorDiaria = preco.ValorDiaria,
            ValorTotal = valorTotalPorReserva,
            ValorFinal = valorTotalPorReserva,
            Origem = OrigemReserva.Online,
            Status = StatusReserva.Pendente,
            Observacoes = dto.Observacoes
        });

        var criadas = await _reservaRepository.CriarEmLoteAsync(reservas);

        return new ReservaLoteResponseDto
        {
            Reservas = criadas.Select(MapToResponse).ToList(),
            QuantidadeVeiculos = criadas.Count,
            ValorTotalCartao = criadas.Sum(r => r.ValorTotal),
            ValorTotalPixDinheiro = criadas.Sum(r => r.ValorTotal - descontoPixDinheiroPorReserva),
            EconomiaTotal = criadas.Count * descontoPixDinheiroPorReserva
        };
    }

    public async Task<ReservaResponseDto> CriarPresencialAsync(CriarReservaPresencialDto dto)
    {
        var tipoVaga = Enum.Parse<TipoVaga>(dto.TipoVaga, true);
        var preco = await _precoRepository.ObterAtivoAsync(tipoVaga)
            ?? throw new InvalidOperationException($"Nenhum preço ativo para vaga {dto.TipoVaga}");

        await VerificarDisponibilidadeAsync(tipoVaga, dto.DataEntrada, dto.QtdDias);

        var valorTotal = preco.ValorDiaria * dto.QtdDias;

        var reserva = new Reserva
        {
            NomeCliente = dto.NomeCliente,
            TelefoneCliente = dto.TelefoneCliente,
            CpfCliente = dto.CpfCliente,
            PlacaVeiculo = NormalizarPlaca(dto.PlacaVeiculo),
            TipoVaga = tipoVaga,
            DataEntrada = dto.DataEntrada,
            QtdDias = dto.QtdDias,
            DataSaidaPrevista = dto.DataSaidaPrevista,
            ValorDiaria = preco.ValorDiaria,
            ValorTotal = valorTotal,
            ValorFinal = valorTotal,
            Origem = OrigemReserva.Presencial,
            Status = StatusReserva.CheckinRealizado,
            DataCheckin = DateTimeHelper.AgoraBrasilia(),
            Observacoes = dto.Observacoes
        };

        var criada = await _reservaRepository.CriarAsync(reserva);
        return MapToResponse(criada);
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
            filtro.DataInicio, filtro.DataFim, status, tipoVaga);

        return reservas.Select(MapToResponse);
    }

    public async Task<ReservaResponseDto?> AssociarPlacaAsync(int id, AssociarPlacaDto dto)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null) return null;

        reserva.PlacaVeiculo = NormalizarPlaca(dto.PlacaVeiculo);

        await _reservaRepository.AtualizarAsync(reserva);
        return MapToResponse(reserva);
    }

    public async Task<ReservaResponseDto?> CheckinAsync(int id)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null) return null;

        if (string.IsNullOrEmpty(reserva.PlacaVeiculo))
            throw new InvalidOperationException("Associe a placa do veículo antes de fazer check-in");

        if (reserva.Status != StatusReserva.Pendente && reserva.Status != StatusReserva.Confirmada)
            throw new InvalidOperationException("Reserva não pode fazer check-in no status atual");

        reserva.Status = StatusReserva.CheckinRealizado;
        reserva.DataCheckin = DateTimeHelper.AgoraBrasilia();

        await _reservaRepository.AtualizarAsync(reserva);
        return MapToResponse(reserva);
    }

    public async Task<ReservaResponseDto?> CheckoutAsync(int id, CheckoutDto dto)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null) return null;

        if (reserva.Status != StatusReserva.CheckinRealizado)
            throw new InvalidOperationException("Check-in não foi realizado");

        var formaPagamento = Enum.Parse<FormaPagamento>(dto.FormaPagamento, true);

        var preco = await _precoRepository.ObterAtivoAsync(reserva.TipoVaga);
        decimal desconto = 0;

        if (formaPagamento == FormaPagamento.Pix || formaPagamento == FormaPagamento.Dinheiro)
        {
            var descontoPorDia = preco?.DescontoPixDinheiro ?? 0;
            desconto = descontoPorDia * reserva.QtdDias;
        }

        reserva.FormaPagamento = formaPagamento;
        reserva.DescontoAplicado = desconto;
        reserva.ValorFinal = reserva.ValorTotal - desconto;
        reserva.Pago = true;
        reserva.DataPagamento = DateTimeHelper.AgoraBrasilia();
        reserva.Status = StatusReserva.CheckoutRealizado;
        reserva.DataCheckout = DateTimeHelper.AgoraBrasilia();

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

    public async Task<CupomEntradaDto?> GerarCupomEntradaAsync(int id)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null) return null;

        var config = await _configuracaoRepository.ObterAsync();

        return new CupomEntradaDto
        {
            NomeEstacionamento = config?.NomeEstacionamento ?? "Estacionamento",
            Endereco = config?.Endereco,
            Contato = config?.Contato,
            Cnpj = config?.Cnpj,
            Numero = reserva.Id,
            PlacaVeiculo = reserva.PlacaVeiculo ?? "-",
            DataHoraEntrada = reserva.DataCheckin ?? reserva.DataEntrada,
            TipoVaga = reserva.TipoVaga.ToString(),
            QtdDias = reserva.QtdDias,
            DataSaidaPrevista = reserva.DataSaidaPrevista,
            ValorDiaria = reserva.ValorDiaria,
            ValorTotal = reserva.ValorTotal
        };
    }

    public async Task<CupomSaidaDto?> GerarCupomSaidaAsync(int id)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null || reserva.Status != StatusReserva.CheckoutRealizado) return null;

        var config = await _configuracaoRepository.ObterAsync();

        return new CupomSaidaDto
        {
            NomeEstacionamento = config?.NomeEstacionamento ?? "Estacionamento",
            Endereco = config?.Endereco,
            Contato = config?.Contato,
            Cnpj = config?.Cnpj,
            Numero = reserva.Id,
            PlacaVeiculo = reserva.PlacaVeiculo ?? "-",
            DataHoraEntrada = reserva.DataCheckin ?? reserva.DataEntrada,
            DataHoraSaida = reserva.DataCheckout ?? DateTimeHelper.AgoraBrasilia(),
            TipoVaga = reserva.TipoVaga.ToString(),
            QtdDias = reserva.QtdDias,
            ValorDiaria = reserva.ValorDiaria,
            ValorTotal = reserva.ValorTotal,
            DescontoAplicado = reserva.DescontoAplicado,
            ValorFinal = reserva.ValorFinal,
            FormaPagamento = reserva.FormaPagamento?.ToString() ?? "-"
        };
    }

    private async Task VerificarDisponibilidadeAsync(TipoVaga tipoVaga, DateTime dataEntrada, int qtdDias, int quantidadeVeiculos = 1)
    {
        var config = await _configuracaoRepository.ObterAsync()
            ?? throw new InvalidOperationException("Configuração do estacionamento não encontrada. Execute o seed primeiro.");

        var totalVagas = tipoVaga == TipoVaga.Coberta
            ? config.TotalVagasCoberta
            : config.TotalVagasDescoberta;

        for (int i = 0; i < qtdDias; i++)
        {
            var data = dataEntrada.Date.AddDays(i);
            var ocupadas = await _reservaRepository.ContarVagasOcupadasAsync(tipoVaga, data);

            if (ocupadas + quantidadeVeiculos > totalVagas)
                throw new InvalidOperationException($"Não há vagas {tipoVaga} suficientes para {quantidadeVeiculos} veículo(s) em {data:dd/MM/yyyy}");
        }
    }

    private static string NormalizarPlaca(string? placa) =>
        (placa ?? string.Empty).Trim().ToUpperInvariant();

    private static ReservaResponseDto MapToResponse(Reserva r) => new()
    {
        Id = r.Id,
        NomeCliente = r.NomeCliente,
        TelefoneCliente = r.TelefoneCliente,
        CpfCliente = r.CpfCliente,
        PlacaVeiculo = r.PlacaVeiculo,
        TipoVaga = r.TipoVaga.ToString(),
        DataEntrada = r.DataEntrada,
        QtdDias = r.QtdDias,
        DataSaidaPrevista = r.DataSaidaPrevista,
        ValorDiaria = r.ValorDiaria,
        ValorTotal = r.ValorTotal,
        DescontoAplicado = r.DescontoAplicado,
        ValorFinal = r.ValorFinal,
        FormaPagamento = r.FormaPagamento?.ToString(),
        Pago = r.Pago,
        Status = r.Status.ToString(),
        Origem = r.Origem.ToString(),
        DataCheckin = r.DataCheckin,
        DataCheckout = r.DataCheckout,
        Observacoes = r.Observacoes,
        DataCriacao = r.DataCriacao
    };
}
