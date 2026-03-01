using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Data;

namespace Estacionamento.Api.Infrastructure.Repositories;

public interface IPagamentoRepository
{
    Task<IEnumerable<Pagamento>> ObterPorReservaAsync(int reservaId);
    Task<Pagamento> CriarAsync(Pagamento pagamento);
    Task<Pagamento> AtualizarAsync(Pagamento pagamento);
}

public class PagamentoRepository : IPagamentoRepository
{
    private readonly AppDbContext _context;

    public PagamentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Pagamento>> ObterPorReservaAsync(int reservaId)
    {
        return await _context.Pagamentos
            .Where(p => p.ReservaId == reservaId)
            .OrderByDescending(p => p.DataPagamento)
            .ToListAsync();
    }

    public async Task<Pagamento> CriarAsync(Pagamento pagamento)
    {
        _context.Pagamentos.Add(pagamento);
        await _context.SaveChangesAsync();
        return pagamento;
    }

    public async Task<Pagamento> AtualizarAsync(Pagamento pagamento)
    {
        _context.Pagamentos.Update(pagamento);
        await _context.SaveChangesAsync();
        return pagamento;
    }
}
