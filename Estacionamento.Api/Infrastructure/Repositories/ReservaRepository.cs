using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Data;

namespace Estacionamento.Api.Infrastructure.Repositories;

public interface IReservaRepository
{
    Task<IEnumerable<Reserva>> ObterTodasAsync();
    Task<Reserva?> ObterPorIdAsync(int id);
    Task<IEnumerable<Reserva>> ObterPorClienteAsync(int clienteId);
    Task<IEnumerable<Reserva>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<IEnumerable<Reserva>> ObterFiltradoAsync(DateTime? dataInicio, DateTime? dataFim, StatusReserva? status, TipoVaga? tipoVaga, int? clienteId);
    Task<int> ContarVagasOcupadasAsync(TipoVaga tipoVaga, DateTime data);
    Task<IEnumerable<Reserva>> ObterParaConfirmacaoAsync(int horasAntecedencia);
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

    public async Task<IEnumerable<Reserva>> ObterTodasAsync()
    {
        return await _context.Reservas
            .Include(r => r.Cliente)
            .Include(r => r.Pagamentos)
            .OrderByDescending(r => r.DataCriacao)
            .ToListAsync();
    }

    public async Task<Reserva?> ObterPorIdAsync(int id)
    {
        return await _context.Reservas
            .Include(r => r.Cliente)
            .Include(r => r.Pagamentos)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Reserva>> ObterPorClienteAsync(int clienteId)
    {
        return await _context.Reservas
            .Include(r => r.Cliente)
            .Where(r => r.ClienteId == clienteId)
            .OrderByDescending(r => r.DataReserva)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reserva>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        return await _context.Reservas
            .Include(r => r.Cliente)
            .Include(r => r.Pagamentos)
            .Where(r => r.DataReserva >= dataInicio && r.DataReserva <= dataFim)
            .OrderBy(r => r.DataReserva)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reserva>> ObterFiltradoAsync(
        DateTime? dataInicio, DateTime? dataFim,
        StatusReserva? status, TipoVaga? tipoVaga, int? clienteId)
    {
        var query = _context.Reservas
            .Include(r => r.Cliente)
            .Include(r => r.Pagamentos)
            .AsQueryable();

        if (dataInicio.HasValue)
            query = query.Where(r => r.DataReserva >= dataInicio.Value);
        if (dataFim.HasValue)
            query = query.Where(r => r.DataReserva <= dataFim.Value);
        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);
        if (tipoVaga.HasValue)
            query = query.Where(r => r.TipoVaga == tipoVaga.Value);
        if (clienteId.HasValue)
            query = query.Where(r => r.ClienteId == clienteId.Value);

        return await query
            .OrderByDescending(r => r.DataReserva)
            .ToListAsync();
    }

    public async Task<int> ContarVagasOcupadasAsync(TipoVaga tipoVaga, DateTime data)
    {
        var statusAtivos = new[]
        {
            StatusReserva.Pendente,
            StatusReserva.Confirmada,
            StatusReserva.CheckinRealizado
        };

        return await _context.Reservas
            .Where(r => r.TipoVaga == tipoVaga
                && statusAtivos.Contains(r.Status)
                && r.DataReserva <= data
                && r.DataFim >= data)
            .SumAsync(r => r.QtdDias > 0 ? 1 : 0);
    }

    public async Task<IEnumerable<Reserva>> ObterParaConfirmacaoAsync(int horasAntecedencia)
    {
        var limite = DateTime.UtcNow.AddHours(horasAntecedencia);

        return await _context.Reservas
            .Include(r => r.Cliente)
            .Where(r => r.Status == StatusReserva.Pendente
                && !r.ConfirmacaoEnviada
                && r.DataReserva <= limite
                && r.DataReserva > DateTime.UtcNow)
            .ToListAsync();
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
