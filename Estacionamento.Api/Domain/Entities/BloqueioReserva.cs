namespace Estacionamento.Api.Domain.Entities;

/// <summary>
/// Período em que o site NÃO aceita novas reservas online.
/// A reserva presencial (feita por um admin) continua liberada.
/// </summary>
public class BloqueioReserva
{
    public int Id { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string? Motivo { get; set; }
    public bool Ativo { get; set; } = true;
    public string? CriadoPor { get; set; }
    public DateTime DataCriacao { get; set; } = Helpers.DateTimeHelper.AgoraBrasilia();
}
