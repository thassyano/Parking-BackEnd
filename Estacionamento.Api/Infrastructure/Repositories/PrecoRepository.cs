using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Helpers;
using Estacionamento.Api.Infrastructure.Data;

namespace Estacionamento.Api.Infrastructure.Repositories;

public interface IPrecoRepository
{
    Task<IEnumerable<Preco>> ObterTodosAsync();
    Task<Preco?> ObterPorIdAsync(int id);
    Task<IEnumerable<Preco>> ObterAtivosAsync();
    Task<Preco?> ObterAtivoAsync(TipoVaga tipoVaga);
    Task<Preco> CriarAsync(Preco preco);
    Task<Preco> AtualizarAsync(Preco preco);
}

public class PrecoRepository : IPrecoRepository
{
    private readonly AppDbContext _context;

    public PrecoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Preco>> ObterTodosAsync()
    {
        return await _context.Precos
            .OrderByDescending(p => p.DataInicio)
            .ToListAsync();
    }

    public async Task<Preco?> ObterPorIdAsync(int id)
    {
        return await _context.Precos.FindAsync(id);
    }

    public async Task<IEnumerable<Preco>> ObterAtivosAsync()
    {
        var agora = DateTimeHelper.AgoraBrasilia();
        return await _context.Precos
            .Where(p => p.Ativo &&
                        p.DataInicio <= agora &&
                        (p.DataFim == null || p.DataFim >= agora))
            .OrderBy(p => p.TipoVaga)
            .ToListAsync();
    }

    public async Task<Preco?> ObterAtivoAsync(TipoVaga tipoVaga)
    {
        var agora = DateTimeHelper.AgoraBrasilia();
        return await _context.Precos
            .Where(p => p.Ativo &&
                        p.TipoVaga == tipoVaga &&
                        p.DataInicio <= agora &&
                        (p.DataFim == null || p.DataFim >= agora))
            .OrderByDescending(p => p.DataInicio)
            .FirstOrDefaultAsync();
    }

    public async Task<Preco> CriarAsync(Preco preco)
    {
        var anteriores = await _context.Precos
            .Where(p => p.Ativo && p.TipoVaga == preco.TipoVaga)
            .ToListAsync();

        foreach (var anterior in anteriores)
        {
            anterior.Ativo = false;
            anterior.DataFim ??= preco.DataInicio;
        }

        _context.Precos.Add(preco);
        await _context.SaveChangesAsync();
        return preco;
    }

    public async Task<Preco> AtualizarAsync(Preco preco)
    {
        _context.Precos.Update(preco);
        await _context.SaveChangesAsync();
        return preco;
    }
}
