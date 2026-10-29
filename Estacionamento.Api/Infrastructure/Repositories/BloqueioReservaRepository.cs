using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Helpers;
using Estacionamento.Api.Infrastructure.Data;

namespace Estacionamento.Api.Infrastructure.Repositories;

public interface IBloqueioReservaRepository
{
    /// <summary>Bloqueios ativos que ainda não terminaram (mais próximos primeiro).</summary>
    Task<IEnumerable<BloqueioReserva>> ObterAtivosAsync();
    Task<BloqueioReserva?> ObterPorIdAsync(int id);

    /// <summary>Primeiro bloqueio ativo que cruza o período [inicio, fim], ou null.</summary>
    Task<BloqueioReserva?> ObterQueCruzaAsync(DateTime inicio, DateTime fim);

    Task<BloqueioReserva> CriarAsync(BloqueioReserva bloqueio);
    Task<BloqueioReserva> AtualizarAsync(BloqueioReserva bloqueio);
}

public class BloqueioReservaRepository : IBloqueioReservaRepository
{
    private readonly AppDbContext _context;

    public BloqueioReservaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BloqueioReserva>> ObterAtivosAsync()
    {
        var agora = DateTimeHelper.AgoraBrasilia();

        return await _context.BloqueiosReserva
            .Where(b => b.Ativo && b.DataFim > agora)
            .OrderBy(b => b.DataInicio)
            .ToListAsync();
    }

    public async Task<BloqueioReserva?> ObterPorIdAsync(int id)
    {
        return await _context.BloqueiosReserva.FindAsync(id);
    }

    public async Task<BloqueioReserva?> ObterQueCruzaAsync(DateTime inicio, DateTime fim)
    {
        // Dois períodos se cruzam quando um começa antes de o outro terminar (e vice-versa).
        return await _context.BloqueiosReserva
            .Where(b => b.Ativo && b.DataInicio < fim && b.DataFim > inicio)
            .OrderBy(b => b.DataInicio)
            .FirstOrDefaultAsync();
    }

    public async Task<BloqueioReserva> CriarAsync(BloqueioReserva bloqueio)
    {
        _context.BloqueiosReserva.Add(bloqueio);
        await _context.SaveChangesAsync();
        return bloqueio;
    }

    public async Task<BloqueioReserva> AtualizarAsync(BloqueioReserva bloqueio)
    {
        _context.BloqueiosReserva.Update(bloqueio);
        await _context.SaveChangesAsync();
        return bloqueio;
    }
}
