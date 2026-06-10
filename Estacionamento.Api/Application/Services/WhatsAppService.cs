using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Application.Services;

public interface IWhatsAppService
{
    Task<WhatsAppRedirectDto> GerarLinkAsync(int reservaId);
    Task<WhatsAppRedirectDto> GerarLinkLoteAsync(List<int> reservaIds);
}

public class WhatsAppService : IWhatsAppService
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IConfiguracaoRepository _configuracaoRepository;

    public WhatsAppService(
        IReservaRepository reservaRepository,
        IConfiguracaoRepository configuracaoRepository)
    {
        _reservaRepository = reservaRepository;
        _configuracaoRepository = configuracaoRepository;
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
            ?? "Olá! Fiz uma reserva no estacionamento.\n\nID: {id}\nNome: {nome}\nPlaca: {placa}\nEntrada: {entrada}\nHorário entrada: {horarioEntrada}\nSaída prevista: {saida}\nTipo: {tipo}\nDias: {dias}\nValor diária: R$ {valorDiaria}";

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
            ?? "Olá! Fiz uma reserva no estacionamento.\n\nID: {id}\nNome: {nome}\nPlaca: {placa}\nEntrada: {entrada}\nHorário entrada: {horarioEntrada}\nSaída prevista: {saida}\nTipo: {tipo}\nDias: {dias}\nValor diária: R$ {valorDiaria}";

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
