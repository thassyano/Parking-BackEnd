using System.ComponentModel.DataAnnotations;

namespace Estacionamento.Api.Application.DTOs;

// === FLUXO ONLINE ===
public class CriarReservaOnlineDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string NomeCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório")]
    public string TelefoneCliente { get; set; } = string.Empty;

    public string? CpfCliente { get; set; }

    [Required(ErrorMessage = "A placa é obrigatória")]
    [MaxLength(10)]
    public string PlacaVeiculo { get; set; } = string.Empty;

    [Required]
    public string TipoVaga { get; set; } = "Coberta";

    [Required]
    public DateTime DataEntrada { get; set; }

    [Required]
    public DateTime DataSaidaPrevista { get; set; }

    [Required]
    [Range(1, 365)]
    public int QtdDias { get; set; } = 1;

    public string? Observacoes { get; set; }
}

public class CriarReservaOnlineLoteDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string NomeCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório")]
    public string TelefoneCliente { get; set; } = string.Empty;

    public string? CpfCliente { get; set; }

    [Required(ErrorMessage = "Informe ao menos uma placa")]
    [MinLength(1, ErrorMessage = "Informe ao menos uma placa")]
    public List<string> PlacasVeiculos { get; set; } = new();

    [Required]
    public string TipoVaga { get; set; } = "Coberta";

    [Required]
    public DateTime DataEntrada { get; set; }

    [Required]
    public DateTime DataSaidaPrevista { get; set; }

    [Required]
    [Range(1, 365)]
    public int QtdDias { get; set; } = 1;

    public string? Observacoes { get; set; }
}

// === FLUXO PRESENCIAL ===
public class CriarReservaPresencialDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string NomeCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório")]
    public string TelefoneCliente { get; set; } = string.Empty;

    public string? CpfCliente { get; set; }

    [Required(ErrorMessage = "A placa é obrigatória")]
    [MaxLength(10)]
    public string PlacaVeiculo { get; set; } = string.Empty;

    [Required]
    public string TipoVaga { get; set; } = "Coberta";

    [Required]
    public DateTime DataEntrada { get; set; }

    [Required]
    public DateTime DataSaidaPrevista { get; set; }

    [Required]
    [Range(1, 365)]
    public int QtdDias { get; set; } = 1;

    public string? Observacoes { get; set; }
}

// === Associar placa quando online chega ===
public class AssociarPlacaDto
{
    [Required(ErrorMessage = "A placa é obrigatória")]
    [MaxLength(10)]
    public string PlacaVeiculo { get; set; } = string.Empty;
}

// === Checkout ===
public class CheckoutDto
{
    [Required(ErrorMessage = "A forma de pagamento é obrigatória")]
    public string FormaPagamento { get; set; } = string.Empty;
}

// === Response ===
public class ReservaResponseDto
{
    public int Id { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string TelefoneCliente { get; set; } = string.Empty;
    public string? CpfCliente { get; set; }
    public string? PlacaVeiculo { get; set; }
    public string TipoVaga { get; set; } = string.Empty;
    public DateTime DataEntrada { get; set; }
    public int QtdDias { get; set; }
    public DateTime DataSaidaPrevista { get; set; }
    public decimal ValorDiaria { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal DescontoAplicado { get; set; }
    public decimal ValorFinal { get; set; }
    public string? FormaPagamento { get; set; }
    public bool Pago { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public DateTime? DataCheckin { get; set; }
    public DateTime? DataCheckout { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class ReservaLoteResponseDto
{
    public List<ReservaResponseDto> Reservas { get; set; } = new();
    public int QuantidadeVeiculos { get; set; }
    public decimal ValorTotalCartao { get; set; }
    public decimal ValorTotalPixDinheiro { get; set; }
    public decimal EconomiaTotal { get; set; }
    public WhatsAppRedirectDto? WhatsApp { get; set; }
}

public class FiltroReservaDto
{
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? Status { get; set; }
    public string? TipoVaga { get; set; }
}
