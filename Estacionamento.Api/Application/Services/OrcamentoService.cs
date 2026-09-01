using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Repositories;
using Estacionamento.Api.Helpers;

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

        // Saida prevista: usa a data/hora informada; se ausente, deriva de QtdDias (dias cheios)
        var saidaPrevista = dto.DataSaidaPrevista ?? dto.DataEntrada.AddDays(dto.QtdDias);

        // Diarias cheias + valor fixo da faixa do periodo parcial (ate 6h / ate 12h / acima = diaria)
        var estadia = CalculadoraEstadia.Calcular(
            dto.DataEntrada, saidaPrevista,
            preco.ValorHorasAdicionaisAte6h, preco.ValorHorasAdicionaisAte12h, preco.ValorDiaria);

        var valorCartao = estadia.ValorEstadia;
        var descontoTotal = preco.DescontoPixDinheiro * estadia.DiariasCompletas;
        var valorPixDinheiro = valorCartao - descontoTotal;

        // Rotulo de precificacao: estadia sub-diaria mostra a faixa; com dias cheios mostra "Diaria"
        var totalHoras = (decimal)(saidaPrevista - dto.DataEntrada).TotalHours;
        var horasParcial = totalHoras - (estadia.DiariasCompletas * 24m);
        var tipoPrecificacao =
            estadia.DiariasCompletas == 0 && horasParcial > 0m && horasParcial <= 6m ? "HorasAte6h"
            : estadia.DiariasCompletas == 0 && horasParcial > 6m && horasParcial <= 12m ? "HorasAte12h"
            : "Diaria";

        return new OrcamentoResponseDto
        {
            TipoVaga = tipoVaga.ToString(),
            DataEntrada = dto.DataEntrada.Date,
            QtdDias = estadia.DiariasCompletas,
            DataSaidaPrevista = saidaPrevista,
            TipoPrecificacao = tipoPrecificacao,
            ValorDiaria = preco.ValorDiaria,
            DiariasCompletas = estadia.DiariasCompletas,
            ValorHorasAdicionais = estadia.ValorHorasAdicionais,
            ValorTotalCartao = valorCartao,
            ValorTotalPixDinheiro = valorPixDinheiro,
            DescontoPixDinheiroPorDia = preco.DescontoPixDinheiro,
            EconomiaTotal = descontoTotal,
            VagasDisponiveis = ocupadas < totalVagas,
            VagasRestantes = Math.Max(0, totalVagas - ocupadas)
        };
    }
}
