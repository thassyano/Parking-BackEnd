using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Helpers;
using Estacionamento.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Estacionamento.Api.Application.Services;

public interface ILogAtividadeService
{
    Task RegistrarAsync(RegistrarLogAtividadeDto dto, CancellationToken cancellationToken = default);
    Task<LogAtividadePaginadoDto> ListarAsync(FiltroLogAtividadeDto filtro, CancellationToken cancellationToken = default);
}

public class LogAtividadeService : ILogAtividadeService
{
    private readonly AppDbContext _context;

    public LogAtividadeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(RegistrarLogAtividadeDto dto, CancellationToken cancellationToken = default)
    {
        var log = new LogAtividade
        {
            DataHora = DateTimeHelper.AgoraBrasilia(),
            AdminId = dto.AdminId,
            AdminUsuario = dto.AdminUsuario,
            Acao = dto.Acao,
            Entidade = dto.Entidade,
            EntidadeId = dto.EntidadeId,
            Detalhes = dto.Detalhes.Length > 500 ? dto.Detalhes[..500] : dto.Detalhes,
            Sucesso = dto.Sucesso,
            Origem = dto.Origem
        };

        _context.LogsAtividade.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<LogAtividadePaginadoDto> ListarAsync(FiltroLogAtividadeDto filtro, CancellationToken cancellationToken = default)
    {
        var pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
        var tamanho = filtro.TamanhoPagina switch
        {
            < 1 => 50,
            > 100 => 100,
            _ => filtro.TamanhoPagina
        };

        var query = _context.LogsAtividade.AsNoTracking().AsQueryable();

        if (filtro.DataInicio.HasValue)
            query = query.Where(l => l.DataHora >= filtro.DataInicio.Value);

        if (filtro.DataFim.HasValue)
        {
            var fim = filtro.DataFim.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(l => l.DataHora <= fim);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Acao))
            query = query.Where(l => l.Acao == filtro.Acao);

        if (!string.IsNullOrWhiteSpace(filtro.AdminUsuario))
            query = query.Where(l => l.AdminUsuario != null && EF.Functions.ILike(l.AdminUsuario, $"%{filtro.AdminUsuario}%"));

        if (!string.IsNullOrWhiteSpace(filtro.Origem))
            query = query.Where(l => l.Origem == filtro.Origem);

        if (filtro.Sucesso.HasValue)
            query = query.Where(l => l.Sucesso == filtro.Sucesso.Value);

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(l => l.DataHora)
            .ThenByDescending(l => l.Id)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .Select(l => new LogAtividadeResponseDto
            {
                Id = l.Id,
                DataHora = l.DataHora,
                AdminId = l.AdminId,
                AdminUsuario = l.AdminUsuario,
                Acao = l.Acao,
                Entidade = l.Entidade,
                EntidadeId = l.EntidadeId,
                Detalhes = l.Detalhes,
                Sucesso = l.Sucesso,
                Origem = l.Origem
            })
            .ToListAsync(cancellationToken);

        return new LogAtividadePaginadoDto
        {
            Itens = itens,
            Total = total,
            Pagina = pagina,
            TamanhoPagina = tamanho,
            TotalPaginas = total == 0 ? 0 : (int)Math.Ceiling(total / (double)tamanho)
        };
    }
}

public static class LogAtividadeHttpExtensions
{
    public static RegistrarLogAtividadeDto CriarRegistro(
        ClaimsPrincipal? user,
        string acao,
        string detalhes,
        string? entidade = null,
        int? entidadeId = null,
        bool sucesso = true,
        string origem = OrigemLog.Admin)
    {
        int? adminId = null;
        string? adminUsuario = null;

        if (user?.Identity?.IsAuthenticated == true)
        {
            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out var parsedId))
                adminId = parsedId;
            adminUsuario = user.FindFirst(ClaimTypes.Name)?.Value;
        }

        return new RegistrarLogAtividadeDto
        {
            AdminId = adminId,
            AdminUsuario = adminUsuario,
            Acao = acao,
            Entidade = entidade,
            EntidadeId = entidadeId,
            Detalhes = detalhes,
            Sucesso = sucesso,
            Origem = origem
        };
    }

    public static RegistrarLogAtividadeDto CriarRegistroSistema(
        string acao,
        string detalhes,
        string? entidade = null,
        int? entidadeId = null,
        bool sucesso = true,
        string origem = OrigemLog.Worker) =>
        new()
        {
            AdminUsuario = "Sistema",
            Acao = acao,
            Entidade = entidade,
            EntidadeId = entidadeId,
            Detalhes = detalhes,
            Sucesso = sucesso,
            Origem = origem
        };
}
