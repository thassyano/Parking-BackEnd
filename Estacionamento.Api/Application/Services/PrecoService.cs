using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Application.Services;

public interface IPrecoService
{
    Task<IEnumerable<Preco>> ObterTodosAsync();
    Task<IEnumerable<Preco>> ObterAtivosAsync();
    Task<Preco?> ObterAtivoAsync(TipoVaga tipoVaga);
    Task<Preco> CriarAsync(TipoVaga tipoVaga, decimal valorHorasAdicionaisAte6h, decimal valorHorasAdicionaisAte12h, decimal valorDiaria, decimal descontoPixDinheiro, DateTime dataInicio, DateTime? dataFim = null);
}

public class PrecoService : IPrecoService
{
    private readonly IPrecoRepository _precoRepository;

    public PrecoService(IPrecoRepository precoRepository)
    {
        _precoRepository = precoRepository;
    }

    public async Task<IEnumerable<Preco>> ObterTodosAsync() => await _precoRepository.ObterTodosAsync();

    public async Task<IEnumerable<Preco>> ObterAtivosAsync() => await _precoRepository.ObterAtivosAsync();

    public async Task<Preco?> ObterAtivoAsync(TipoVaga tipoVaga) => await _precoRepository.ObterAtivoAsync(tipoVaga);

    public async Task<Preco> CriarAsync(TipoVaga tipoVaga, decimal valorHorasAdicionaisAte6h, decimal valorHorasAdicionaisAte12h, decimal valorDiaria, decimal descontoPixDinheiro, DateTime dataInicio, DateTime? dataFim = null)
    {
        if (valorDiaria <= 0)
            throw new InvalidOperationException("O valor da diaria deve ser maior que zero");

        if (valorHorasAdicionaisAte6h < 0 || valorHorasAdicionaisAte12h < 0)
            throw new InvalidOperationException("Os valores das faixas nao podem ser negativos");

        if (valorHorasAdicionaisAte6h > valorHorasAdicionaisAte12h)
            throw new InvalidOperationException("A faixa ate 6h nao pode ser maior que a faixa ate 12h");

        if (valorHorasAdicionaisAte12h > valorDiaria)
            throw new InvalidOperationException("A faixa ate 12h nao pode ser maior que a diaria cheia");

        if (descontoPixDinheiro < 0)
            throw new InvalidOperationException("O desconto nao pode ser negativo");

        if (descontoPixDinheiro >= valorDiaria)
            throw new InvalidOperationException("O desconto nao pode ser maior ou igual ao valor da diaria");

        if (dataFim.HasValue && dataFim.Value.Date <= dataInicio.Date)
            throw new InvalidOperationException("A data fim deve ser posterior a data de inicio");

        var preco = new Preco
        {
            TipoVaga = tipoVaga,
            ValorHorasAdicionaisAte6h = valorHorasAdicionaisAte6h,
            ValorHorasAdicionaisAte12h = valorHorasAdicionaisAte12h,
            ValorDiaria = valorDiaria,
            DescontoPixDinheiro = descontoPixDinheiro,
            DataInicio = dataInicio,
            DataFim = dataFim,
            Ativo = true
        };

        return await _precoRepository.CriarAsync(preco);
    }
}
