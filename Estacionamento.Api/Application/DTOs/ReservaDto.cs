using System.ComponentModel.DataAnnotations;

namespace Estacionamento.Api.Application.DTOs;

// === FLUXO ONLINE ===
public class CriarReservaOnlineDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string NomeCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório")]
    [MaxLength(11, ErrorMessage = "Telefone deve ter no máximo 11 caracteres")]
    public string TelefoneCliente { get; set; } = string.Empty;

    public string? CpfCliente { get; set; }

    [Required(ErrorMessage = "A placa é obrigatória")]
    [MaxLength(10, ErrorMessage = "Placa do automóvel deve ter no máximo 10 caracteres")]
    public string PlacaVeiculo { get; set; } = string.Empty;

    [Required]
    public string TipoVaga { get; set; } = "Coberta";

    [Required]
    public DateTime DataEntrada { get; set; }

    [Required]
    public DateTime DataSaidaPrevista { get; set; }

    [Required]
    [Range(0, 365)]
    public int QtdDias { get; set; } = 1;

    public string? Observacoes { get; set; }

    [MaxLength(50, ErrorMessage = "Marca deve ter no máximo 50 caracteres")]
    public string? MarcaVeiculo { get; set; }

    [MaxLength(50, ErrorMessage = "Modelo deve ter no máximo 50 caracteres")]
    public string? ModeloVeiculo { get; set; }

    [MaxLength(30, ErrorMessage = "Cor deve ter no máximo 30 caracteres")]
    public string? CorVeiculo { get; set; }
}

// === FLUXO PRESENCIAL ===
public class CriarReservaPresencialDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string NomeCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório")]
    [MaxLength(11, ErrorMessage = "Telefone deve ter no máximo 11 caracteres")]
    public string TelefoneCliente { get; set; } = string.Empty;

    public string? CpfCliente { get; set; }

    [Required(ErrorMessage = "A placa é obrigatória")]
    [MaxLength(10, ErrorMessage = "Placa do veículo deve ter no máximo 10 caracteres")]
    public string PlacaVeiculo { get; set; } = string.Empty;

    [Required]
    public string TipoVaga { get; set; } = "Coberta";

    [Required]
    public DateTime DataEntrada { get; set; }

    [Required]
    public DateTime DataSaidaPrevista { get; set; }

    [Required]
    [Range(0, 365)]
    public int QtdDias { get; set; } = 1;

    public string? Observacoes { get; set; }

    [MaxLength(50, ErrorMessage = "Marca deve ter no máximo 50 caracteres")]
    public string? MarcaVeiculo { get; set; }

    [MaxLength(50, ErrorMessage = "Modelo deve ter no máximo 50 caracteres")]
    public string? ModeloVeiculo { get; set; }

    [MaxLength(30, ErrorMessage = "Cor deve ter no máximo 30 caracteres")]
    public string? CorVeiculo { get; set; }

    /// <summary>
    /// Quando true, a reserva é criada como Pendente (sem check-in imediato).
    /// </summary>
    public bool ReservaFutura { get; set; } = false;
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

    // Cobrar traslado nesta saída (R$ configurável; grátis a partir de N diárias)
    public bool ComTraslado { get; set; } = false;
}

// === Response ===
public class ReservaResponseDto
{
    public int Id { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string TelefoneCliente { get; set; } = string.Empty;
    public string? CpfCliente { get; set; }
    public string? PlacaVeiculo { get; set; }
    public string? MarcaVeiculo { get; set; }
    public string? ModeloVeiculo { get; set; }
    public string? CorVeiculo { get; set; }
    public string TipoVaga { get; set; } = string.Empty;
    public DateTime DataEntrada { get; set; }
    public int QtdDias { get; set; }
    public DateTime DataSaidaPrevista { get; set; }
    public decimal ValorDiaria { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal DescontoAplicado { get; set; }
    public decimal ValorFinal { get; set; }
    public bool ComTraslado { get; set; }
    public decimal ValorTraslado { get; set; }
    public string? FormaPagamento { get; set; }
    public bool Pago { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public DateTime? DataCheckin { get; set; }
    public DateTime? DataCheckout { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataCriacao { get; set; }
    public bool ConfirmadaPeloCliente { get; set; }
    public bool MensagemConfirmacaoEnviada { get; set; }
    public DateTime? DataEnvioConfirmacao { get; set; }
}

public class FiltroReservaDto
{
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? Status { get; set; }
    public string? TipoVaga { get; set; }
    public string? PlacaVeiculo { get; set; }
}

// === FLUXO ONLINE EM LOTE (múltiplos veículos) ===
public class CarroLoteDto
{
    public string? PlacaVeiculo { get; set; }

    [Required]
    public string TipoVaga { get; set; } = "Coberta";

    [Required]
    public DateTime DataEntrada { get; set; }

    [Required]
    public DateTime DataSaidaPrevista { get; set; }

    [Required]
    [Range(0, 365)]
    public int QtdDias { get; set; } = 1;

    public string? Observacoes { get; set; }

    [MaxLength(50)]
    public string? MarcaVeiculo { get; set; }

    [MaxLength(50)]
    public string? ModeloVeiculo { get; set; }

    [MaxLength(30)]
    public string? CorVeiculo { get; set; }
}

public class CriarReservaLoteOnlineDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string NomeCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório")]
    public string TelefoneCliente { get; set; } = string.Empty;

    public string? CpfCliente { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Informe pelo menos um veículo")]
    public List<CarroLoteDto> Carros { get; set; } = new();
}

public class ReservaLoteResponseDto
{
    public List<ReservaResponseDto> Reservas { get; set; } = new();
    public int TotalReservas { get; set; }
    public decimal ValorTotalGeral { get; set; }
}

// === FLUXO PRESENCIAL EM LOTE (múltiplos veículos) ===
public class CarroPresencialLoteDto
{
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
    [Range(0, 365)]
    public int QtdDias { get; set; } = 1;

    public string? Observacoes { get; set; }

    [MaxLength(50)]
    public string? MarcaVeiculo { get; set; }

    [MaxLength(50)]
    public string? ModeloVeiculo { get; set; }

    [MaxLength(30)]
    public string? CorVeiculo { get; set; }
}

public class CriarReservaLotePresencialDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string NomeCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório")]
    public string TelefoneCliente { get; set; } = string.Empty;

    public string? CpfCliente { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Informe pelo menos um veículo")]
    public List<CarroPresencialLoteDto> Carros { get; set; } = new();
}

// === Editar marca/modelo/cor (qualquer perfil de admin) ===
public class AtualizarDadosVeiculoDto
{
    [MaxLength(50, ErrorMessage = "Marca deve ter no máximo 50 caracteres")]
    public string? MarcaVeiculo { get; set; }

    [MaxLength(50, ErrorMessage = "Modelo deve ter no máximo 50 caracteres")]
    public string? ModeloVeiculo { get; set; }

    [MaxLength(30, ErrorMessage = "Cor deve ter no máximo 30 caracteres")]
    public string? CorVeiculo { get; set; }
}

// === Prévia do check-out (não grava nada) ===
public class PreviaCheckoutDto
{
    public DateTime DataEntrada { get; set; }
    public DateTime DataSaida { get; set; }
    public string Permanencia { get; set; } = string.Empty;
    public string FormaPagamento { get; set; } = string.Empty;
    public int DiariasCompletas { get; set; }
    /// <summary>Diárias cheias cobradas (as de 24h + o parcial a partir de 12h). É a base do desconto Pix/Dinheiro.</summary>
    public int DiariasCobradas { get; set; }
    public decimal ValorEstadia { get; set; }
    public decimal ValorHorasAdicionais { get; set; }
    public decimal DescontoPixDinheiroPorDia { get; set; }
    public decimal Desconto { get; set; }
    public bool ComTraslado { get; set; }
    public bool TrasladoGratis { get; set; }
    public decimal ValorTraslado { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal ValorFinal { get; set; }
}

// === Corrigir a forma de pagamento depois do check-out ===
public class AlterarFormaPagamentoDto
{
    [Required(ErrorMessage = "A forma de pagamento é obrigatória")]
    public string FormaPagamento { get; set; } = string.Empty;

    /// <summary>Quando informado, também liga/desliga a cobrança de traslado. Nulo = mantém.</summary>
    public bool? ComTraslado { get; set; }
}

// === Reserva repetida (mesma placa e período) ===
public class ConflitoPlacaDto
{
    public int Id { get; set; }
    public string PlacaVeiculo { get; set; } = string.Empty;
    public DateTime DataEntrada { get; set; }
    public DateTime DataSaidaPrevista { get; set; }
    public string Status { get; set; } = string.Empty;
    public string TipoVaga { get; set; } = string.Empty;
    public bool PodeAtualizar { get; set; }
}

public class AlterarReservaAdminDto
{
    /// <summary>Opcional: nova entrada. Nulo = mantém a atual.</summary>
    public DateTime? DataEntrada { get; set; }

    [Required]
    public DateTime DataSaidaPrevista { get; set; }

    /// <summary>Opcional: novo tipo de vaga. Nulo = mantém o atual.</summary>
    public string? TipoVaga { get; set; }
}

public class AlterarReservaClienteDto
{
    public DateTime? DataEntrada { get; set; }

    [Required]
    public DateTime DataSaidaPrevista { get; set; }

    public string? TipoVaga { get; set; }

    [Required(ErrorMessage = "A placa é obrigatória")]
    [MaxLength(10)]
    public string PlacaVeiculo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório")]
    public string TelefoneCliente { get; set; } = string.Empty;
}
