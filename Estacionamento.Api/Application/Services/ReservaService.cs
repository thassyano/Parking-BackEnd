using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Helpers;
using Estacionamento.Api.Infrastructure.Repositories;
using System.Text.RegularExpressions;

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
        var nomeCliente = NormalizarNomeCliente(dto.NomeCliente);
        var telefoneCliente = NormalizarTelefoneCliente(dto.TelefoneCliente);
        var placaVeiculo = NormalizarPlacaVeiculo(dto.PlacaVeiculo);
        var qtdDiasCalculado = CalcularQtdDias(dto.DataEntrada, dto.DataSaidaPrevista);
        var tipoVaga = Enum.Parse<TipoVaga>(dto.TipoVaga, true);
        var preco = await _precoRepository.ObterAtivoAsync(tipoVaga)
            ?? throw new InvalidOperationException($"Nenhum preco ativo para vaga {dto.TipoVaga}");

        await VerificarDisponibilidadeAsync(tipoVaga, dto.DataEntrada, qtdDiasCalculado);

        var valorTotal = preco.ValorDiaria * qtdDiasCalculado;

        var reserva = new Reserva
        {
            NomeCliente = nomeCliente,
            TelefoneCliente = telefoneCliente,
            CpfCliente = NormalizarCampoOpcional(dto.CpfCliente),
            PlacaVeiculo = placaVeiculo,
            TipoVaga = tipoVaga,
            DataEntrada = dto.DataEntrada,
            QtdDias = qtdDiasCalculado,
            DataSaidaPrevista = dto.DataSaidaPrevista,
            ValorDiaria = preco.ValorDiaria,
            ValorTotal = valorTotal,
            ValorFinal = valorTotal,
            Origem = OrigemReserva.Online,
            Status = StatusReserva.Pendente,
            Observacoes = NormalizarCampoOpcional(dto.Observacoes)
        };

        var criada = await _reservaRepository.CriarAsync(reserva);
        return MapToResponse(criada);
    }

    public async Task<ReservaLoteResponseDto> CriarOnlineLoteAsync(CriarReservaLoteOnlineDto dto)
    {
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
        var nomeCliente = NormalizarNomeCliente(dto.NomeCliente);
        var telefoneCliente = NormalizarTelefoneCliente(dto.TelefoneCliente);
        var placaVeiculo = NormalizarPlacaVeiculo(dto.PlacaVeiculo);
        var qtdDiasCalculado = CalcularQtdDias(dto.DataEntrada, dto.DataSaidaPrevista);
        var tipoVaga = Enum.Parse<TipoVaga>(dto.TipoVaga, true);
        var preco = await _precoRepository.ObterAtivoAsync(tipoVaga)
            ?? throw new InvalidOperationException($"Nenhum preco ativo para vaga {dto.TipoVaga}");

        await VerificarDisponibilidadeAsync(tipoVaga, dto.DataEntrada, qtdDiasCalculado);

        var valorTotal = preco.ValorDiaria * qtdDiasCalculado;

        var reserva = new Reserva
        {
            NomeCliente = nomeCliente,
            TelefoneCliente = telefoneCliente,
            CpfCliente = NormalizarCampoOpcional(dto.CpfCliente),
            PlacaVeiculo = placaVeiculo,
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
            Observacoes = NormalizarCampoOpcional(dto.Observacoes)
        };

        var criada = await _reservaRepository.CriarAsync(reserva);
        return MapToResponse(criada);
    }

    public async Task<ReservaLoteResponseDto> CriarPresencialLoteAsync(CriarReservaLotePresencialDto dto)
    {
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

        reserva.PlacaVeiculo = NormalizarPlacaVeiculo(dto.PlacaVeiculo);

        await _reservaRepository.AtualizarAsync(reserva);
        return MapToResponse(reserva);
    }

    public async Task<ReservaResponseDto?> CheckinAsync(int id)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(id);
        if (reserva == null) return null;

        if (string.IsNullOrEmpty(reserva.PlacaVeiculo))
            throw new InvalidOperationException("Associe a placa do veiculo antes de fazer check-in");

        if (reserva.Status != StatusReserva.Pendente && reserva.Status != StatusReserva.Confirmada)
            throw new InvalidOperationException("Reserva nao pode fazer check-in no status atual");

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
            throw new InvalidOperationException("Check-in nao foi realizado");

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
            throw new InvalidOperationException("Nao e possivel cancelar uma reserva ja finalizada");

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

    private async Task VerificarDisponibilidadeAsync(TipoVaga tipoVaga, DateTime dataEntrada, int qtdDias)
    {
        var config = await _configuracaoRepository.ObterAsync()
            ?? throw new InvalidOperationException("Configuracao do estacionamento nao encontrada. Execute o seed primeiro.");

        var totalVagas = tipoVaga == TipoVaga.Coberta
            ? config.TotalVagasCoberta
            : config.TotalVagasDescoberta;

        for (int i = 0; i < qtdDias; i++)
        {
            var data = dataEntrada.Date.AddDays(i);
            var ocupadas = await _reservaRepository.ContarVagasOcupadasAsync(tipoVaga, data);

            if (ocupadas >= totalVagas)
                throw new InvalidOperationException($"Nao ha vagas {tipoVaga} disponiveis para {data:dd/MM/yyyy}");
        }
    }

    private static int CalcularQtdDias(DateTime dataEntrada, DateTime dataSaidaPrevista)
    {
        if (dataSaidaPrevista <= dataEntrada)
            throw new InvalidOperationException("A data e hora de saida prevista devem ser posteriores a entrada");

        var totalDias = (dataSaidaPrevista - dataEntrada).TotalDays;
        return Math.Max(1, (int)Math.Ceiling(totalDias));
    }

    private static string NormalizarNomeCliente(string nomeCliente)
    {
        var nomeNormalizado = Regex.Replace(nomeCliente.Trim(), @"\s+", " ");

        if (!Regex.IsMatch(nomeNormalizado, @"^[\p{L}\s]+$"))
            throw new InvalidOperationException("O nome do cliente deve conter apenas letras");

        return nomeNormalizado;
    }

    private static string NormalizarTelefoneCliente(string telefoneCliente)
    {
        var digitos = new string(telefoneCliente.Where(char.IsDigit).ToArray());

        if (digitos.Length != 11)
            throw new InvalidOperationException("Telefone deve estar no formato (00) 000000000");

        return $"({digitos[..2]}) {digitos[2..]}";
    }

    private static string NormalizarPlacaVeiculo(string placaVeiculo)
    {
        var placaNormalizada = placaVeiculo.Trim().ToUpperInvariant();

        if (placaNormalizada.Length is < 1 or > 7)
            throw new InvalidOperationException("A placa do veiculo deve ter no maximo 7 caracteres");

        if (!Regex.IsMatch(placaNormalizada, @"^[A-Z0-9]+$"))
            throw new InvalidOperationException("A placa do veiculo deve conter apenas caracteres alfanumericos");

        return placaNormalizada;
    }

    private static string? NormalizarCampoOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
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
