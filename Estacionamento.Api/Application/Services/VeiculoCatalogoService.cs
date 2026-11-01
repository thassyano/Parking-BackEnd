using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Estacionamento.Api.Application.DTOs;

namespace Estacionamento.Api.Application.Services;

public interface IVeiculoCatalogoService
{
    Task<IReadOnlyList<ItemCatalogoDto>> ObterMarcasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ItemCatalogoDto>> ObterModelosAsync(string codigoMarca, CancellationToken ct = default);
    IReadOnlyList<string> ObterCores();
}

/// <summary>
/// Catálogo de marcas e modelos vindo da API pública da FIPE, com cache em memória.
/// Se a FIPE estiver fora do ar devolve lista vazia (e não guarda no cache),
/// e o front passa a aceitar digitação livre.
/// </summary>
public class VeiculoCatalogoService : IVeiculoCatalogoService
{
    private const string BaseUrl = "https://fipe.parallelum.com.br/api/v2/cars";
    private static readonly TimeSpan CacheDuracao = TimeSpan.FromHours(24);

    private static readonly string[] Cores =
    [
        "Branco", "Preto", "Prata", "Cinza", "Vermelho", "Azul", "Verde",
        "Amarelo", "Laranja", "Marrom", "Bege", "Dourado", "Vinho", "Roxo", "Rosa"
    ];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<VeiculoCatalogoService> _logger;

    public VeiculoCatalogoService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<VeiculoCatalogoService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
    }

    public IReadOnlyList<string> ObterCores() => Cores;

    public Task<IReadOnlyList<ItemCatalogoDto>> ObterMarcasAsync(CancellationToken ct = default) =>
        ObterListaAsync("marcas", $"{BaseUrl}/brands", ct);

    public Task<IReadOnlyList<ItemCatalogoDto>> ObterModelosAsync(string codigoMarca, CancellationToken ct = default)
    {
        // O código vai na URL: aceita apenas dígitos para não montar chamadas arbitrárias.
        if (string.IsNullOrWhiteSpace(codigoMarca) || !codigoMarca.All(char.IsDigit))
            return Task.FromResult<IReadOnlyList<ItemCatalogoDto>>(Array.Empty<ItemCatalogoDto>());

        return ObterListaAsync($"modelos:{codigoMarca}", $"{BaseUrl}/brands/{codigoMarca}/models", ct);
    }

    private async Task<IReadOnlyList<ItemCatalogoDto>> ObterListaAsync(string chave, string url, CancellationToken ct)
    {
        var cacheKey = $"veiculo-catalogo:{chave}";
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<ItemCatalogoDto>? emCache) && emCache != null)
            return emCache;

        try
        {
            var client = _httpClientFactory.CreateClient("Fipe");
            using var response = await client.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            var itens = doc.RootElement.EnumerateArray()
                .Select(e => new ItemCatalogoDto(
                    e.GetProperty("code").GetString() ?? string.Empty,
                    e.GetProperty("name").GetString() ?? string.Empty))
                .Where(i => i.Codigo.Length > 0 && i.Nome.Length > 0)
                .OrderBy(i => i.Nome, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            if (itens.Count > 0)
                _cache.Set(cacheKey, (IReadOnlyList<ItemCatalogoDto>)itens, CacheDuracao);

            return itens;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Falha ao consultar o catálogo de veículos ({Url})", url);
            return Array.Empty<ItemCatalogoDto>();
        }
    }
}
