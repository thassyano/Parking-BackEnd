using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Application.Services;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Helpers;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Controllers;

/// <summary>
/// Bloqueio de novas reservas ONLINE em um período. Reservas presenciais continuam permitidas.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BloqueiosController : ControllerBase
{
    private readonly IBloqueioReservaRepository _repository;
    private readonly ILogAtividadeService _logService;

    public BloqueiosController(IBloqueioReservaRepository repository, ILogAtividadeService logService)
    {
        _repository = repository;
        _logService = logService;
    }

    /// <summary>Lista os bloqueios ativos que ainda não terminaram</summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Listar()
    {
        var bloqueios = await _repository.ObterAtivosAsync();
        return Ok(bloqueios.Select(MapToResponse));
    }

    /// <summary>Público: informa se o período escolhido cai em um bloqueio (usado na tela do cliente)</summary>
    [HttpGet("verificar")]
    public async Task<IActionResult> Verificar([FromQuery] DateTime inicio, [FromQuery] DateTime fim)
    {
        if (fim <= inicio)
            return Ok(new BloqueioStatusDto { Bloqueado = false });

        var bloqueio = await _repository.ObterQueCruzaAsync(inicio, fim);
        if (bloqueio == null)
            return Ok(new BloqueioStatusDto { Bloqueado = false });

        return Ok(new BloqueioStatusDto
        {
            Bloqueado = true,
            Mensagem = BloqueioMensagem.Montar(bloqueio),
            DataInicio = bloqueio.DataInicio,
            DataFim = bloqueio.DataFim
        });
    }

    /// <summary>Cria um bloqueio de reservas online</summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Criar([FromBody] CriarBloqueioReservaDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (dto.DataFim <= dto.DataInicio)
            return BadRequest(new { message = "A data final deve ser depois da data inicial" });

        var bloqueio = await _repository.CriarAsync(new BloqueioReserva
        {
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim,
            Motivo = string.IsNullOrWhiteSpace(dto.Motivo) ? null : dto.Motivo.Trim(),
            CriadoPor = User.FindFirst(ClaimTypes.Name)?.Value
        });

        await _logService.RegistrarAsync(LogAtividadeHttpExtensions.CriarRegistro(
            User, AcaoLog.BloqueioCriado,
            $"Reservas online bloqueadas de {bloqueio.DataInicio:dd/MM/yyyy HH:mm} até {bloqueio.DataFim:dd/MM/yyyy HH:mm}",
            "BloqueioReserva", bloqueio.Id));

        return CreatedAtAction(nameof(Listar), MapToResponse(bloqueio));
    }

    /// <summary>Remove (desativa) um bloqueio</summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Remover(int id)
    {
        var bloqueio = await _repository.ObterPorIdAsync(id);
        if (bloqueio == null || !bloqueio.Ativo)
            return NotFound(new { message = "Bloqueio não encontrado" });

        bloqueio.Ativo = false;
        await _repository.AtualizarAsync(bloqueio);

        await _logService.RegistrarAsync(LogAtividadeHttpExtensions.CriarRegistro(
            User, AcaoLog.BloqueioRemovido,
            $"Bloqueio de {bloqueio.DataInicio:dd/MM/yyyy HH:mm} até {bloqueio.DataFim:dd/MM/yyyy HH:mm} removido",
            "BloqueioReserva", bloqueio.Id));

        return NoContent();
    }

    private static BloqueioReservaResponseDto MapToResponse(BloqueioReserva b) => new()
    {
        Id = b.Id,
        DataInicio = b.DataInicio,
        DataFim = b.DataFim,
        Motivo = b.Motivo,
        Ativo = b.Ativo,
        CriadoPor = b.CriadoPor,
        DataCriacao = b.DataCriacao
    };
}
