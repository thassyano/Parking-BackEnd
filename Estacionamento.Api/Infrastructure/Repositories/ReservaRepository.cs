using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Data;

namespace Estacionamento.Api.Infrastructure.Repositories;

public interface IReservaRepository
{
    Task<IEnumerable<Reserva>> ObterTodasAsync();
    Task<Reserva?> ObterPorIdAsync(int id);
    Task<IEnumerable<Reserva>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<IEnumerable<Reserva>> ObterFiltradoAsync(DateTime? dataInicio, DateTime? dataFim, StatusReserva? status, TipoVaga? tipoVaga);
    Task<int> ContarVagasOcupadasAsync(TipoVaga tipoVaga, DateTime data);
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
        StatusReserva? status, TipoVaga? tipoVaga)
    {
        var query = _context.Reservas.AsQueryable();

        if (dataInicio.HasValue)
            query = query.Where(r => r.DataEntrada >= ToUtc(dataInicio.Value));
        if (dataFim.HasValue)
            query = query.Where(r => r.DataEntrada <= ToUtc(dataFim.Value));
        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);
        if (tipoVaga.HasValue)
            query = query.Where(r => r.TipoVaga == tipoVaga.Value);

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
