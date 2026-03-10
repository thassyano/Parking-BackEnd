using Estacionamento.Api.Helpers;

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
    Cartao,
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

    // Dados do cliente
    public string NomeCliente { get; set; } = string.Empty;
    public string TelefoneCliente { get; set; } = string.Empty;
    public string? CpfCliente { get; set; }

    // Dados do veículo
    public string? PlacaVeiculo { get; set; }

    // Reserva
    public TipoVaga TipoVaga { get; set; }
    public DateTime DataEntrada { get; set; }
    public int QtdDias { get; set; } = 1;
    public DateTime DataSaidaPrevista { get; set; }

    // Valores
    public decimal ValorDiaria { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal DescontoAplicado { get; set; }
    public decimal ValorFinal { get; set; }

    // Pagamento (na saída)
    public FormaPagamento? FormaPagamento { get; set; }
    public bool Pago { get; set; }
    public DateTime? DataPagamento { get; set; }

    // Status e fluxo
    public StatusReserva Status { get; set; } = StatusReserva.Pendente;
    public OrigemReserva Origem { get; set; } = OrigemReserva.Online;
    public DateTime? DataCheckin { get; set; }
    public DateTime? DataCheckout { get; set; }

    public string? Observacoes { get; set; }
    public DateTime DataCriacao { get; set; } = DateTimeHelper.AgoraBrasilia();
}
