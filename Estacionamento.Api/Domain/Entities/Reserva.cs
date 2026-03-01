namespace Estacionamento.Api.Domain.Entities;

public enum StatusReserva
{
    Pendente,
    Confirmada,
    CheckinRealizado,
    CheckoutRealizado,
    Cancelada
}

public enum FormaPagamento
{
    Pix,
    CartaoCredito,
    CartaoDebito,
    Dinheiro
}

public enum OrigemReserva
{
    Online,
    Presencial
}

public class Reserva
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public TipoVaga TipoVaga { get; set; }
    public DateTime DataReserva { get; set; }
    public int QtdDias { get; set; } = 1;
    public DateTime DataFim { get; set; }

    public decimal ValorDiaria { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal? DescontoAplicado { get; set; }
    public decimal ValorFinal { get; set; }

    public FormaPagamento? FormaPagamento { get; set; }
    public StatusReserva Status { get; set; } = StatusReserva.Pendente;
    public OrigemReserva Origem { get; set; } = OrigemReserva.Online;

    public DateTime? DataCheckin { get; set; }
    public DateTime? DataCheckout { get; set; }

    public bool ConfirmacaoEnviada { get; set; }
    public DateTime? DataConfirmacao { get; set; }

    public string? Observacoes { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();
}
