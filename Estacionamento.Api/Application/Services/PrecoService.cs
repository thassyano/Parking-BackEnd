using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Application.Services;

public interface IPrecoService
{
    Task<IEnumerable<Preco>> ObterTodosAsync();
    Task<IEnumerable<Preco>> ObterAtivosAsync();
    Task<Preco?> ObterAtivoAsync(TipoVaga tipoVaga);
    Task<Preco> CriarAsync(TipoVaga tipoVaga, decimal valorDiaria, decimal descontoPixDinheiro, DateTime? dataInicio = null);
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

    public async Task<Preco> CriarAsync(TipoVaga tipoVaga, decimal valorDiaria, decimal descontoPixDinheiro, DateTime? dataInicio = null)
    {
        if (valorDiaria <= 0)
            throw new InvalidOperationException("O valor da diária deve ser maior que zero");

        if (descontoPixDinheiro < 0)
            throw new InvalidOperationException("O desconto não pode ser negativo");

        if (descontoPixDinheiro >= valorDiaria)
            throw new InvalidOperationException("O desconto não pode ser maior ou igual ao valor da diária");

        var preco = new Preco
        {
            TipoVaga = tipoVaga,
            ValorDiaria = valorDiaria,
            DescontoPixDinheiro = descontoPixDinheiro,
            DataInicio = dataInicio ?? DateTime.UtcNow,
            Ativo = true
        };

        return await _precoRepository.CriarAsync(preco);
    }
}
