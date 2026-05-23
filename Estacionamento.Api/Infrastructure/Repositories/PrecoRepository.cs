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
        await NormalizarAtivosAsync();

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
        await NormalizarAtivosAsync();

        var hoje = DateTimeHelper.AgoraBrasilia().Date;
        return await _context.Precos
            .Where(p => p.Ativo &&
                        p.DataInicio.Date <= hoje &&
                        (p.DataFim == null || p.DataFim.Value.Date >= hoje))
            .OrderBy(p => p.TipoVaga)
            .ToListAsync();
    }

    public async Task<Preco?> ObterAtivoAsync(TipoVaga tipoVaga)
    {
        await NormalizarAtivosAsync();

        var hoje = DateTimeHelper.AgoraBrasilia().Date;
        return await _context.Precos
            .Where(p => p.Ativo &&
                        p.TipoVaga == tipoVaga &&
                        p.DataInicio.Date <= hoje &&
                        (p.DataFim == null || p.DataFim.Value.Date >= hoje))
            .OrderByDescending(p => p.DataInicio)
            .ThenByDescending(p => p.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<Preco> CriarAsync(Preco preco)
    {
        var inicioNovoPreco = preco.DataInicio.Date;
        var fimNovoPreco = (preco.DataFim ?? DateTime.MaxValue).Date;

        var sobrepostos = await _context.Precos
            .Where(p => p.Ativo &&
                        p.TipoVaga == preco.TipoVaga &&
                        p.DataInicio.Date <= fimNovoPreco &&
                        (p.DataFim == null || p.DataFim.Value.Date >= inicioNovoPreco))
            .ToListAsync();

        foreach (var anterior in sobrepostos)
        {
            anterior.Ativo = false;

            if (!anterior.DataFim.HasValue || anterior.DataFim.Value.Date >= inicioNovoPreco)
            {
                var dataFimAjustada = inicioNovoPreco;

                if (dataFimAjustada < anterior.DataInicio.Date)
                    dataFimAjustada = anterior.DataInicio.Date;

                anterior.DataFim = dataFimAjustada;
            }
        }

        _context.Precos.Add(preco);
        await _context.SaveChangesAsync();

        await NormalizarAtivosAsync();
        await _context.Entry(preco).ReloadAsync();

        return preco;
    }

    public async Task<Preco> AtualizarAsync(Preco preco)
    {
        _context.Precos.Update(preco);
        await _context.SaveChangesAsync();
        return preco;
    }

    private async Task NormalizarAtivosAsync()
    {
        var hoje = DateTimeHelper.AgoraBrasilia().Date;
        var precos = await _context.Precos
            .OrderBy(p => p.TipoVaga)
            .ThenByDescending(p => p.DataInicio)
            .ThenByDescending(p => p.Id)
            .ToListAsync();

        var alterou = false;

        foreach (var grupo in precos.GroupBy(p => p.TipoVaga))
        {
            var vigentesHoje = grupo
                .Where(p => IntervaloValido(p) &&
                            p.DataInicio.Date <= hoje &&
                            (!p.DataFim.HasValue || p.DataFim.Value.Date >= hoje))
                .OrderByDescending(p => p.DataInicio)
                .ThenByDescending(p => p.Id)
                .ToList();

            var proximoAgendado = grupo
                .Where(p => IntervaloValido(p) && p.DataInicio.Date > hoje)
                .OrderBy(p => p.DataInicio)
                .ThenByDescending(p => p.Id)
                .FirstOrDefault();

            var precoPrincipal = vigentesHoje.FirstOrDefault() ?? proximoAgendado;

            foreach (var preco in grupo)
            {
                if (!IntervaloValido(preco))
                {
                    if (preco.Ativo)
                    {
                        preco.Ativo = false;
                        alterou = true;
                    }

                    continue;
                }

                var deveFicarAtivo = precoPrincipal != null && preco.Id == precoPrincipal.Id;
                if (preco.Ativo != deveFicarAtivo)
                {
                    preco.Ativo = deveFicarAtivo;
                    alterou = true;
                }
            }
        }

        if (alterou)
            await _context.SaveChangesAsync();
    }

    private static bool IntervaloValido(Preco preco)
    {
        return !preco.DataFim.HasValue || preco.DataFim.Value.Date >= preco.DataInicio.Date;
    }
}
