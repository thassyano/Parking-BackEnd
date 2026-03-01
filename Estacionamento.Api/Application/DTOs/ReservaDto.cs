using System.ComponentModel.DataAnnotations;
using Estacionamento.Api.Domain.Entities;

namespace Estacionamento.Api.Application.DTOs;

public class CriarReservaDto
{
    [Required]
    public int ClienteId { get; set; }

    [Required]
    public string TipoVaga { get; set; } = "Coberta"; // "Coberta" ou "Descoberta"

    [Required]
    public DateTime DataReserva { get; set; }

    public int QtdDias { get; set; } = 1;

    public string? FormaPagamento { get; set; } // "Pix", "CartaoCredito", "CartaoDebito", "Dinheiro"

    public string Origem { get; set; } = "Online"; // "Online" ou "Presencial"

    public string? Observacoes { get; set; }
}

public class CriarReservaPresencialDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string NomeCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório")]
    public string TelefoneCliente { get; set; } = string.Empty;

    public string? EmailCliente { get; set; }
    public string? PlacaVeiculo { get; set; }
    public string? ModeloVeiculo { get; set; }
    public string? CorVeiculo { get; set; }

    [Required]
    public string TipoVaga { get; set; } = "Coberta";

    [Required]
    public DateTime DataReserva { get; set; }

    public int QtdDias { get; set; } = 1;

    public string? FormaPagamento { get; set; }
    public string? Observacoes { get; set; }
}

public class ReservaResponseDto
{
    public int Id { get; set; }
    public ClienteResumoDto Cliente { get; set; } = null!;
    public string TipoVaga { get; set; } = string.Empty;
    public DateTime DataReserva { get; set; }
    public int QtdDias { get; set; }
    public DateTime DataFim { get; set; }
    public decimal ValorDiaria { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal? DescontoAplicado { get; set; }
    public decimal ValorFinal { get; set; }
    public string? FormaPagamento { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public DateTime? DataCheckin { get; set; }
    public DateTime? DataCheckout { get; set; }
    public bool ConfirmacaoEnviada { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class ClienteResumoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? PlacaVeiculo { get; set; }
}

public class FiltroReservaDto
{
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? Status { get; set; }
    public string? TipoVaga { get; set; }
    public int? ClienteId { get; set; }
}
