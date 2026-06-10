using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Application.Services;

public interface IOrcamentoService
{
    Task<OrcamentoResponseDto> CalcularAsync(ConsultaOrcamentoDto dto);
}

public class OrcamentoService : IOrcamentoService
{
    private readonly IPrecoRepository _precoRepository;
    private readonly IReservaRepository _reservaRepository;
    private readonly IConfiguracaoRepository _configuracaoRepository;

    public OrcamentoService(
        IPrecoRepository precoRepository,
        IReservaRepository reservaRepository,
        IConfiguracaoRepository configuracaoRepository)
    {
        _precoRepository = precoRepository;
        _reservaRepository = reservaRepository;
        _configuracaoRepository = configuracaoRepository;
    }

    public async Task<OrcamentoResponseDto> CalcularAsync(ConsultaOrcamentoDto dto)
    {
        var tipoVaga = Enum.Parse<TipoVaga>(dto.TipoVaga, true);

        var preco = await _precoRepository.ObterAtivoAsync(tipoVaga)
            ?? throw new InvalidOperationException($"Nenhum preço ativo para vaga {dto.TipoVaga}");

        var config = await _configuracaoRepository.ObterAsync();
        var totalVagas = config == null ? 0
            : tipoVaga == TipoVaga.Coberta ? config.TotalVagasCoberta : config.TotalVagasDescoberta;

        var ocupadas = await _reservaRepository.ContarVagasOcupadasAsync(tipoVaga, dto.DataEntrada.Date);

        var valorCartao = preco.ValorDiaria * dto.QtdDias;
        var descontoTotal = preco.DescontoPixDinheiro * dto.QtdDias;
        var valorPixDinheiro = valorCartao - descontoTotal;

        return new OrcamentoResponseDto
        {
            TipoVaga = tipoVaga.ToString(),
            DataEntrada = dto.DataEntrada.Date,
            QtdDias = dto.QtdDias,
            DataSaidaPrevista = dto.DataEntrada.Date.AddDays(dto.QtdDias),
            ValorDiaria = preco.ValorDiaria,
            ValorTotalCartao = valorCartao,
            ValorTotalPixDinheiro = valorPixDinheiro,
            DescontoPixDinheiroPorDia = preco.DescontoPixDinheiro,
            EconomiaTotal = descontoTotal,
            VagasDisponiveis = ocupadas < totalVagas,
            VagasRestantes = Math.Max(0, totalVagas - ocupadas)
        };
    }
}
