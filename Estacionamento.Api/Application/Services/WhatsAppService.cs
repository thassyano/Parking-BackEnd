using System.Text;
using System.Text.Json;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Application.Services;

public interface IWhatsAppService
{
    Task<WhatsAppRedirectDto> GerarLinkAsync(int reservaId);
    Task<WhatsAppRedirectDto> GerarLinkLoteAsync(List<int> reservaIds);
    Task<WhatsAppRedirectDto> GerarLinkAlteracaoAsync(int reservaId);
    Task<WhatsAppRedirectDto> GerarLinkAlteracaoLoteAsync(List<int> reservaIds);
    Task<bool> EnviarConfirmacaoViaEvolutionAsync(Reserva reserva, ConfiguracaoEstacionamento config);
    Task<(bool Sucesso, string? Erro)> EnviarMensagemTesteAsync(string telefoneCliente, ConfiguracaoEstacionamento config);
}

public class WhatsAppService : IWhatsAppService
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IConfiguracaoRepository _configuracaoRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(
        IReservaRepository reservaRepository,
        IConfiguracaoRepository configuracaoRepository,
        IHttpClientFactory httpClientFactory,
        ILogger<WhatsAppService> logger)
    {
        _reservaRepository = reservaRepository;
        _configuracaoRepository = configuracaoRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<bool> EnviarConfirmacaoViaEvolutionAsync(Reserva reserva, ConfiguracaoEstacionamento config)
    {
        if (!EvolutionConfigurada(config))
        {
            _logger.LogWarning("Evolution API não configurada (URL, key ou instância ausente).");
            return false;
        }

        var telefone = FormatarTelefoneInternacional(reserva.TelefoneCliente);
        if (string.IsNullOrEmpty(telefone))
        {
            _logger.LogWarning("Telefone inválido para reserva {Id}: {Telefone}.", reserva.Id, reserva.TelefoneCliente);
            return false;
        }

        var linkConfirmacao = string.IsNullOrEmpty(config.UrlConfirmacaoFrontend)
            ? $"(configure UrlConfirmacaoFrontend - token: {reserva.ConfirmacaoToken})"
            : $"{config.UrlConfirmacaoFrontend.TrimEnd('/')}/confirmar?token={reserva.ConfirmacaoToken}";

        var nomeEstacionamento = string.IsNullOrEmpty(config.NomeEstacionamento)
            ? "Estacionamento"
            : config.NomeEstacionamento;

        var mensagem = $"Olá {reserva.NomeCliente}! 🅿️\n\n" +
            $"Você tem uma reserva no *{nomeEstacionamento}*:\n\n" +
            $"📅 Entrada: {reserva.DataEntrada:dd/MM/yyyy}\n" +
            $"🚗 Placa: {reserva.PlacaVeiculo ?? "-"}\n" +
            $"🔑 Vaga: {reserva.TipoVaga}\n" +
            $"📆 Dias: {reserva.QtdDias}\n\n" +
            $"✅ *Para CONFIRMAR sua reserva, clique aqui:*\n{linkConfirmacao}\n\n" +
            $"⚠️ Se não confirmar antes da data de entrada, a reserva será cancelada automaticamente.\n\n" +
            $"Dúvidas? Entre em contato\n\n" +
            $"*Atenção*, se tiver reservado mais de um veiculo, será necessário confirmar cada um.";

        var (sucesso, _) = await EnviarTextoEvolutionAsync(telefone, mensagem, config);
        return sucesso;
    }

    public async Task<(bool Sucesso, string? Erro)> EnviarMensagemTesteAsync(string telefoneCliente, ConfiguracaoEstacionamento config)
    {
        if (!EvolutionConfigurada(config))
            return (false, "Evolution API não configurada (URL, key ou instância).");

        var telefone = FormatarTelefoneInternacional(telefoneCliente);
        if (string.IsNullOrEmpty(telefone))
            return (false, "Telefone inválido. Use DDD + número (10 ou 11 dígitos) ou 55 + DDD + número.");

        var nome = string.IsNullOrEmpty(config.NomeEstacionamento) ? "Estacionamento" : config.NomeEstacionamento;
        var mensagem = $"✅ Teste de integração WhatsApp — *{nome}*.\n\nSe você recebeu esta mensagem, a Evolution API está funcionando.";

        return await EnviarTextoEvolutionAsync(telefone, mensagem, config);
    }

    private async Task<(bool Sucesso, string? Erro)> EnviarTextoEvolutionAsync(string telefone, string mensagem, ConfiguracaoEstacionamento config)
    {
        var url = $"{config.EvolutionApiUrl!.TrimEnd('/')}/message/sendText/{config.EvolutionInstanceName}";

        var client = _httpClientFactory.CreateClient("EvolutionApi");
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("apikey", config.EvolutionApiKey);

        // v2.3+ (evoapicloud): { number, text } | v2.2.x (atendai): { number, textMessage: { text } }
        var formatos = new object[]
        {
            new { number = telefone, text = mensagem },
            new { number = telefone, textMessage = new { text = mensagem } }
        };

        string? ultimoErro = null;

        foreach (var payload in formatos)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Evolution sendText OK para {Telefone}.", telefone);
                    return (true, null);
                }

                ultimoErro = $"HTTP {(int)response.StatusCode}: {body}";
                _logger.LogWarning("Evolution sendText tentativa falhou: {Erro}. URL: {Url}", ultimoErro, url);

                // 401/404 não adianta tentar outro formato
                if ((int)response.StatusCode is 401 or 404)
                    break;
            }
            catch (Exception ex)
            {
                ultimoErro = ex.Message;
                _logger.LogError(ex, "Erro ao chamar Evolution API em {Url}.", url);
            }
        }

        return (false, ultimoErro);
    }

    private static bool EvolutionConfigurada(ConfiguracaoEstacionamento config) =>
        !string.IsNullOrEmpty(config.EvolutionApiUrl)
        && !string.IsNullOrEmpty(config.EvolutionApiKey)
        && !string.IsNullOrEmpty(config.EvolutionInstanceName);

    private static string? FormatarTelefoneInternacional(string telefone)
    {
        var digits = new string(telefone.Where(char.IsDigit).ToArray());

        if (string.IsNullOrEmpty(digits)) return null;

        if (digits.StartsWith("55") && digits.Length >= 12)
            return digits;

        if (digits.Length is 10 or 11)
            return "55" + digits;

        return null;
    }

    private const string TemplateReservaPadrao =
        "Olá! Fiz uma reserva no estacionamento.\n\nID: {id}\nNome: {nome}\nPlaca: {placa}\nEntrada: {entrada}\nHorário entrada: {horarioEntrada}\nSaída prevista: {saida}\nHorário saída: {horarioSaida}\nTipo: {tipo}\nDias: {dias}\nValor diária: R$ {valorDiaria}\nValor total: R$ {valorTotal}";

    private const string TemplateAlteracaoPadrao =
        "Olá! *Alterei* minha reserva no estacionamento.\n\nID: {id}\nNome: {nome}\nPlaca: {placa}\nEntrada: {entrada}\nHorário entrada: {horarioEntrada}\n*Nova saída prevista:* {saida}\nHorário saída: {horarioSaida}\nTipo: {tipo}\nDias: {dias}\nValor diária: R$ {valorDiaria}\nValor total: R$ {valorTotal}";

    public async Task<WhatsAppRedirectDto> GerarLinkAsync(int reservaId)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(reservaId)
            ?? throw new InvalidOperationException("Reserva não encontrada");

        var config = await ObterConfigComWhatsAppAsync();
        var template = config.MensagemWhatsApp ?? TemplateReservaPadrao;
        var mensagem = AplicarTemplate(reserva, template);

        return CriarRedirect(config, mensagem);
    }

    public async Task<WhatsAppRedirectDto> GerarLinkLoteAsync(List<int> reservaIds)
    {
        var config = await ObterConfigComWhatsAppAsync();
        var template = config.MensagemWhatsApp ?? TemplateReservaPadrao;

        var blocos = new List<string>();
        decimal valorTotalGeral = 0;
        string nomeCliente = "";

        for (int i = 0; i < reservaIds.Count; i++)
        {
            var reserva = await _reservaRepository.ObterPorIdAsync(reservaIds[i])
                ?? throw new InvalidOperationException($"Reserva {reservaIds[i]} não encontrada");

            if (i == 0) nomeCliente = reserva.NomeCliente;
            valorTotalGeral += reserva.ValorTotal;
            blocos.Add($"🚗 Veículo {i + 1}\n{AplicarTemplate(reserva, template)}");
        }

        var cabecalho = $"Olá! Fiz uma reserva para {reservaIds.Count} veículos.\nNome: {nomeCliente}\n";
        var rodape = $"\n💰 Valor total geral: R$ {valorTotalGeral:N2}";
        var mensagem = cabecalho + "\n" + string.Join("\n\n---\n\n", blocos) + rodape;

        return CriarRedirect(config, mensagem);
    }

    public async Task<WhatsAppRedirectDto> GerarLinkAlteracaoAsync(int reservaId)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(reservaId)
            ?? throw new InvalidOperationException("Reserva não encontrada");

        var config = await ObterConfigComWhatsAppAsync();
        var mensagem = AplicarTemplate(reserva, TemplateAlteracaoPadrao);

        return CriarRedirect(config, mensagem);
    }

    public async Task<WhatsAppRedirectDto> GerarLinkAlteracaoLoteAsync(List<int> reservaIds)
    {
        var config = await ObterConfigComWhatsAppAsync();

        var blocos = new List<string>();
        decimal valorTotalGeral = 0;
        string nomeCliente = "";

        for (int i = 0; i < reservaIds.Count; i++)
        {
            var reserva = await _reservaRepository.ObterPorIdAsync(reservaIds[i])
                ?? throw new InvalidOperationException($"Reserva {reservaIds[i]} não encontrada");

            if (i == 0) nomeCliente = reserva.NomeCliente;
            valorTotalGeral += reserva.ValorTotal;
            blocos.Add($"🚗 Veículo {i + 1}\n{AplicarTemplate(reserva, TemplateAlteracaoPadrao)}");
        }

        var cabecalho = $"Olá! *Alterei* minhas reservas ({reservaIds.Count} veículos).\nNome: {nomeCliente}\n";
        var rodape = $"\n💰 Valor total geral: R$ {valorTotalGeral:N2}";
        var mensagem = cabecalho + "\n" + string.Join("\n\n---\n\n", blocos) + rodape;

        return CriarRedirect(config, mensagem);
    }

    private async Task<ConfiguracaoEstacionamento> ObterConfigComWhatsAppAsync()
    {
        var config = await _configuracaoRepository.ObterAsync()
            ?? throw new InvalidOperationException("Configuração do estacionamento não encontrada");

        if (string.IsNullOrEmpty(config.TelefoneWhatsApp))
            throw new InvalidOperationException("Telefone WhatsApp não configurado");

        return config;
    }

    private static string AplicarTemplate(Reserva reserva, string template) =>
        template
            .Replace("{id}", reserva.Id.ToString())
            .Replace("{nome}", reserva.NomeCliente)
            .Replace("{placa}", reserva.PlacaVeiculo ?? "-")
            .Replace("{entrada}", reserva.DataEntrada.ToString("dd/MM/yyyy"))
            .Replace("{horarioEntrada}", reserva.DataEntrada.ToString("HH:mm"))
            .Replace("{saida}", reserva.DataSaidaPrevista.ToString("dd/MM/yyyy"))
            .Replace("{horarioSaida}", reserva.DataSaidaPrevista.ToString("HH:mm"))
            .Replace("{tipo}", reserva.TipoVaga.ToString())
            .Replace("{dias}", reserva.QtdDias.ToString())
            .Replace("{valorDiaria}", reserva.ValorDiaria.ToString("N2"))
            .Replace("{valorTotal}", reserva.ValorTotal.ToString("N2"));

    private static WhatsAppRedirectDto CriarRedirect(ConfiguracaoEstacionamento config, string mensagem)
    {
        var telefoneFormatado = config.TelefoneWhatsApp!
            .Replace(" ", "").Replace("-", "")
            .Replace("(", "").Replace(")", "").Replace("+", "");

        var url = $"https://wa.me/{telefoneFormatado}?text={Uri.EscapeDataString(mensagem)}";

        return new WhatsAppRedirectDto
        {
            Url = url,
            Mensagem = mensagem,
            TelefoneEstacionamento = config.TelefoneWhatsApp!
        };
    }
}
