using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Data;

namespace Estacionamento.Api.Infrastructure.Repositories;

public interface IClienteRepository
{
    Task<IEnumerable<Cliente>> ObterTodosAsync(bool apenasAtivos = true);
    Task<Cliente?> ObterPorIdAsync(int id);
    Task<Cliente?> ObterPorTelefoneAsync(string telefone);
    Task<Cliente> CriarAsync(Cliente cliente);
    Task<Cliente> AtualizarAsync(Cliente cliente);
    Task<bool> ExisteAsync(int id);
}

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Cliente>> ObterTodosAsync(bool apenasAtivos = true)
    {
        var query = _context.Clientes.AsQueryable();
        if (apenasAtivos)
            query = query.Where(c => c.Ativo);

        return await query
            .OrderBy(c => c.Nome)
            .Include(c => c.Reservas)
            .ToListAsync();
    }

    public async Task<Cliente?> ObterPorIdAsync(int id)
    {
        return await _context.Clientes
            .Include(c => c.Reservas)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cliente?> ObterPorTelefoneAsync(string telefone)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.Telefone == telefone && c.Ativo);
    }

    public async Task<Cliente> CriarAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task<Cliente> AtualizarAsync(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task<bool> ExisteAsync(int id)
    {
        return await _context.Clientes.AnyAsync(c => c.Id == id);
    }
}
