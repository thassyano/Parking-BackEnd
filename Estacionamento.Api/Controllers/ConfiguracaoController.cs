using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Application.Services;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfiguracaoController : ControllerBase
{
    private readonly IConfiguracaoRepository _configuracaoRepository;
    private readonly IWhatsAppService _whatsAppService;

    public ConfiguracaoController(
        IConfiguracaoRepository configuracaoRepository,
        IWhatsAppService whatsAppService)
    {
        _configuracaoRepository = configuracaoRepository;
        _whatsAppService = whatsAppService;
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
        if (dto.TelefoneWhatsApp != null) config.TelefoneWhatsApp = dto.TelefoneWhatsApp;
        if (dto.MensagemWhatsApp != null) config.MensagemWhatsApp = dto.MensagemWhatsApp;
        if (dto.EvolutionApiUrl != null) config.EvolutionApiUrl = dto.EvolutionApiUrl;
        if (!string.IsNullOrWhiteSpace(dto.EvolutionApiKey)) config.EvolutionApiKey = dto.EvolutionApiKey;
        if (dto.EvolutionInstanceName != null) config.EvolutionInstanceName = dto.EvolutionInstanceName;
        if (dto.UrlConfirmacaoFrontend != null) config.UrlConfirmacaoFrontend = dto.UrlConfirmacaoFrontend;
        if (dto.HorasAntecedenciaConfirmacao.HasValue) config.HorasAntecedenciaConfirmacao = dto.HorasAntecedenciaConfirmacao.Value;

        var atualizada = await _configuracaoRepository.CriarOuAtualizarAsync(config);
        return Ok(MapToResponse(atualizada));
    }

    /// <summary>Envia mensagem de teste via Evolution API para validar integração.</summary>
    [HttpPost("testar-evolution")]
    public async Task<IActionResult> TestarEvolution([FromBody] TestarEvolutionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var config = await _configuracaoRepository.ObterAsync();
        if (config == null)
            return NotFound(new { message = "Configuração não encontrada." });

        var (enviado, erroEvolution) = await _whatsAppService.EnviarMensagemTesteAsync(dto.TelefoneCliente, config);
        if (!enviado)
        {
            return BadRequest(new
            {
                message = "Não foi possível enviar. Verifique instância conectada, nome da instância e número de destino (com DDI 55).",
                evolutionErro = erroEvolution,
                evolutionConfigurada = MapToResponse(config).EvolutionConfigurada,
                instanceName = config.EvolutionInstanceName
            });
        }

        return Ok(new { message = "Mensagem de teste enviada. Verifique o WhatsApp do número informado." });
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
