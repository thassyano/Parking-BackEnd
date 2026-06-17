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
    Task<bool> EnviarConfirmacaoViaEvolutionAsync(Reserva reserva, ConfiguracaoEstacionamento config);
}

public class WhatsAppService : IWhatsAppService
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IConfiguracaoRepository _configuracaoRepository;
    private readonly IHttpClientFactory _httpClientFactory;

    public WhatsAppService(
        IReservaRepository reservaRepository,
        IConfiguracaoRepository configuracaoRepository,
        IHttpClientFactory httpClientFactory)
    {
        _reservaRepository = reservaRepository;
        _configuracaoRepository = configuracaoRepository;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> EnviarConfirmacaoViaEvolutionAsync(Reserva reserva, ConfiguracaoEstacionamento config)
    {
        if (string.IsNullOrEmpty(config.EvolutionApiUrl)
            || string.IsNullOrEmpty(config.EvolutionApiKey)
            || string.IsNullOrEmpty(config.EvolutionInstanceName))
            return false;

        var telefone = FormatarTelefoneInternacional(reserva.TelefoneCliente);
        if (string.IsNullOrEmpty(telefone))
            return false;

        var linkConfirmacao = string.IsNullOrEmpty(config.UrlConfirmacaoFrontend)
            ? $"(link de confirmação não configurado - token: {reserva.ConfirmacaoToken})"
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
            $"Dúvidas? {config.TelefoneWhatsApp ?? ""}";

        var payload = new { number = telefone, text = mensagem };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = $"{config.EvolutionApiUrl.TrimEnd('/')}/message/sendText/{config.EvolutionInstanceName}";

        var client = _httpClientFactory.CreateClient("EvolutionApi");
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("apikey", config.EvolutionApiKey);

        var response = await client.PostAsync(url, content);
        return response.IsSuccessStatusCode;
    }

    private static string? FormatarTelefoneInternacional(string telefone)
    {
        var digits = new string(telefone.Where(char.IsDigit).ToArray());

        if (string.IsNullOrEmpty(digits)) return null;

        // Se já começa com 55 e tem 12-13 dígitos, está correto
        if (digits.StartsWith("55") && digits.Length >= 12)
            return digits;

        // 10 ou 11 dígitos = número brasileiro sem código do país
        if (digits.Length is 10 or 11)
            return "55" + digits;

        return null;
    }

    public async Task<WhatsAppRedirectDto> GerarLinkAsync(int reservaId)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(reservaId)
            ?? throw new InvalidOperationException("Reserva não encontrada");

        var config = await _configuracaoRepository.ObterAsync()
            ?? throw new InvalidOperationException("Configuração do estacionamento não encontrada");

        if (string.IsNullOrEmpty(config.TelefoneWhatsApp))
            throw new InvalidOperationException("Telefone WhatsApp não configurado");

        var template = config.MensagemWhatsApp
            ?? "Olá! Fiz uma reserva no estacionamento.\n\nID: {id}\nNome: {nome}\nPlaca: {placa}\nEntrada: {entrada}\nHorário entrada: {horarioEntrada}\nSaída prevista: {saida}\nTipo: {tipo}\nDias: {dias}\nValor diária: R$ {valorDiaria}\n\n📍 Localização: https://maps.app.goo.gl/NKGxJoH1GgiaJHDE9";

        var mensagem = template
            .Replace("{id}", reserva.Id.ToString())
            .Replace("{nome}", reserva.NomeCliente)
            .Replace("{placa}", reserva.PlacaVeiculo)
            .Replace("{entrada}", reserva.DataEntrada.ToString("dd/MM/yyyy"))
            .Replace("{horarioEntrada}", reserva.DataEntrada.ToShortTimeString())
            .Replace("{saida}", reserva.DataSaidaPrevista.ToString("dd/MM/yyyy"))
            .Replace("{tipo}", reserva.TipoVaga.ToString())
            .Replace("{dias}", reserva.QtdDias.ToString())
            .Replace("{valorDiaria}", reserva.ValorDiaria.ToString("N2"))
            .Replace("{valorTotal}", reserva.ValorTotal.ToString("N2"));

        var telefoneFormatado = config.TelefoneWhatsApp
            .Replace(" ", "").Replace("-", "")
            .Replace("(", "").Replace(")", "").Replace("+", "");

        var url = $"https://wa.me/{telefoneFormatado}?text={Uri.EscapeDataString(mensagem)}";

        return new WhatsAppRedirectDto
        {
            Url = url,
            Mensagem = mensagem,
            TelefoneEstacionamento = config.TelefoneWhatsApp
        };
    }

    public async Task<WhatsAppRedirectDto> GerarLinkLoteAsync(List<int> reservaIds)
    {
        var config = await _configuracaoRepository.ObterAsync()
            ?? throw new InvalidOperationException("Configuração do estacionamento não encontrada");

        if (string.IsNullOrEmpty(config.TelefoneWhatsApp))
            throw new InvalidOperationException("Telefone WhatsApp não configurado");

        var template = config.MensagemWhatsApp
            ?? "Olá! Fiz uma reserva no estacionamento.\n\nID: {id}\nNome: {nome}\nPlaca: {placa}\nEntrada: {entrada}\nHorário entrada: {horarioEntrada}\nSaída prevista: {saida}\nTipo: {tipo}\nDias: {dias}\nValor diária: R$ {valorDiaria}\n\n📍 Localização: https://maps.app.goo.gl/NKGxJoH1GgiaJHDE9";

        var blocos = new List<string>();
        decimal valorTotalGeral = 0;
        string nomeCliente = "";

        for (int i = 0; i < reservaIds.Count; i++)
        {
            var reserva = await _reservaRepository.ObterPorIdAsync(reservaIds[i])
                ?? throw new InvalidOperationException($"Reserva {reservaIds[i]} não encontrada");

            if (i == 0) nomeCliente = reserva.NomeCliente;
            valorTotalGeral += reserva.ValorTotal;

            var bloco = template
                .Replace("{id}", reserva.Id.ToString())
                .Replace("{nome}", reserva.NomeCliente)
                .Replace("{placa}", reserva.PlacaVeiculo ?? "-")
                .Replace("{entrada}", reserva.DataEntrada.ToString("dd/MM/yyyy"))
                .Replace("{horarioEntrada}", reserva.DataEntrada.ToShortTimeString())
                .Replace("{saida}", reserva.DataSaidaPrevista.ToString("dd/MM/yyyy"))
                .Replace("{tipo}", reserva.TipoVaga.ToString())
                .Replace("{dias}", reserva.QtdDias.ToString())
                .Replace("{valorDiaria}", reserva.ValorDiaria.ToString("N2"))
                .Replace("{valorTotal}", reserva.ValorTotal.ToString("N2"));

            blocos.Add($"🚗 Veículo {i + 1}\n{bloco}");
        }

        var cabecalho = $"Olá! Fiz uma reserva para {reservaIds.Count} veículos.\nNome: {nomeCliente}\n";
        var rodape = $"\n💰 Valor total geral: R$ {valorTotalGeral:N2}";
        var mensagem = cabecalho + "\n" + string.Join("\n\n---\n\n", blocos) + rodape;

        var telefoneFormatado = config.TelefoneWhatsApp
            .Replace(" ", "").Replace("-", "")
            .Replace("(", "").Replace(")", "").Replace("+", "");

        var url = $"https://wa.me/{telefoneFormatado}?text={Uri.EscapeDataString(mensagem)}";

        return new WhatsAppRedirectDto
        {
            Url = url,
            Mensagem = mensagem,
            TelefoneEstacionamento = config.TelefoneWhatsApp
        };
    }
}
