using Estacionamento.Api.Domain.Entities;

namespace Estacionamento.Api.Helpers;

public static class BloqueioMensagem
{
    /// <summary>Texto mostrado ao cliente quando o período pedido cai num bloqueio de reservas online.</summary>
    public static string Montar(BloqueioReserva b) =>
        $"Reservas online indisponíveis de {b.DataInicio:dd/MM/yyyy HH:mm} até {b.DataFim:dd/MM/yyyy HH:mm}. " +
        "Para reservar nesse período, entre em contato com o estacionamento.";
}
