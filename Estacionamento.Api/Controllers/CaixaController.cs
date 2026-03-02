using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Application.Services;

namespace Estacionamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CaixaController : ControllerBase
{
    private readonly ICaixaService _caixaService;

    public CaixaController(ICaixaService caixaService)
    {
        _caixaService = caixaService;
    }

    [HttpPost("fechamento")]
    public async Task<IActionResult> Fechamento([FromBody] FiltroFechamentoCaixaDto filtro)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (filtro.DataFim < filtro.DataInicio)
            return BadRequest(new { message = "Data fim deve ser maior ou igual à data início" });

        var resultado = await _caixaService.GerarFechamentoAsync(filtro);
        return Ok(resultado);
    }

    [HttpPost("exportar-excel")]
    public async Task<IActionResult> ExportarExcel([FromBody] FiltroFechamentoCaixaDto filtro)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (filtro.DataFim < filtro.DataInicio)
            return BadRequest(new { message = "Data fim deve ser maior ou igual à data início" });

        var arquivo = await _caixaService.ExportarExcelAsync(filtro);

        var nomeArquivo = $"fechamento-caixa-{filtro.DataInicio:yyyy-MM-dd}-a-{filtro.DataFim:yyyy-MM-dd}.xlsx";
        return File(arquivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nomeArquivo);
    }
}
