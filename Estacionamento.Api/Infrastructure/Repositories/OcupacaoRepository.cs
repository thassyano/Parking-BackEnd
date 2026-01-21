using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Data;

namespace Estacionamento.Api.Infrastructure.Repositories;

public interface IOcupacaoRepository
{
    Task<IEnumerable<Ocupacao>> ObterTodasAsync();
    Task<Ocupacao?> ObterPorIdAsync(int id);
    Task<IEnumerable<Ocupacao>> ObterAtivasAsync();
    Task<Ocupacao?> ObterAtivaPorVagaIdAsync(int vagaId);
    Task<Ocupacao> CriarAsync(Ocupacao ocupacao);
    Task<Ocupacao> AtualizarAsync(Ocupacao ocupacao);
    Task<IEnumerable<Ocupacao>> ObterPorPlacaAsync(string placa);
}

public class OcupacaoRepository : IOcupacaoRepository
{
    private readonly AppDbContext _context;

    public OcupacaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Ocupacao>> ObterTodasAsync()
    {
        return await _context.Ocupacoes
            .Include(o => o.Vaga)
            .OrderByDescending(o => o.DataEntrada)
            .ToListAsync();
    }

    public async Task<Ocupacao?> ObterPorIdAsync(int id)
    {
        return await _context.Ocupacoes
            .Include(o => o.Vaga)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Ocupacao>> ObterAtivasAsync()
    {
        return await _context.Ocupacoes
            .Include(o => o.Vaga)
            .Where(o => o.Ativa && o.DataSaida == null)
            .OrderByDescending(o => o.DataEntrada)
            .ToListAsync();
    }

    public async Task<Ocupacao?> ObterAtivaPorVagaIdAsync(int vagaId)
    {
        return await _context.Ocupacoes
            .Include(o => o.Vaga)
            .FirstOrDefaultAsync(o => o.VagaId == vagaId && o.Ativa && o.DataSaida == null);
    }

    public async Task<Ocupacao> CriarAsync(Ocupacao ocupacao)
    {
        _context.Ocupacoes.Add(ocupacao);
        await _context.SaveChangesAsync();
        return ocupacao;
    }

    public async Task<Ocupacao> AtualizarAsync(Ocupacao ocupacao)
    {
        _context.Ocupacoes.Update(ocupacao);
        await _context.SaveChangesAsync();
        return ocupacao;
    }

    public async Task<IEnumerable<Ocupacao>> ObterPorPlacaAsync(string placa)
    {
        return await _context.Ocupacoes
            .Include(o => o.Vaga)
            .Where(o => o.PlacaVeiculo == placa)
            .OrderByDescending(o => o.DataEntrada)
            .ToListAsync();
    }
}

