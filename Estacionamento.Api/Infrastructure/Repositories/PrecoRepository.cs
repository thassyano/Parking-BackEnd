using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Data;

namespace Estacionamento.Api.Infrastructure.Repositories;

public interface IPrecoRepository
{
    Task<IEnumerable<Preco>> ObterTodasAsync();
    Task<Preco?> ObterPorIdAsync(int id);
    Task<Preco?> ObterPrecoAtivoAsync();
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

    public async Task<IEnumerable<Preco>> ObterTodasAsync()
    {
        return await _context.Precos
            .OrderByDescending(p => p.DataInicio)
            .ToListAsync();
    }

    public async Task<Preco?> ObterPorIdAsync(int id)
    {
        return await _context.Precos.FindAsync(id);
    }

    public async Task<Preco?> ObterPrecoAtivoAsync()
    {
        var agora = DateTime.UtcNow;
        return await _context.Precos
            .Where(p => p.Ativo && 
                       p.DataInicio <= agora && 
                       (p.DataFim == null || p.DataFim >= agora))
            .OrderByDescending(p => p.DataInicio)
            .FirstOrDefaultAsync();
    }

    public async Task<Preco> CriarAsync(Preco preco)
    {
        // Desativar preços anteriores
        var precosAnteriores = await _context.Precos
            .Where(p => p.Ativo)
            .ToListAsync();
        
        foreach (var precoAnterior in precosAnteriores)
        {
            precoAnterior.Ativo = false;
            if (precoAnterior.DataFim == null)
            {
                precoAnterior.DataFim = preco.DataInicio;
            }
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

