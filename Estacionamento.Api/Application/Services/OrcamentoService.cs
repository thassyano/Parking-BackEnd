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

        var ocupadas = await _reservaRepository.ContarVagasOcupadasAsync(tipoVaga, dto.DataReserva.Date);

        var valorCartao = preco.ValorDiaria * dto.QtdDias;
        var valorPix = valorCartao;
        decimal? descontoPix = preco.DescontoPix;

        if (descontoPix.HasValue && descontoPix.Value > 0)
        {
            valorPix = valorCartao - (valorCartao * (descontoPix.Value / 100));
        }

        return new OrcamentoResponseDto
        {
            TipoVaga = tipoVaga.ToString(),
            DataReserva = dto.DataReserva.Date,
            QtdDias = dto.QtdDias,
            DataFim = dto.DataReserva.Date.AddDays(dto.QtdDias - 1),
            ValorDiaria = preco.ValorDiaria,
            ValorTotalCartao = valorCartao,
            ValorTotalPix = valorPix,
            DescontoPix = descontoPix,
            EconomiaComPix = valorCartao - valorPix,
            VagasDisponiveis = ocupadas < totalVagas,
            VagasRestantes = Math.Max(0, totalVagas - ocupadas)
        };
    }
}
