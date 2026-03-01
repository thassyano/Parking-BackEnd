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

        return Ok(new ConfiguracaoResponseDto
        {
            Id = config.Id,
            TotalVagasCoberta = config.TotalVagasCoberta,
            TotalVagasDescoberta = config.TotalVagasDescoberta,
            TelefoneWhatsApp = config.TelefoneWhatsApp,
            MensagemWhatsApp = config.MensagemWhatsApp,
            HorasAntecedenciaConfirmacao = config.HorasAntecedenciaConfirmacao,
            DataAtualizacao = config.DataAtualizacao
        });
    }

    [HttpPut]
    public async Task<IActionResult> Atualizar([FromBody] AtualizarConfiguracaoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var config = await _configuracaoRepository.ObterAsync();
        if (config == null)
        {
            config = new ConfiguracaoEstacionamento();
        }

        if (dto.TotalVagasCoberta.HasValue) config.TotalVagasCoberta = dto.TotalVagasCoberta.Value;
        if (dto.TotalVagasDescoberta.HasValue) config.TotalVagasDescoberta = dto.TotalVagasDescoberta.Value;
        if (dto.TelefoneWhatsApp != null) config.TelefoneWhatsApp = dto.TelefoneWhatsApp;
        if (dto.MensagemWhatsApp != null) config.MensagemWhatsApp = dto.MensagemWhatsApp;
        if (dto.HorasAntecedenciaConfirmacao.HasValue) config.HorasAntecedenciaConfirmacao = dto.HorasAntecedenciaConfirmacao.Value;

        var atualizada = await _configuracaoRepository.CriarOuAtualizarAsync(config);

        return Ok(new ConfiguracaoResponseDto
        {
            Id = atualizada.Id,
            TotalVagasCoberta = atualizada.TotalVagasCoberta,
            TotalVagasDescoberta = atualizada.TotalVagasDescoberta,
            TelefoneWhatsApp = atualizada.TelefoneWhatsApp,
            MensagemWhatsApp = atualizada.MensagemWhatsApp,
            HorasAntecedenciaConfirmacao = atualizada.HorasAntecedenciaConfirmacao,
            DataAtualizacao = atualizada.DataAtualizacao
        });
    }
}
