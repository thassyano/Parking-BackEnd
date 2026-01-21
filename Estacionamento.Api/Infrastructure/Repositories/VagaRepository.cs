using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Data;

namespace Estacionamento.Api.Infrastructure.Repositories;

public interface IVagaRepository
{
    Task<IEnumerable<Vaga>> ObterTodasAsync();
    Task<Vaga?> ObterPorIdAsync(int id);
    Task<Vaga?> ObterPorNumeroAsync(string numero);
    Task<Vaga> CriarAsync(Vaga vaga);
    Task<Vaga> AtualizarAsync(Vaga vaga);
    Task<bool> ExisteAsync(int id);
}

public class VagaRepository : IVagaRepository
{
    private readonly AppDbContext _context;

    public VagaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Vaga>> ObterTodasAsync()
    {
        return await _context.Vagas
            .Include(v => v.Ocupacoes.Where(o => o.Ativa))
            .ToListAsync();
    }

    public async Task<Vaga?> ObterPorIdAsync(int id)
    {
        return await _context.Vagas
            .Include(v => v.Ocupacoes)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Vaga?> ObterPorNumeroAsync(string numero)
    {
        return await _context.Vagas
            .Include(v => v.Ocupacoes.Where(o => o.Ativa))
            .FirstOrDefaultAsync(v => v.Numero == numero);
    }

    public async Task<Vaga> CriarAsync(Vaga vaga)
    {
        _context.Vagas.Add(vaga);
        await _context.SaveChangesAsync();
        return vaga;
    }

    public async Task<Vaga> AtualizarAsync(Vaga vaga)
    {
        _context.Vagas.Update(vaga);
        await _context.SaveChangesAsync();
        return vaga;
    }

    public async Task<bool> ExisteAsync(int id)
    {
        return await _context.Vagas.AnyAsync(v => v.Id == id);
    }
}

