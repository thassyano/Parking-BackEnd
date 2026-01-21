using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Repositories;
using Estacionamento.Api.Application.DTOs;

namespace Estacionamento.Api.Application.Services;

public interface IOcupacaoService
{
    Task<Ocupacao> CriarOcupacaoAsync(CriarOcupacaoDto dto);
    Task<Ocupacao> FinalizarOcupacaoAsync(int ocupacaoId);
    Task<decimal> CalcularValorAsync(int ocupacaoId);
    Task<IEnumerable<Ocupacao>> ObterOcupacoesAtivasAsync();
    Task<Ocupacao?> ObterOcupacaoPorIdAsync(int id);
}

public class OcupacaoService : IOcupacaoService
{
    private readonly IOcupacaoRepository _ocupacaoRepository;
    private readonly IVagaRepository _vagaRepository;
    private readonly IPrecoRepository _precoRepository;

    public OcupacaoService(
        IOcupacaoRepository ocupacaoRepository,
        IVagaRepository vagaRepository,
        IPrecoRepository precoRepository)
    {
        _ocupacaoRepository = ocupacaoRepository;
        _vagaRepository = vagaRepository;
        _precoRepository = precoRepository;
    }

    public async Task<Ocupacao> CriarOcupacaoAsync(CriarOcupacaoDto dto)
    {
        var vaga = await _vagaRepository.ObterPorIdAsync(dto.VagaId);
        if (vaga == null)
            throw new InvalidOperationException("Vaga não encontrada");

        if (vaga.Ocupada)
            throw new InvalidOperationException("Vaga já está ocupada");

        var ocupacaoAtiva = await _ocupacaoRepository.ObterAtivaPorVagaIdAsync(dto.VagaId);
        if (ocupacaoAtiva != null)
            throw new InvalidOperationException("Já existe uma ocupação ativa para esta vaga");

        var ocupacao = new Ocupacao
        {
            VagaId = dto.VagaId,
            PlacaVeiculo = dto.PlacaVeiculo.ToUpper(),
            DataEntrada = DateTime.UtcNow,
            Ativa = true
        };

        vaga.Ocupada = true;
        vaga.DataUltimaOcupacao = DateTime.UtcNow;

        await _ocupacaoRepository.CriarAsync(ocupacao);
        await _vagaRepository.AtualizarAsync(vaga);

        return ocupacao;
    }

    public async Task<Ocupacao> FinalizarOcupacaoAsync(int ocupacaoId)
    {
        var ocupacao = await _ocupacaoRepository.ObterPorIdAsync(ocupacaoId);
        if (ocupacao == null)
            throw new InvalidOperationException("Ocupação não encontrada");

        if (ocupacao.DataSaida != null)
            throw new InvalidOperationException("Ocupação já foi finalizada");

        ocupacao.DataSaida = DateTime.UtcNow;
        ocupacao.Ativa = false;
        ocupacao.ValorPago = await CalcularValorAsync(ocupacaoId);

        var vaga = ocupacao.Vaga;
        vaga.Ocupada = false;

        await _ocupacaoRepository.AtualizarAsync(ocupacao);
        await _vagaRepository.AtualizarAsync(vaga);

        return ocupacao;
    }

    public async Task<decimal> CalcularValorAsync(int ocupacaoId)
    {
        var ocupacao = await _ocupacaoRepository.ObterPorIdAsync(ocupacaoId);
        if (ocupacao == null)
            throw new InvalidOperationException("Ocupação não encontrada");

        var preco = await _precoRepository.ObterPrecoAtivoAsync();
        if (preco == null)
            throw new InvalidOperationException("Nenhum preço ativo encontrado");

        var dataSaida = ocupacao.DataSaida ?? DateTime.UtcNow;
        var tempoEstacionado = dataSaida - ocupacao.DataEntrada;
        var minutosTotais = (int)Math.Ceiling(tempoEstacionado.TotalMinutes);

        // Mínimo de 1 minuto
        if (minutosTotais < 1)
            minutosTotais = 1;

        return minutosTotais * preco.ValorMinuto;
    }

    public async Task<IEnumerable<Ocupacao>> ObterOcupacoesAtivasAsync()
    {
        return await _ocupacaoRepository.ObterAtivasAsync();
    }

    public async Task<Ocupacao?> ObterOcupacaoPorIdAsync(int id)
    {
        return await _ocupacaoRepository.ObterPorIdAsync(id);
    }
}

