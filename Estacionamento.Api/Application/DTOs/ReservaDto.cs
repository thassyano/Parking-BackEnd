using System.ComponentModel.DataAnnotations;

namespace Estacionamento.Api.Application.DTOs;

public class CriarReservaOnlineDto
{
    [Required(ErrorMessage = "O nome e obrigatorio")]
    [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "O nome do cliente deve conter apenas letras")]
    public string NomeCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone e obrigatorio")]
    [RegularExpression(@"^\(\d{2}\)\s\d{9}$", ErrorMessage = "Telefone deve estar no formato (00) 000000000")]
    public string TelefoneCliente { get; set; } = string.Empty;

    public string? CpfCliente { get; set; }

    [Required(ErrorMessage = "A placa e obrigatoria")]
    [MaxLength(7, ErrorMessage = "Placa do veiculo deve ter no maximo 7 caracteres")]
    [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "A placa do veiculo deve conter apenas caracteres alfanumericos")]
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

public class CriarReservaPresencialDto
{
    [Required(ErrorMessage = "O nome e obrigatorio")]
    [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "O nome do cliente deve conter apenas letras")]
    public string NomeCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone e obrigatorio")]
    [RegularExpression(@"^\(\d{2}\)\s\d{9}$", ErrorMessage = "Telefone deve estar no formato (00) 000000000")]
    public string TelefoneCliente { get; set; } = string.Empty;

    public string? CpfCliente { get; set; }

    [Required(ErrorMessage = "A placa e obrigatoria")]
    [MaxLength(7, ErrorMessage = "Placa do veiculo deve ter no maximo 7 caracteres")]
    [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "A placa do veiculo deve conter apenas caracteres alfanumericos")]
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

public class AssociarPlacaDto
{
    [Required(ErrorMessage = "A placa e obrigatoria")]
    [MaxLength(7, ErrorMessage = "Placa do veiculo deve ter no maximo 7 caracteres")]
    [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "A placa do veiculo deve conter apenas caracteres alfanumericos")]
    public string PlacaVeiculo { get; set; } = string.Empty;
}

public class CheckoutDto
{
    [Required(ErrorMessage = "A forma de pagamento e obrigatoria")]
    public string FormaPagamento { get; set; } = string.Empty;
}

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

public class FiltroReservaDto
{
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? Status { get; set; }
    public string? TipoVaga { get; set; }
    public string? PlacaVeiculo { get; set; }
}

public class CarroLoteDto
{
    [MaxLength(7, ErrorMessage = "Placa do veiculo deve ter no maximo 7 caracteres")]
    [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "A placa do veiculo deve conter apenas caracteres alfanumericos")]
    public string? PlacaVeiculo { get; set; }

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

public class CriarReservaLoteOnlineDto
{
    [Required(ErrorMessage = "O nome e obrigatorio")]
    [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "O nome do cliente deve conter apenas letras")]
    public string NomeCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone e obrigatorio")]
    [RegularExpression(@"^\(\d{2}\)\s\d{9}$", ErrorMessage = "Telefone deve estar no formato (00) 000000000")]
    public string TelefoneCliente { get; set; } = string.Empty;

    public string? CpfCliente { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Informe pelo menos um veiculo")]
    public List<CarroLoteDto> Carros { get; set; } = new();
}

public class ReservaLoteResponseDto
{
    public List<ReservaResponseDto> Reservas { get; set; } = new();
    public int TotalReservas { get; set; }
    public decimal ValorTotalGeral { get; set; }
}

public class CarroPresencialLoteDto
{
    [Required(ErrorMessage = "A placa e obrigatoria")]
    [MaxLength(7, ErrorMessage = "Placa do veiculo deve ter no maximo 7 caracteres")]
    [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "A placa do veiculo deve conter apenas caracteres alfanumericos")]
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

public class CriarReservaLotePresencialDto
{
    [Required(ErrorMessage = "O nome e obrigatorio")]
    [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "O nome do cliente deve conter apenas letras")]
    public string NomeCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone e obrigatorio")]
    [RegularExpression(@"^\(\d{2}\)\s\d{9}$", ErrorMessage = "Telefone deve estar no formato (00) 000000000")]
    public string TelefoneCliente { get; set; } = string.Empty;

    public string? CpfCliente { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Informe pelo menos um veiculo")]
    public List<CarroPresencialLoteDto> Carros { get; set; } = new();
}
