using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Data;

namespace Estacionamento.Api.Infrastructure.Repositories;

public interface IReservaRepository
{
    Task<IEnumerable<Reserva>> ObterTodasAsync();
    Task<Reserva?> ObterPorIdAsync(int id);
    Task<Reserva?> ObterPorTokenAsync(Guid token);
    Task<IEnumerable<Reserva>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<IEnumerable<Reserva>> ObterFiltradoAsync(DateTime? dataInicio, DateTime? dataFim, StatusReserva? status, TipoVaga? tipoVaga, string? placaVeiculo);
    Task<int> ContarVagasOcupadasAsync(TipoVaga tipoVaga, DateTime data);
    Task<IEnumerable<Reserva>> ObterParaEnvioConfirmacaoAsync(int horasAntecedencia);
    Task<IEnumerable<Reserva>> ObterParaCancelamentoAutomaticoAsync();
    Task<Reserva> CriarAsync(Reserva reserva);
    Task<Reserva> AtualizarAsync(Reserva reserva);
}

public class ReservaRepository : IReservaRepository
{
    private readonly AppDbContext _context;

    public ReservaRepository(AppDbContext context)
    {
        _context = context;
    }

    private static DateTime ToUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt.ToUniversalTime();

    public async Task<IEnumerable<Reserva>> ObterTodasAsync()
    {
        return await _context.Reservas
            .OrderByDescending(r => r.DataCriacao)
            .ToListAsync();
    }

    public async Task<Reserva?> ObterPorIdAsync(int id)
    {
        return await _context.Reservas.FindAsync(id);
    }

    public async Task<Reserva?> ObterPorTokenAsync(Guid token)
    {
        return await _context.Reservas.FirstOrDefaultAsync(r => r.ConfirmacaoToken == token);
    }

    public async Task<IEnumerable<Reserva>> ObterParaEnvioConfirmacaoAsync(int horasAntecedencia)
    {
        var agora = Helpers.DateTimeHelper.AgoraBrasilia();
        var limite = agora.AddHours(horasAntecedencia);
        var statusAtivos = new[] { StatusReserva.Pendente, StatusReserva.Confirmada };

        return await _context.Reservas
            .Where(r => r.DataEntrada > agora
                && r.DataEntrada <= limite
                && !r.MensagemConfirmacaoEnviada
                && statusAtivos.Contains(r.Status))
            .ToListAsync();
    }

    public async Task<IEnumerable<Reserva>> ObterParaCancelamentoAutomaticoAsync()
    {
        var hoje = Helpers.DateTimeHelper.AgoraBrasilia().Date;
        var statusAtivos = new[] { StatusReserva.Pendente, StatusReserva.Confirmada };

        return await _context.Reservas
            .Where(r => r.DataEntrada < hoje.AddDays(1)
                && !r.ConfirmadaPeloCliente
                && r.MensagemConfirmacaoEnviada
                && statusAtivos.Contains(r.Status))
            .ToListAsync();
    }

    public async Task<IEnumerable<Reserva>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        var inicio = ToUtc(dataInicio);
        var fim = ToUtc(dataFim);

        return await _context.Reservas
            .Where(r => r.DataEntrada >= inicio && r.DataEntrada <= fim)
            .OrderBy(r => r.DataEntrada)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reserva>> ObterFiltradoAsync(
        DateTime? dataInicio, DateTime? dataFim,
        StatusReserva? status, TipoVaga? tipoVaga, string? placaVeiculo)
    {
        var query = _context.Reservas.AsQueryable();

        if (dataInicio.HasValue && dataFim.HasValue)
        {
            query = query.Where(r => r.DataEntrada >= ToUtc(dataInicio.Value)
                                     && r.DataEntrada <= ToUtc(dataFim.Value.Date.AddDays(1).AddTicks(-1)));
        }
        else if (dataInicio.HasValue)
        {
            var inicio = ToUtc(dataInicio.Value.Date);
            var fim = ToUtc(dataInicio.Value.Date.AddDays(1).AddTicks(-1));
            query = query.Where(r => r.DataEntrada >= inicio && r.DataEntrada <= fim);
        }
        else if (dataFim.HasValue)
        {
            query = query.Where(r => r.DataEntrada <= ToUtc(dataFim.Value.Date.AddDays(1).AddTicks(-1)));
        }

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);
        if (tipoVaga.HasValue)
            query = query.Where(r => r.TipoVaga == tipoVaga.Value);
        if (!string.IsNullOrWhiteSpace(placaVeiculo))
        {
            var placa = placaVeiculo.Trim().ToUpper();
            query = query.Where(r => r.PlacaVeiculo != null && r.PlacaVeiculo == placa);
        }

        return await query
            .OrderByDescending(r => r.DataEntrada)
            .ToListAsync();

    }

    public async Task<int> ContarVagasOcupadasAsync(TipoVaga tipoVaga, DateTime data)
    {
        var dataUtc = ToUtc(data.Date);

        var statusAtivos = new[]
        {
            StatusReserva.Pendente,
            StatusReserva.Confirmada,
            StatusReserva.CheckinRealizado
        };

        return await _context.Reservas
            .CountAsync(r => r.TipoVaga == tipoVaga
                && statusAtivos.Contains(r.Status)
                && r.DataEntrada <= dataUtc
                && r.DataSaidaPrevista >= dataUtc);
    }

    public async Task<Reserva> CriarAsync(Reserva reserva)
    {
        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
        return reserva;
    }

    public async Task<Reserva> AtualizarAsync(Reserva reserva)
    {
        _context.Reservas.Update(reserva);
        await _context.SaveChangesAsync();
        return reserva;
    }
}
