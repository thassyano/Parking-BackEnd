using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Application.Services;

public interface IPrecoService
{
    Task<IEnumerable<Preco>> ObterTodosAsync();
    Task<IEnumerable<Preco>> ObterAtivosAsync();
    Task<Preco?> ObterAtivoAsync(TipoVaga tipoVaga);
    Task<Preco> CriarAsync(TipoVaga tipoVaga, decimal valorDiaria, decimal descontoPixDinheiro, decimal valorHorasAdicionaisAte6h, decimal valorHorasAdicionaisAte12h, DateTime dataInicio, DateTime? dataFim = null);
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
        await _precoRepository.DesativarPrecosExpiradosAsync();

        return await _precoRepository.ObterTodosAsync();
    }

    public async Task<IEnumerable<Preco>> ObterAtivosAsync()
    {
        await _precoRepository.DesativarPrecosExpiradosAsync();

        return await _precoRepository.ObterAtivosAsync();
    }

    public async Task<Preco?> ObterAtivoAsync(TipoVaga tipoVaga)
    {
        await _precoRepository.DesativarPrecosExpiradosAsync();

        return await _precoRepository.ObterAtivoAsync(tipoVaga);
    }

    public async Task<Preco> CriarAsync(TipoVaga tipoVaga, decimal valorDiaria, decimal descontoPixDinheiro, decimal valorHorasAdicionaisAte6h, decimal valorHorasAdicionaisAte12h, DateTime dataInicio, DateTime? dataFim = null)
    {
        if (valorDiaria <= 0)
            throw new InvalidOperationException("O valor da diária deve ser maior que zero");

        if (descontoPixDinheiro < 0)
            throw new InvalidOperationException("O desconto não pode ser negativo");

        if (descontoPixDinheiro >= valorDiaria)
            throw new InvalidOperationException("O desconto não pode ser maior ou igual ao valor da diária");

        if (valorHorasAdicionaisAte6h < 0)
            throw new InvalidOperationException("O valor de horas adicionais (até 6h) não pode ser negativo");

        if (valorHorasAdicionaisAte12h < 0)
            throw new InvalidOperationException("O valor de horas adicionais (até 12h) não pode ser negativo");

        if (valorHorasAdicionaisAte12h < valorHorasAdicionaisAte6h)
            throw new InvalidOperationException("O valor de até 12h não pode ser menor que o de até 6h");

        if (dataFim.HasValue && dataFim.Value.Date <= dataInicio.Date)
            throw new InvalidOperationException("A data fim deve ser posterior à data de início");

        var preco = new Preco
        {
            TipoVaga = tipoVaga,
            ValorDiaria = valorDiaria,
            DescontoPixDinheiro = descontoPixDinheiro,
            ValorHorasAdicionaisAte6h = valorHorasAdicionaisAte6h,
            ValorHorasAdicionaisAte12h = valorHorasAdicionaisAte12h,
            DataInicio = dataInicio,
            DataFim = dataFim,
            Ativo = true
        };

        return await _precoRepository.CriarAsync(preco);
    }
}
