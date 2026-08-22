using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfiguracaoController : ControllerBase
{
    private readonly IConfiguracaoRepository _configuracaoRepository;

    public ConfiguracaoController(IConfiguracaoRepository configuracaoRepository)
    {
        _configuracaoRepository = configuracaoRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Obter()
    {
        var config = await _configuracaoRepository.ObterAsync();
        if (config == null)
            return NotFound(new { message = "Configuração não encontrada. Execute o seed primeiro." });

        return Ok(MapToResponse(config));
    }

    [HttpPut]
    public async Task<IActionResult> Atualizar([FromBody] AtualizarConfiguracaoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var config = await _configuracaoRepository.ObterAsync() ?? new ConfiguracaoEstacionamento();

        if (dto.NomeEstacionamento != null) config.NomeEstacionamento = dto.NomeEstacionamento;
        if (dto.Endereco != null) config.Endereco = dto.Endereco;
        if (dto.Contato != null) config.Contato = dto.Contato;
        if (dto.Cnpj != null) config.Cnpj = dto.Cnpj;
        if (dto.TotalVagasCoberta.HasValue) config.TotalVagasCoberta = dto.TotalVagasCoberta.Value;
        if (dto.TotalVagasDescoberta.HasValue) config.TotalVagasDescoberta = dto.TotalVagasDescoberta.Value;
        if (dto.ValorTraslado.HasValue) config.ValorTraslado = dto.ValorTraslado.Value;
        if (dto.TrasladoGratisAPartirDiarias.HasValue) config.TrasladoGratisAPartirDiarias = dto.TrasladoGratisAPartirDiarias.Value;
        if (dto.TelefoneWhatsApp != null) config.TelefoneWhatsApp = dto.TelefoneWhatsApp;
        if (dto.MensagemWhatsApp != null) config.MensagemWhatsApp = dto.MensagemWhatsApp;
        if (dto.EvolutionApiUrl != null) config.EvolutionApiUrl = dto.EvolutionApiUrl;
        if (dto.EvolutionApiKey != null) config.EvolutionApiKey = dto.EvolutionApiKey;
        if (dto.EvolutionInstanceName != null) config.EvolutionInstanceName = dto.EvolutionInstanceName;
        if (dto.UrlConfirmacaoFrontend != null) config.UrlConfirmacaoFrontend = dto.UrlConfirmacaoFrontend;
        if (dto.HorasAntecedenciaConfirmacao.HasValue) config.HorasAntecedenciaConfirmacao = dto.HorasAntecedenciaConfirmacao.Value;

        var atualizada = await _configuracaoRepository.CriarOuAtualizarAsync(config);
        return Ok(MapToResponse(atualizada));
    }

    private static ConfiguracaoResponseDto MapToResponse(ConfiguracaoEstacionamento c) => new()
    {
        Id = c.Id,
        NomeEstacionamento = c.NomeEstacionamento,
        Endereco = c.Endereco,
        Contato = c.Contato,
        Cnpj = c.Cnpj,
        TotalVagasCoberta = c.TotalVagasCoberta,
        TotalVagasDescoberta = c.TotalVagasDescoberta,
        ValorTraslado = c.ValorTraslado,
        TrasladoGratisAPartirDiarias = c.TrasladoGratisAPartirDiarias,
        TelefoneWhatsApp = c.TelefoneWhatsApp,
        MensagemWhatsApp = c.MensagemWhatsApp,
        EvolutionApiUrl = c.EvolutionApiUrl,
        EvolutionInstanceName = c.EvolutionInstanceName,
        EvolutionConfigurada = !string.IsNullOrEmpty(c.EvolutionApiUrl)
            && !string.IsNullOrEmpty(c.EvolutionApiKey)
            && !string.IsNullOrEmpty(c.EvolutionInstanceName),
        UrlConfirmacaoFrontend = c.UrlConfirmacaoFrontend,
        HorasAntecedenciaConfirmacao = c.HorasAntecedenciaConfirmacao,
        DataAtualizacao = c.DataAtualizacao
    };
}
