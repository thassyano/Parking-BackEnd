using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Repositories;
using Estacionamento.Api.Application.DTOs;

namespace Estacionamento.Api.Application.Services;

public interface IVagaService
{
    Task<IEnumerable<Vaga>> ObterTodasVagasAsync();
    Task<Vaga> CriarVagaAsync(string numero);
    Task<Vaga?> ObterVagaPorIdAsync(int id);
    Task<DisponibilidadeDto> ObterDisponibilidadeAsync();
}

public class VagaService : IVagaService
{
    private readonly IVagaRepository _vagaRepository;
    private readonly IOcupacaoRepository _ocupacaoRepository;

    public VagaService(IVagaRepository vagaRepository, IOcupacaoRepository ocupacaoRepository)
    {
        _vagaRepository = vagaRepository;
        _ocupacaoRepository = ocupacaoRepository;
    }

    public async Task<IEnumerable<Vaga>> ObterTodasVagasAsync()
    {
        return await _vagaRepository.ObterTodasAsync();
    }

    public async Task<Vaga> CriarVagaAsync(string numero)
    {
        var vagaExistente = await _vagaRepository.ObterPorNumeroAsync(numero);
        if (vagaExistente != null)
            throw new InvalidOperationException("Já existe uma vaga com este número");

        var vaga = new Vaga
        {
            Numero = numero,
            Ocupada = false
        };

        return await _vagaRepository.CriarAsync(vaga);
    }

    public async Task<Vaga?> ObterVagaPorIdAsync(int id)
    {
        return await _vagaRepository.ObterPorIdAsync(id);
    }

    public async Task<DisponibilidadeDto> ObterDisponibilidadeAsync()
    {
        var vagas = await _vagaRepository.ObterTodasAsync();
        var ocupacoesAtivas = await _ocupacaoRepository.ObterAtivasAsync();

        var disponibilidade = new DisponibilidadeDto
        {
            TotalVagas = vagas.Count(),
            VagasOcupadas = vagas.Count(v => v.Ocupada),
            VagasDisponiveis = vagas.Count(v => !v.Ocupada)
        };

        foreach (var vaga in vagas)
        {
            var ocupacaoAtiva = ocupacoesAtivas.FirstOrDefault(o => o.VagaId == vaga.Id);
            disponibilidade.Vagas.Add(new VagaDisponibilidadeDto
            {
                Id = vaga.Id,
                Numero = vaga.Numero,
                Ocupada = vaga.Ocupada,
                PlacaVeiculo = ocupacaoAtiva?.PlacaVeiculo,
                DataEntrada = ocupacaoAtiva?.DataEntrada
            });
        }

        return disponibilidade;
    }
}

