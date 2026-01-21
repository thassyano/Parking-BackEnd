using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Application.Services;
using Estacionamento.Api.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IVagaRepository _vagaRepository;
    private readonly IOcupacaoRepository _ocupacaoRepository;

    public DashboardController(
        IVagaRepository vagaRepository,
        IOcupacaoRepository ocupacaoRepository)
    {
        _vagaRepository = vagaRepository;
        _ocupacaoRepository = ocupacaoRepository;
    }

    [HttpGet]
    public async Task<IActionResult> ObterDashboard()
    {
        var vagas = await _vagaRepository.ObterTodasAsync();
        var ocupacoesAtivas = await _ocupacaoRepository.ObterAtivasAsync();
        var todasOcupacoes = await _ocupacaoRepository.ObterTodasAsync();

        var hoje = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

        var ocupacoesHoje = todasOcupacoes.Where(o => 
            o.DataEntrada.Date == hoje && o.ValorPago.HasValue);

        var ocupacoesMes = todasOcupacoes.Where(o => 
            o.DataEntrada >= inicioMes && o.ValorPago.HasValue);

        var dashboard = new DashboardDto
        {
            TotalVagas = vagas.Count(),
            VagasOcupadas = vagas.Count(v => v.Ocupada),
            VagasDisponiveis = vagas.Count(v => !v.Ocupada),
            ReceitaHoje = ocupacoesHoje.Sum(o => o.ValorPago ?? 0),
            ReceitaMes = ocupacoesMes.Sum(o => o.ValorPago ?? 0),
            OcupacoesHoje = ocupacoesHoje.Count(),
            OcupacoesAtivas = ocupacoesAtivas.Select(o => new OcupacaoResumoDto
            {
                Id = o.Id,
                NumeroVaga = o.Vaga.Numero,
                PlacaVeiculo = o.PlacaVeiculo,
                DataEntrada = o.DataEntrada,
                TempoEstacionado = DateTime.UtcNow - o.DataEntrada
            }).ToList()
        };

        return Ok(dashboard);
    }
}

