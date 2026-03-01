using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Application.Services;

public interface IDisponibilidadeService
{
    Task<DisponibilidadeResponseDto> ConsultarDiaAsync(DateTime data);
    Task<DisponibilidadePeriodoDto> ConsultarPeriodoAsync(DateTime dataInicio, DateTime dataFim);
}

public class DisponibilidadeService : IDisponibilidadeService
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IConfiguracaoRepository _configuracaoRepository;

    public DisponibilidadeService(
        IReservaRepository reservaRepository,
        IConfiguracaoRepository configuracaoRepository)
    {
        _reservaRepository = reservaRepository;
        _configuracaoRepository = configuracaoRepository;
    }

    public async Task<DisponibilidadeResponseDto> ConsultarDiaAsync(DateTime data)
    {
        var config = await _configuracaoRepository.ObterAsync();
        if (config == null)
            return new DisponibilidadeResponseDto { Data = data.Date };

        var cobertaOcupadas = await _reservaRepository.ContarVagasOcupadasAsync(TipoVaga.Coberta, data.Date);
        var descobertaOcupadas = await _reservaRepository.ContarVagasOcupadasAsync(TipoVaga.Descoberta, data.Date);

        return new DisponibilidadeResponseDto
        {
            Data = data.Date,
            VagasCobertaTotal = config.TotalVagasCoberta,
            VagasCobertaOcupadas = cobertaOcupadas,
            VagasCobertaDisponiveis = Math.Max(0, config.TotalVagasCoberta - cobertaOcupadas),
            VagasDescobertaTotal = config.TotalVagasDescoberta,
            VagasDescobertaOcupadas = descobertaOcupadas,
            VagasDescobertaDisponiveis = Math.Max(0, config.TotalVagasDescoberta - descobertaOcupadas)
        };
    }

    public async Task<DisponibilidadePeriodoDto> ConsultarPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        var resultado = new DisponibilidadePeriodoDto
        {
            DataInicio = dataInicio.Date,
            DataFim = dataFim.Date
        };

        for (var data = dataInicio.Date; data <= dataFim.Date; data = data.AddDays(1))
        {
            resultado.Dias.Add(await ConsultarDiaAsync(data));
        }

        return resultado;
    }
}
