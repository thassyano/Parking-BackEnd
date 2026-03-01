namespace Estacionamento.Api.Domain.Entities;

public enum StatusPagamento
{
    Pendente,
    Pago,
    Cancelado
}

public class Pagamento
{
    public int Id { get; set; }
    public int ReservaId { get; set; }
    public Reserva Reserva { get; set; } = null!;

    public decimal Valor { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public StatusPagamento Status { get; set; } = StatusPagamento.Pendente;

    public DateTime DataPagamento { get; set; } = DateTime.UtcNow;
    public string? Comprovante { get; set; } // referência ou código do comprovante
    public string? Observacao { get; set; }
}
