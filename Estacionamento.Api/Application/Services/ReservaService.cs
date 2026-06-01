using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Helpers;
using Estacionamento.Api.Infrastructure.Repositories;
using static Estacionamento.Api.Helpers.PrecificacaoHelper;

namespace Estacionamento.Api.Application.Services;

public interface IReservaService
{
    Task<ReservaResponseDto> CriarOnlineAsync(CriarReservaOnlineDto dto);
    Task<ReservaLoteResponseDto> CriarOnlineLoteAsync(CriarReservaLoteOnlineDto dto);
    Task<ReservaResponseDto> CriarPresencialAsync(CriarReservaPresencialDto dto);
    Task<ReservaLoteResponseDto> CriarPresencialLoteAsync(CriarReservaLotePresencialDto dto);
    Task<IEnumerable<ReservaResponseDto>> ObterTodasAsync();
    Task<ReservaResponseDto?> ObterPorIdAsync(int id);
    Task<IEnumerable<ReservaResponseDto>> FiltrarAsync(FiltroReservaDto filtro);
    Task<ReservaResponseDto?> AssociarPlacaAsync(int id, AssociarPlacaDto dto);
    Task<ReservaResponseDto?> CheckinAsync(int id);
    Task<ReservaResponseDto?> CheckoutAsync(int id, CheckoutDto dto);
    Task<ReservaResponseDto?> CancelarAsync(int id);
    Task<CupomEntradaDto?> GerarCupomEntradaAsync(int id);
    Task<CupomSaidaDto?> GerarCupomSaidaAsync(int id);
    Task<ReservaResponseDto?> AtualizarAsync(int id, AtualizarReservaDto dto);
    Task<ReservaResponseDto?> AtualizarClienteAsync(int id, AtualizarReservaClienteDto dto);
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
        DateTimeHelper.ValidarPeriodoReserva(dto.DataEntrada, dto.DataSaidaPrevista);

        var tipoVaga = Enum.Parse<TipoVaga>(dto.TipoVaga, true);
        var preco = await _precoRepository.ObterAtivoAsync(tipoVaga)
            ?? throw new InvalidOperationException($"Nenhum preço ativo para vaga {dto.TipoVaga}");

        await VerificarDisponibilidadeAsync(tipoVaga, dto.DataEntrada, dto.QtdDias);

        var (valorTotal, qtdDiasCalculado, _) = PrecificacaoHelper.Calcular(preco, dto.DataEntrada, dto.DataSaidaPrevista);

        var reserva = new Reserva
        {
            NomeCliente = dto.NomeCliente,
            TelefoneCliente = dto.TelefoneCliente,
            CpfCliente = dto.CpfCliente,
            PlacaVeiculo = dto.PlacaVeiculo.ToUpper(),
            TipoVaga = tipoVaga,
            DataEntrada = dto.DataEntrada,
            QtdDias = qtdDiasCalculado,
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

    public async Task<ReservaLoteResponseDto> CriarOnlineLoteAsync(CriarReservaLoteOnlineDto dto)
    {
        // Valida placas antes de persistir qualquer registro
        for (int i = 0; i < dto.Carros.Count; i++)
        {
            var placa = dto.Carros[i].PlacaVeiculo;
            if (!string.IsNullOrEmpty(placa) && placa.Length > 10)
                throw new InvalidOperationException($"A placa do veículo {i + 1} não pode ter mais de 10 caracteres");
        }

        var criadas = new List<ReservaResponseDto>();

        foreach (var carro in dto.Carros)
        {
            var reservaDto = new CriarReservaOnlineDto
            {
                NomeCliente = dto.NomeCliente,
                TelefoneCliente = dto.TelefoneCliente,
                CpfCliente = dto.CpfCliente,
                PlacaVeiculo = carro.PlacaVeiculo ?? string.Empty,
                TipoVaga = carro.TipoVaga,
                DataEntrada = carro.DataEntrada,
                DataSaidaPrevista = carro.DataSaidaPrevista,
                QtdDias = carro.QtdDias,
                Observacoes = carro.Observacoes
            };

            var reserva = await CriarOnlineAsync(reservaDto);
            criadas.Add(reserva);
        }

        return new ReservaLoteResponseDto
        {
            Reservas = criadas,
            TotalReservas = criadas.Count,
            ValorTotalGeral = criadas.Sum(r => r.ValorTotal)
        };
    }

    public async Task<ReservaResponseDto> CriarPresencialAsync(CriarReservaPresencialDto dto)
    {
        var tipoVaga = Enum.Parse<TipoVaga>(dto.TipoVaga, true);
        var preco = await _precoRepository.ObterAtivoAsync(tipoVaga)
            ?? throw new InvalidOperationException($"Nenhum preço ativo para vaga {dto.TipoVaga}");

        await VerificarDisponibilidadeAsync(tipoVaga, dto.DataEntrada, dto.QtdDias);

        var (valorTotal, qtdDiasCalculado, _) = PrecificacaoHelper.Calcular(preco, dto.DataEntrada, dto.DataSaidaPrevista);

        var reserva = new Reserva
        {
            NomeCliente = dto.NomeCliente,
            TelefoneCliente = dto.TelefoneCliente,
            CpfCliente = dto.CpfCliente,
            PlacaVeiculo = dto.PlacaVeiculo.ToUpper(),
            TipoVaga = tipoVaga,
            DataEntrada = dto.DataEntrada,
            QtdDias = qtdDiasCalculado,
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

    public async Task<ReservaLoteResponseDto> CriarPresencialLoteAsync(CriarReservaLotePresencialDto dto)
    {
        for (int i = 0; i < dto.Carros.Count; i++)
        {
            var placa = dto.Carros[i].PlacaVeiculo;
            if (!string.IsNullOrEmpty(placa) && placa.Length > 10)
                throw new InvalidOperationException($"A placa do veículo {i + 1} não pode ter mais de 10 caracteres");
        }

        var criadas = new List<ReservaResponseDto>();

        foreach (var carro in dto.Carros)
        {
            var reservaDto = new CriarReservaPresencialDto
            {
                NomeCliente = dto.NomeCliente,
                TelefoneCliente = dto.TelefoneCliente,
                CpfCliente = dto.CpfCliente,
                PlacaVeiculo = carro.PlacaVeiculo,
                TipoVaga = carro.TipoVaga,
                DataEntrada = carro.DataEntrada,
                DataSaidaPrevista = carro.DataSaidaPrevista,
                QtdDias = carro.QtdDias,
                Observacoes = carro.Observacoes
            };

            var reserva = await CriarPresencialAsync(reservaDto);
            criadas.Add(reserva);
        }

        return new ReservaLoteResponseDto
        {
            Reservas = criadas,
            TotalReservas = criadas.Count,
            ValorTotalGeral = criadas.Sum(r => r.ValorTotal)
        };
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
            filtro.DataInicio, filtro.DataFim, status, tipoVaga, filtro.PlacaVeiculo);

        return reservas.Select(MapToResponse);
    }

    public async Task<ReservaResponseDto?> AssociarPlacaAsync(int id, AssociarPlacaDto dto)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null) return null;

        reserva.PlacaVeiculo = dto.PlacaVeiculo.ToUpper();

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
            if (descontoPorDia > 0 && preco != null)
            {
                // Recalcula qtdDias a partir das datas reais — não depende do valor armazenado
                var (_, qtdDiasEfetivo, _) = PrecificacaoHelper.Calcular(preco, reserva.DataEntrada, reserva.DataSaidaPrevista);
                desconto = descontoPorDia * qtdDiasEfetivo;
            }
        }

        // Calcula horas adicionais se o veículo saiu após DataSaidaPrevista
        var dataCheckout = DateTimeHelper.AgoraBrasilia();
        var valorHorasAdicionais = CalcularHorasAdicionais(reserva, preco, dataCheckout);

        reserva.FormaPagamento = formaPagamento;
        reserva.DescontoAplicado = desconto;
        reserva.ValorHorasAdicionais = valorHorasAdicionais;
        reserva.ValorFinal = reserva.ValorTotal - desconto + valorHorasAdicionais;
        reserva.Pago = true;
        reserva.DataPagamento = dataCheckout;
        reserva.Status = StatusReserva.CheckoutRealizado;
        reserva.DataCheckout = dataCheckout;

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
            ValorHorasAdicionais = reserva.ValorHorasAdicionais,
            ValorFinal = reserva.ValorFinal,
            FormaPagamento = reserva.FormaPagamento?.ToString() ?? "-"
        };
    }

    public async Task<ReservaResponseDto?> AtualizarAsync(int id, AtualizarReservaDto dto)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null) return null;

        if (reserva.Status != StatusReserva.Pendente && reserva.Status != StatusReserva.Confirmada)
            throw new InvalidOperationException("Só é possível alterar reservas com status Pendente ou Confirmada");

        var preco = await _precoRepository.ObterAtivoAsync(reserva.TipoVaga)
            ?? throw new InvalidOperationException($"Nenhum preço ativo para vaga {reserva.TipoVaga}");

        var (novoValorTotal, novoQtdDias, _) = PrecificacaoHelper.Calcular(preco, reserva.DataEntrada, dto.DataSaidaPrevista);

        reserva.QtdDias = novoQtdDias;
        reserva.DataSaidaPrevista = dto.DataSaidaPrevista;
        reserva.ValorDiaria = preco.ValorDiaria;
        reserva.ValorTotal = novoValorTotal;
        reserva.ValorFinal = novoValorTotal;

        await _reservaRepository.AtualizarAsync(reserva);
        return MapToResponse(reserva);
    }

    public async Task<ReservaResponseDto?> AtualizarClienteAsync(int id, AtualizarReservaClienteDto dto)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null) return null;

        var telefoneInformado = NormalizarTelefone(dto.TelefoneCliente);
        var telefoneReserva = NormalizarTelefone(reserva.TelefoneCliente);

        if (string.IsNullOrEmpty(telefoneInformado) || telefoneInformado != telefoneReserva)
            throw new InvalidOperationException("Telefone não confere com a reserva existente");

        var placaInformada = dto.PlacaVeiculo.Trim().ToUpper();
        var placaReserva = reserva.PlacaVeiculo?.Trim().ToUpper();

        if (string.IsNullOrEmpty(placaReserva) || placaInformada != placaReserva)
            throw new InvalidOperationException("Placa não confere com a reserva existente");

        return await AtualizarAsync(id, new AtualizarReservaDto { DataSaidaPrevista = dto.DataSaidaPrevista });
    }

    private static string NormalizarTelefone(string telefone) =>
        new string(telefone.Where(char.IsDigit).ToArray());

    private static decimal CalcularHorasAdicionais(Reserva reserva, Preco? preco, DateTime dataCheckout)
    {
        if (preco == null) return 0;

        var horasExtras = (dataCheckout - reserva.DataSaidaPrevista).TotalHours;
        if (horasExtras <= 0) return 0;

        if (horasExtras <= 6)
            return preco.ValorHorasAdicionaisAte6h;

        if (horasExtras <= 12)
            return preco.ValorHorasAdicionaisAte12h;

        // Acima de 12h: cobra uma diária cheia adicional
        return preco.ValorDiaria;
    }

    private async Task VerificarDisponibilidadeAsync(TipoVaga tipoVaga, DateTime dataEntrada, int qtdDias)
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

            if (ocupadas >= totalVagas)
                throw new InvalidOperationException($"Não há vagas {tipoVaga} disponíveis para {data:dd/MM/yyyy}");
        }
    }

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
        ValorHorasAdicionais = r.ValorHorasAdicionais,
        ValorFinal = r.ValorFinal,
        FormaPagamento = r.FormaPagamento?.ToString(),
        Pago = r.Pago,
        Status = r.Status.ToString(),
        Origem = r.Origem.ToString(),
        DataCheckin = r.DataCheckin,
        DataCheckout = r.DataCheckout,
        Observacoes = r.Observacoes,
        DataCriacao = r.DataCriacao,
        ConfirmadaPeloCliente = r.ConfirmadaPeloCliente,
        MensagemConfirmacaoEnviada = r.MensagemConfirmacaoEnviada,
        DataEnvioConfirmacao = r.DataEnvioConfirmacao
    };
}
