using Microsoft.AspNetCore.Mvc;
using Estacionamento.Api.Application.Services;

namespace Estacionamento.Api.Controllers;

/// <summary>Listas para os dropdowns de marca, modelo e cor (públicas: o cliente usa ao reservar).</summary>
[ApiController]
[Route("api/[controller]")]
public class VeiculosController : ControllerBase
{
    private readonly IVeiculoCatalogoService _catalogo;

    public VeiculosController(IVeiculoCatalogoService catalogo)
    {
        _catalogo = catalogo;
    }

    /// <summary>Marcas de carros (lista vazia se o serviço externo estiver indisponível)</summary>
    [HttpGet("marcas")]
    public async Task<IActionResult> Marcas(CancellationToken ct) =>
        Ok(await _catalogo.ObterMarcasAsync(ct));

    /// <summary>Modelos de uma marca, pelo código retornado em /marcas</summary>
    [HttpGet("marcas/{codigo}/modelos")]
    public async Task<IActionResult> Modelos(string codigo, CancellationToken ct) =>
        Ok(await _catalogo.ObterModelosAsync(codigo, ct));

    /// <summary>Cores disponíveis</summary>
    [HttpGet("cores")]
    public IActionResult Cores() => Ok(_catalogo.ObterCores());
}
