using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Application.Services;

public interface IWhatsAppService
{
    Task<WhatsAppRedirectDto> GerarLinkAsync(int reservaId);
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
            ?? "Olá! Fiz uma reserva no estacionamento.\n\nID: {id}\nNome: {nome}\nEntrada: {entrada}\nSaída prevista: {saida}\nTipo: {tipo}\nDias: {dias}\nValor diária: R$ {valorDiaria}";

        var mensagem = template
            .Replace("{id}", reserva.Id.ToString())
            .Replace("{nome}", reserva.NomeCliente)
            .Replace("{entrada}", reserva.DataEntrada.ToString("dd/MM/yyyy"))
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
}
