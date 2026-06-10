using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Helpers;
using Estacionamento.Api.Infrastructure.Data;

namespace Estacionamento.Api.Infrastructure.Repositories;

public interface IConfiguracaoRepository
{
    Task<ConfiguracaoEstacionamento?> ObterAsync();
    Task<ConfiguracaoEstacionamento> CriarOuAtualizarAsync(ConfiguracaoEstacionamento config);
}

public class ConfiguracaoRepository : IConfiguracaoRepository
{
    private readonly AppDbContext _context;

    public ConfiguracaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ConfiguracaoEstacionamento?> ObterAsync()
    {
        return await _context.Configuracoes.FirstOrDefaultAsync();
    }

    public async Task<ConfiguracaoEstacionamento> CriarOuAtualizarAsync(ConfiguracaoEstacionamento config)
    {
        var existente = await _context.Configuracoes.FirstOrDefaultAsync();
        if (existente == null)
        {
            _context.Configuracoes.Add(config);
        }
        else
        {
            existente.NomeEstacionamento = config.NomeEstacionamento;
            existente.Endereco = config.Endereco;
            existente.Contato = config.Contato;
            existente.Cnpj = config.Cnpj;
            existente.TotalVagasCoberta = config.TotalVagasCoberta;
            existente.TotalVagasDescoberta = config.TotalVagasDescoberta;
            existente.TelefoneWhatsApp = config.TelefoneWhatsApp;
            existente.MensagemWhatsApp = config.MensagemWhatsApp;
            existente.HorasAntecedenciaConfirmacao = config.HorasAntecedenciaConfirmacao;
            existente.DataAtualizacao = DateTimeHelper.AgoraBrasilia();
        }

        await _context.SaveChangesAsync();
        return existente ?? config;
    }
}
