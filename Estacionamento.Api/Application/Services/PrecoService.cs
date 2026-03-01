using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Application.Services;

public interface IPrecoService
{
    Task<IEnumerable<Preco>> ObterTodosAsync();
    Task<IEnumerable<Preco>> ObterAtivosAsync();
    Task<Preco?> ObterAtivoAsync(TipoVaga tipoVaga);
    Task<Preco> CriarAsync(TipoVaga tipoVaga, decimal valorDiaria, decimal? descontoPix, DateTime? dataInicio = null);
}

public class PrecoService : IPrecoService
{
    private readonly IPrecoRepository _precoRepository;

    public PrecoService(IPrecoRepository precoRepository)
    {
        _precoRepository = precoRepository;
    }

    public async Task<IEnumerable<Preco>> ObterTodosAsync()
    {
        return await _precoRepository.ObterTodosAsync();
    }

    public async Task<IEnumerable<Preco>> ObterAtivosAsync()
    {
        return await _precoRepository.ObterAtivosAsync();
    }

    public async Task<Preco?> ObterAtivoAsync(TipoVaga tipoVaga)
    {
        return await _precoRepository.ObterAtivoAsync(tipoVaga);
    }

    public async Task<Preco> CriarAsync(TipoVaga tipoVaga, decimal valorDiaria, decimal? descontoPix, DateTime? dataInicio = null)
    {
        if (valorDiaria <= 0)
            throw new InvalidOperationException("O valor da diária deve ser maior que zero");

        if (descontoPix.HasValue && (descontoPix.Value < 0 || descontoPix.Value > 100))
            throw new InvalidOperationException("O desconto Pix deve estar entre 0 e 100%");

        var preco = new Preco
        {
            TipoVaga = tipoVaga,
            ValorDiaria = valorDiaria,
            DescontoPix = descontoPix,
            DataInicio = dataInicio ?? DateTime.UtcNow,
            Ativo = true
        };

        return await _precoRepository.CriarAsync(preco);
    }
}
