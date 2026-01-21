using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Application.Services;

public interface IPrecoService
{
    Task<IEnumerable<Preco>> ObterTodosPrecosAsync();
    Task<Preco?> ObterPrecoAtivoAsync();
    Task<Preco> CriarPrecoAsync(decimal valorHora, decimal valorMinuto, DateTime? dataInicio = null);
}

public class PrecoService : IPrecoService
{
    private readonly IPrecoRepository _precoRepository;

    public PrecoService(IPrecoRepository precoRepository)
    {
        _precoRepository = precoRepository;
    }

    public async Task<IEnumerable<Preco>> ObterTodosPrecosAsync()
    {
        return await _precoRepository.ObterTodasAsync();
    }

    public async Task<Preco?> ObterPrecoAtivoAsync()
    {
        return await _precoRepository.ObterPrecoAtivoAsync();
    }

    public async Task<Preco> CriarPrecoAsync(decimal valorHora, decimal valorMinuto, DateTime? dataInicio = null)
    {
        if (valorHora <= 0 || valorMinuto <= 0)
            throw new InvalidOperationException("Os valores devem ser maiores que zero");

        var preco = new Preco
        {
            ValorHora = valorHora,
            ValorMinuto = valorMinuto,
            DataInicio = dataInicio ?? DateTime.UtcNow,
            Ativo = true
        };

        return await _precoRepository.CriarAsync(preco);
    }
}

