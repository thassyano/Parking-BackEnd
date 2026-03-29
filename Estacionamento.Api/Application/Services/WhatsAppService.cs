using System.Text;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Application.Services;

public interface IWhatsAppService
{
    Task<WhatsAppRedirectDto> GerarLinkAsync(int reservaId);
    Task<WhatsAppRedirectDto> GerarLinkLoteAsync(IEnumerable<int> reservaIds);
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

        var telefoneFormatado = ObterTelefoneWhatsApp(config);
        var template = config.MensagemWhatsApp
            ?? "Olá! Fiz uma reserva no estacionamento.\n\nID: {id}\nNome: {nome}\nPlaca: {placa}\nEntrada: {entrada}\nHorário entrada: {horarioEntrada}\nSaída prevista: {saida}\nTipo: {tipo}\nDias: {dias}\nValor diária: R$ {valorDiaria}\nValor total: R$ {valorTotal}";

        var mensagem = template
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

        return CriarRedirect(config.TelefoneWhatsApp!, telefoneFormatado, mensagem);
    }

    public async Task<WhatsAppRedirectDto> GerarLinkLoteAsync(IEnumerable<int> reservaIds)
    {
        var ids = reservaIds.Distinct().ToList();
        if (ids.Count == 0)
            throw new InvalidOperationException("Nenhuma reserva informada");

        var reservas = new List<Reserva>();
        foreach (var id in ids)
        {
            var reserva = await _reservaRepository.ObterPorIdAsync(id)
                ?? throw new InvalidOperationException($"Reserva {id} não encontrada");
            reservas.Add(reserva);
        }

        var config = await _configuracaoRepository.ObterAsync()
            ?? throw new InvalidOperationException("Configuração do estacionamento não encontrada");

        var telefoneFormatado = ObterTelefoneWhatsApp(config);
        var mensagem = MontarMensagemLote(reservas);

        return CriarRedirect(config.TelefoneWhatsApp!, telefoneFormatado, mensagem);
    }

    private static string MontarMensagemLote(List<Reserva> reservas)
    {
        var primeiraReserva = reservas[0];
        var totalCartao = reservas.Sum(reserva => reserva.ValorTotal);
        var builder = new StringBuilder();

        builder.AppendLine("Olá! Fiz um agendamento com múltiplos veículos.");
        builder.AppendLine();
        builder.AppendLine($"Quantidade de carros: {reservas.Count}");
        builder.AppendLine($"Reservas: {string.Join(", ", reservas.Select(r => $"#{r.Id}"))}");
        builder.AppendLine($"Nome: {primeiraReserva.NomeCliente}");
        builder.AppendLine($"Telefone: {primeiraReserva.TelefoneCliente}");
        builder.AppendLine($"Entrada: {primeiraReserva.DataEntrada:dd/MM/yyyy}");
        builder.AppendLine($"Horário entrada: {primeiraReserva.DataEntrada:HH:mm}");
        builder.AppendLine($"Saída prevista: {primeiraReserva.DataSaidaPrevista:dd/MM/yyyy}");
        builder.AppendLine($"Tipo: {primeiraReserva.TipoVaga}");
        builder.AppendLine($"Dias: {primeiraReserva.QtdDias}");
        builder.AppendLine($"Valor total: R$ {totalCartao:N2}");
        builder.AppendLine("Placas:");

        foreach (var reserva in reservas)
        {
            builder.AppendLine($"- {reserva.PlacaVeiculo}");
        }

        return builder.ToString().Trim();
    }

    private static string ObterTelefoneWhatsApp(ConfiguracaoEstacionamento config)
    {
        if (string.IsNullOrEmpty(config.TelefoneWhatsApp))
            throw new InvalidOperationException("Telefone WhatsApp não configurado");

        return config.TelefoneWhatsApp
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace("(", string.Empty)
            .Replace(")", string.Empty)
            .Replace("+", string.Empty);
    }

    private static WhatsAppRedirectDto CriarRedirect(string telefoneOriginal, string telefoneFormatado, string mensagem)
    {
        var url = $"https://wa.me/{telefoneFormatado}?text={Uri.EscapeDataString(mensagem)}";

        return new WhatsAppRedirectDto
        {
            Url = url,
            Mensagem = mensagem,
            TelefoneEstacionamento = telefoneOriginal
        };
    }
}
