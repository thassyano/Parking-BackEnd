using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Helpers;
using Estacionamento.Api.Infrastructure.Repositories;

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
    Task<PreviaCheckoutDto?> PreviaCheckoutAsync(int id, string formaPagamento, bool comTraslado);
    Task<ReservaResponseDto?> AlterarFormaPagamentoAsync(int id, AlterarFormaPagamentoDto dto);
    Task<ReservaResponseDto?> AtualizarDadosVeiculoAsync(int id, AtualizarDadosVeiculoDto dto);
    Task<ConflitoPlacaDto?> VerificarConflitoAsync(string placa, DateTime dataEntrada, DateTime dataSaidaPrevista);
    Task<ReservaResponseDto?> AlterarAdminAsync(int id, AlterarReservaAdminDto dto);
    Task<ReservaResponseDto?> AlterarClienteAsync(int id, AlterarReservaClienteDto dto);
}

public class ReservaService : IReservaService
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IPrecoRepository _precoRepository;
    private readonly IConfiguracaoRepository _configuracaoRepository;
    private readonly IBloqueioReservaRepository _bloqueioRepository;

    public ReservaService(
        IReservaRepository reservaRepository,
        IPrecoRepository precoRepository,
        IConfiguracaoRepository configuracaoRepository,
        IBloqueioReservaRepository bloqueioRepository)
    {
        _reservaRepository = reservaRepository;
        _precoRepository = precoRepository;
        _configuracaoRepository = configuracaoRepository;
        _bloqueioRepository = bloqueioRepository;
    }

    public async Task<ReservaResponseDto> CriarOnlineAsync(CriarReservaOnlineDto dto)
    {
        var tipoVaga = Enum.Parse<TipoVaga>(dto.TipoVaga, true);
        var preco = await _precoRepository.ObterAtivoAsync(tipoVaga)
            ?? throw new InvalidOperationException($"Nenhum preço ativo para vaga {dto.TipoVaga}");

        // Valor previsto: diarias cheias (blocos de 24h) + valor fixo da faixa do periodo parcial
        var estadia = CalculadoraEstadia.Calcular(
            dto.DataEntrada, dto.DataSaidaPrevista,
            preco.ValorHorasAdicionaisAte6h, preco.ValorHorasAdicionaisAte12h, preco.ValorDiaria);

        await VerificarBloqueioAsync(dto.DataEntrada, dto.DataSaidaPrevista);
        await VerificarDisponibilidadeAsync(tipoVaga, dto.DataEntrada, estadia.DiariasCobradas);

        var reserva = new Reserva
        {
            NomeCliente = dto.NomeCliente,
            TelefoneCliente = dto.TelefoneCliente,
            CpfCliente = dto.CpfCliente,
            PlacaVeiculo = dto.PlacaVeiculo.ToUpper(),
            MarcaVeiculo = NormalizarTexto(dto.MarcaVeiculo),
            ModeloVeiculo = NormalizarTexto(dto.ModeloVeiculo),
            CorVeiculo = NormalizarTexto(dto.CorVeiculo),
            TipoVaga = tipoVaga,
            DataEntrada = dto.DataEntrada,
            QtdDias = estadia.DiariasCompletas,
            DataSaidaPrevista = dto.DataSaidaPrevista,
            ValorDiaria = preco.ValorDiaria,
            ValorTotal = estadia.ValorEstadia,
            ValorFinal = estadia.ValorEstadia,
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

        // Confere o bloqueio de todos os carros antes de gravar qualquer um (tudo ou nada)
        foreach (var carro in dto.Carros)
            await VerificarBloqueioAsync(carro.DataEntrada, carro.DataSaidaPrevista);

        var criadas = new List<ReservaResponseDto>();

        foreach (var carro in dto.Carros)
        {
            var reservaDto = new CriarReservaOnlineDto
            {
                NomeCliente = dto.NomeCliente,
                TelefoneCliente = dto.TelefoneCliente,
                CpfCliente = dto.CpfCliente,
                PlacaVeiculo = carro.PlacaVeiculo ?? string.Empty,
                MarcaVeiculo = carro.MarcaVeiculo,
                ModeloVeiculo = carro.ModeloVeiculo,
                CorVeiculo = carro.CorVeiculo,
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

        // Valor previsto: diarias cheias (blocos de 24h) + valor fixo da faixa do periodo parcial
        var estadia = CalculadoraEstadia.Calcular(
            dto.DataEntrada, dto.DataSaidaPrevista,
            preco.ValorHorasAdicionaisAte6h, preco.ValorHorasAdicionaisAte12h, preco.ValorDiaria);

        await VerificarDisponibilidadeAsync(tipoVaga, dto.DataEntrada, estadia.DiariasCobradas);

        var reserva = new Reserva
        {
            NomeCliente = dto.NomeCliente,
            TelefoneCliente = dto.TelefoneCliente,
            CpfCliente = dto.CpfCliente,
            PlacaVeiculo = dto.PlacaVeiculo.ToUpper(),
            MarcaVeiculo = NormalizarTexto(dto.MarcaVeiculo),
            ModeloVeiculo = NormalizarTexto(dto.ModeloVeiculo),
            CorVeiculo = NormalizarTexto(dto.CorVeiculo),
            TipoVaga = tipoVaga,
            DataEntrada = dto.DataEntrada,
            QtdDias = estadia.DiariasCompletas,
            DataSaidaPrevista = dto.DataSaidaPrevista,
            ValorDiaria = preco.ValorDiaria,
            ValorTotal = estadia.ValorEstadia,
            ValorFinal = estadia.ValorEstadia,
            Origem = OrigemReserva.Presencial,
            Status = dto.ReservaFutura ? StatusReserva.Pendente : StatusReserva.CheckinRealizado,
            DataCheckin = dto.ReservaFutura ? null : DateTimeHelper.AgoraBrasilia(),
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
                MarcaVeiculo = carro.MarcaVeiculo,
                ModeloVeiculo = carro.ModeloVeiculo,
                CorVeiculo = carro.CorVeiculo,
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

        var formaPagamento = ParseFormaPagamento(dto.FormaPagamento);
        var saida = DateTimeHelper.AgoraBrasilia();
        var calculo = await CalcularCheckoutAsync(reserva, formaPagamento, dto.ComTraslado, saida);

        reserva.ValorDiaria = calculo.ValorDiaria;
        reserva.QtdDias = calculo.Previa.DiariasCompletas;
        reserva.ValorTotal = calculo.Previa.ValorTotal;
        reserva.DescontoAplicado = calculo.Previa.Desconto;
        reserva.ComTraslado = calculo.Previa.ComTraslado;
        reserva.ValorTraslado = calculo.Previa.ValorTraslado;
        reserva.ValorFinal = calculo.Previa.ValorFinal;
        reserva.FormaPagamento = formaPagamento;
        reserva.Pago = true;
        reserva.DataPagamento = saida;
        reserva.Status = StatusReserva.CheckoutRealizado;
        reserva.DataCheckout = saida;

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

        var entrada = reserva.DataCheckin ?? reserva.DataEntrada;
        var saida = reserva.DataCheckout ?? DateTimeHelper.AgoraBrasilia();

        var preco = await _precoRepository.ObterAtivoAsync(reserva.TipoVaga);
        var adicionais = preco == null ? 0m : CalculadoraEstadia.Calcular(
            entrada, saida, preco.ValorHorasAdicionaisAte6h, preco.ValorHorasAdicionaisAte12h, preco.ValorDiaria).ValorHorasAdicionais;

        return new CupomSaidaDto
        {
            NomeEstacionamento = config?.NomeEstacionamento ?? "Estacionamento",
            Endereco = config?.Endereco,
            Contato = config?.Contato,
            Cnpj = config?.Cnpj,
            Numero = reserva.Id,
            PlacaVeiculo = reserva.PlacaVeiculo ?? "-",
            DataHoraEntrada = entrada,
            DataSaidaPrevista = reserva.DataSaidaPrevista,
            DataHoraSaida = saida,
            TipoVaga = reserva.TipoVaga.ToString(),
            QtdDias = reserva.QtdDias,
            Permanencia = CalculadoraEstadia.FormatarPermanencia(saida - entrada),
            ValorDiaria = reserva.ValorDiaria,
            ValorTotal = reserva.ValorTotal,
            DescontoAplicado = reserva.DescontoAplicado,
            ValorHorasAdicionais = adicionais,
            ComTraslado = reserva.ComTraslado,
            ValorTraslado = reserva.ValorTraslado,
            ValorFinal = reserva.ValorFinal,
            FormaPagamento = reserva.FormaPagamento?.ToString() ?? "-"
        };
    }


    public async Task<PreviaCheckoutDto?> PreviaCheckoutAsync(int id, string formaPagamento, bool comTraslado)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null) return null;

        if (reserva.Status != StatusReserva.CheckinRealizado)
            throw new InvalidOperationException("Só é possível calcular o check-out de uma reserva com check-in realizado");

        var forma = ParseFormaPagamento(formaPagamento);
        var calculo = await CalcularCheckoutAsync(reserva, forma, comTraslado, DateTimeHelper.AgoraBrasilia());
        return calculo.Previa;
    }

    public async Task<ReservaResponseDto?> AlterarFormaPagamentoAsync(int id, AlterarFormaPagamentoDto dto)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null) return null;

        if (reserva.Status != StatusReserva.CheckoutRealizado)
            throw new InvalidOperationException("A forma de pagamento só pode ser corrigida depois do check-out");

        var forma = ParseFormaPagamento(dto.FormaPagamento);
        var config = await _configuracaoRepository.ObterAsync();

        var entrada = reserva.DataCheckin ?? reserva.DataEntrada;
        var saida = reserva.DataCheckout ?? DateTimeHelper.AgoraBrasilia();

        // Mantém o valor da estadia já cobrado (sem traslado); só o desconto e o traslado são refeitos.
        var valorEstadia = reserva.ValorTotal - reserva.ValorTraslado;

        // Quantidade de diárias que dão direito ao desconto depende só do tempo de permanência.
        var diariasParaDesconto = CalculadoraEstadia.Calcular(entrada, saida, 0m, 0m, 0m).DiariasParaDesconto;

        decimal desconto = 0m;
        if (forma == FormaPagamento.Pix || forma == FormaPagamento.Dinheiro)
        {
            var descontoPorDiaria = await ObterDescontoPorDiariaAsync(reserva, diariasParaDesconto);
            desconto = descontoPorDiaria * diariasParaDesconto;
        }

        var comTraslado = dto.ComTraslado ?? reserva.ComTraslado;
        var valorTraslado = reserva.ValorTraslado;
        if (dto.ComTraslado.HasValue && dto.ComTraslado.Value != reserva.ComTraslado)
            valorTraslado = CalcularTraslado(comTraslado, saida - entrada, config);

        reserva.FormaPagamento = forma;
        reserva.DescontoAplicado = desconto;
        reserva.ComTraslado = comTraslado;
        reserva.ValorTraslado = comTraslado ? valorTraslado : 0m;
        reserva.ValorTotal = valorEstadia + reserva.ValorTraslado;
        reserva.ValorFinal = valorEstadia - desconto + reserva.ValorTraslado;

        await _reservaRepository.AtualizarAsync(reserva);
        return MapToResponse(reserva);
    }

    public async Task<ReservaResponseDto?> AtualizarDadosVeiculoAsync(int id, AtualizarDadosVeiculoDto dto)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null) return null;

        reserva.MarcaVeiculo = NormalizarTexto(dto.MarcaVeiculo);
        reserva.ModeloVeiculo = NormalizarTexto(dto.ModeloVeiculo);
        reserva.CorVeiculo = NormalizarTexto(dto.CorVeiculo);

        await _reservaRepository.AtualizarAsync(reserva);
        return MapToResponse(reserva);
    }

    public async Task<ConflitoPlacaDto?> VerificarConflitoAsync(string placa, DateTime dataEntrada, DateTime dataSaidaPrevista)
    {
        if (string.IsNullOrWhiteSpace(placa)) return null;

        var existente = await _reservaRepository.ObterConflitoPorPlacaAsync(placa, dataEntrada, dataSaidaPrevista);
        if (existente == null) return null;

        return new ConflitoPlacaDto
        {
            Id = existente.Id,
            PlacaVeiculo = existente.PlacaVeiculo ?? string.Empty,
            DataEntrada = existente.DataEntrada,
            DataSaidaPrevista = existente.DataSaidaPrevista,
            Status = existente.Status.ToString(),
            TipoVaga = existente.TipoVaga.ToString(),
            // Reserva que já deu entrada no pátio não é sobrescrita.
            PodeAtualizar = PodeAlterarPeriodo(existente)
        };
    }

    public async Task<ReservaResponseDto?> AlterarAdminAsync(int id, AlterarReservaAdminDto dto)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null) return null;

        if (!PodeAlterarPeriodo(reserva))
            throw new InvalidOperationException("Esta reserva não pode mais ser alterada no status atual");

        var entrada = dto.DataEntrada ?? reserva.DataEntrada;
        var tipoVaga = string.IsNullOrWhiteSpace(dto.TipoVaga)
            ? reserva.TipoVaga
            : ParseTipoVaga(dto.TipoVaga);

        await ReaplicarPeriodoAsync(reserva, entrada, dto.DataSaidaPrevista, tipoVaga);
        return MapToResponse(reserva);
    }

    public async Task<ReservaResponseDto?> AlterarClienteAsync(int id, AlterarReservaClienteDto dto)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null) return null;

        // O cliente se identifica com placa + telefone (sem login).
        var placaConfere = string.Equals(
            reserva.PlacaVeiculo?.Trim(), dto.PlacaVeiculo.Trim(), StringComparison.OrdinalIgnoreCase);
        var telefoneConfere = ApenasDigitos(reserva.TelefoneCliente) == ApenasDigitos(dto.TelefoneCliente)
            && ApenasDigitos(dto.TelefoneCliente).Length > 0;

        if (!placaConfere || !telefoneConfere)
            throw new InvalidOperationException("Não foi possível confirmar seus dados. Confira a placa e o telefone.");

        if (!PodeAlterarPeriodo(reserva) || reserva.Status == StatusReserva.CheckinRealizado)
            throw new InvalidOperationException("Esta reserva não pode mais ser alterada");

        var entrada = dto.DataEntrada ?? reserva.DataEntrada;
        var tipoVaga = string.IsNullOrWhiteSpace(dto.TipoVaga)
            ? reserva.TipoVaga
            : ParseTipoVaga(dto.TipoVaga);

        var periodoMudou = entrada != reserva.DataEntrada || dto.DataSaidaPrevista != reserva.DataSaidaPrevista;
        if (periodoMudou)
            await VerificarBloqueioAsync(entrada, dto.DataSaidaPrevista);

        await ReaplicarPeriodoAsync(reserva, entrada, dto.DataSaidaPrevista, tipoVaga);
        return MapToResponse(reserva);
    }

    // ---------- Regras internas ----------

    private sealed record CalculoCheckout(PreviaCheckoutDto Previa, decimal ValorDiaria);

    /// <summary>
    /// Calcula o check-out pelo tempo real de permanência. Usado pela prévia e pelo check-out de verdade,
    /// para que os dois sempre mostrem exatamente o mesmo valor.
    /// </summary>
    private async Task<CalculoCheckout> CalcularCheckoutAsync(
        Reserva reserva, FormaPagamento forma, bool comTraslado, DateTime saida)
    {
        var preco = await _precoRepository.ObterAtivoAsync(reserva.TipoVaga)
            ?? throw new InvalidOperationException($"Nenhum preço ativo para vaga {reserva.TipoVaga}");
        var config = await _configuracaoRepository.ObterAsync();

        var entrada = reserva.DataCheckin ?? reserva.DataEntrada;

        // Cobrança pelo tempo REAL: cada 24h = 1 diária; o resto entra em faixa (até 6h, até 12h, 12h ou mais = diária cheia)
        var estadia = CalculadoraEstadia.Calcular(
            entrada, saida, preco.ValorHorasAdicionaisAte6h, preco.ValorHorasAdicionaisAte12h, preco.ValorDiaria);

        // Desconto Pix/Dinheiro por diária cheia cobrada (inclui o parcial a partir de 12h)
        decimal desconto = 0m;
        if (forma == FormaPagamento.Pix || forma == FormaPagamento.Dinheiro)
            desconto = preco.DescontoPixDinheiro * estadia.DiariasParaDesconto;

        var gratis = comTraslado && TrasladoGratis(estadia.Permanencia, config);
        var valorTraslado = CalcularTraslado(comTraslado, estadia.Permanencia, config);

        var previa = new PreviaCheckoutDto
        {
            DataEntrada = entrada,
            DataSaida = saida,
            Permanencia = CalculadoraEstadia.FormatarPermanencia(estadia.Permanencia),
            FormaPagamento = forma.ToString(),
            DiariasCompletas = estadia.DiariasCompletas,
            DiariasCobradas = estadia.DiariasParaDesconto,
            ValorEstadia = estadia.ValorEstadia,
            ValorHorasAdicionais = estadia.ValorHorasAdicionais,
            DescontoPixDinheiroPorDia = preco.DescontoPixDinheiro,
            Desconto = desconto,
            ComTraslado = comTraslado,
            TrasladoGratis = gratis,
            ValorTraslado = valorTraslado,
            ValorTotal = estadia.ValorEstadia + valorTraslado,
            ValorFinal = estadia.ValorEstadia - desconto + valorTraslado
        };

        return new CalculoCheckout(previa, preco.ValorDiaria);
    }

    /// <summary>Traslado é grátis a partir de N diárias de 24h, ou seja, N x 24 horas de permanência (padrão: 48h).</summary>
    private static bool TrasladoGratis(TimeSpan permanencia, ConfiguracaoEstacionamento? config)
    {
        var diasGratis = config?.TrasladoGratisAPartirDiarias ?? 2;
        return permanencia >= TimeSpan.FromHours(diasGratis * 24d);
    }

    private static decimal CalcularTraslado(bool comTraslado, TimeSpan permanencia, ConfiguracaoEstacionamento? config)
    {
        if (!comTraslado) return 0m;
        return TrasladoGratis(permanencia, config) ? 0m : (config?.ValorTraslado ?? 0m);
    }

    /// <summary>
    /// Desconto por diária a usar ao corrigir o pagamento: o que já foi aplicado antes (mantém o preço da época),
    /// ou o desconto do preço ativo quando antes não havia desconto.
    /// </summary>
    private async Task<decimal> ObterDescontoPorDiariaAsync(Reserva reserva, int diariasParaDesconto)
    {
        if (reserva.DescontoAplicado > 0m && diariasParaDesconto > 0)
            return reserva.DescontoAplicado / diariasParaDesconto;

        var preco = await _precoRepository.ObterAtivoAsync(reserva.TipoVaga);
        return preco?.DescontoPixDinheiro ?? 0m;
    }

    private async Task VerificarBloqueioAsync(DateTime inicio, DateTime fim)
    {
        var bloqueio = await _bloqueioRepository.ObterQueCruzaAsync(inicio, fim);
        if (bloqueio != null)
            throw new InvalidOperationException(BloqueioMensagem.Montar(bloqueio));
    }

    private static bool PodeAlterarPeriodo(Reserva r) =>
        r.Status is StatusReserva.Pendente or StatusReserva.Confirmada;

    /// <summary>Aplica novo período/vaga a uma reserva ainda não iniciada, refazendo disponibilidade e valores.</summary>
    private async Task ReaplicarPeriodoAsync(Reserva reserva, DateTime entrada, DateTime saidaPrevista, TipoVaga tipoVaga)
    {
        if (saidaPrevista <= entrada)
            throw new InvalidOperationException("A saída deve ser depois da entrada");

        var preco = await _precoRepository.ObterAtivoAsync(tipoVaga)
            ?? throw new InvalidOperationException($"Nenhum preço ativo para vaga {tipoVaga}");

        var estadia = CalculadoraEstadia.Calcular(
            entrada, saidaPrevista,
            preco.ValorHorasAdicionaisAte6h, preco.ValorHorasAdicionaisAte12h, preco.ValorDiaria);

        // A própria reserva não conta como vaga ocupada na verificação
        await VerificarDisponibilidadeAsync(tipoVaga, entrada, estadia.DiariasCobradas, reserva.Id);

        reserva.DataEntrada = entrada;
        reserva.DataSaidaPrevista = saidaPrevista;
        reserva.TipoVaga = tipoVaga;
        reserva.QtdDias = estadia.DiariasCompletas;
        reserva.ValorDiaria = preco.ValorDiaria;
        reserva.ValorTotal = estadia.ValorEstadia;
        reserva.ValorFinal = estadia.ValorEstadia;

        await _reservaRepository.AtualizarAsync(reserva);
    }

    private static FormaPagamento ParseFormaPagamento(string valor) =>
        Enum.TryParse<FormaPagamento>(valor, true, out var forma)
            ? forma
            : throw new InvalidOperationException("Forma de pagamento inválida");

    private static TipoVaga ParseTipoVaga(string valor) =>
        Enum.TryParse<TipoVaga>(valor, true, out var tipo)
            ? tipo
            : throw new InvalidOperationException("Tipo de vaga inválido");

    private static string? NormalizarTexto(string? valor)
    {
        var texto = valor?.Trim();
        return string.IsNullOrEmpty(texto) ? null : texto;
    }

    private static string ApenasDigitos(string? valor) =>
        new((valor ?? string.Empty).Where(char.IsDigit).ToArray());

    private async Task VerificarDisponibilidadeAsync(TipoVaga tipoVaga, DateTime dataEntrada, int qtdDias, int? ignorarReservaId = null)
    {
        var config = await _configuracaoRepository.ObterAsync()
            ?? throw new InvalidOperationException("Configuração do estacionamento não encontrada. Execute o seed primeiro.");

        var totalVagas = tipoVaga == TipoVaga.Coberta
            ? config.TotalVagasCoberta
            : config.TotalVagasDescoberta;

        for (int i = 0; i < qtdDias; i++)
        {
            var data = dataEntrada.Date.AddDays(i);
            var ocupadas = await _reservaRepository.ContarVagasOcupadasAsync(tipoVaga, data, ignorarReservaId);

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
        MarcaVeiculo = r.MarcaVeiculo,
        ModeloVeiculo = r.ModeloVeiculo,
        CorVeiculo = r.CorVeiculo,
        TipoVaga = r.TipoVaga.ToString(),
        DataEntrada = r.DataEntrada,
        QtdDias = r.QtdDias,
        DataSaidaPrevista = r.DataSaidaPrevista,
        ValorDiaria = r.ValorDiaria,
        ValorTotal = r.ValorTotal,
        DescontoAplicado = r.DescontoAplicado,
        ValorFinal = r.ValorFinal,
        ComTraslado = r.ComTraslado,
        ValorTraslado = r.ValorTraslado,
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
