using System.ComponentModel.DataAnnotations;

namespace Estacionamento.Api.Application.DTOs;

public class FiltroFechamentoCaixaDto
{
    [Required]
    public DateTime DataInicio { get; set; }

    [Required]
    public DateTime DataFim { get; set; }
}

public class FechamentoCaixaResponseDto
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }

    public int TotalReservas { get; set; }
    public int ReservasPagas { get; set; }
    public int ReservasCanceladas { get; set; }
    public int ReservasPendentes { get; set; }

    public decimal ReceitaTotal { get; set; }
    public decimal ReceitaPix { get; set; }
    public decimal ReceitaCartao { get; set; }
    public decimal ReceitaDinheiro { get; set; }

    public int VagasCobertas { get; set; }
    public int VagasDescobertas { get; set; }

    public List<ReservaResumoCaixaDto> Reservas { get; set; } = new();
}

public class ReservaResumoCaixaDto
{
    public int ReservaId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string TelefoneCliente { get; set; } = string.Empty;
    public string? PlacaVeiculo { get; set; }
    public string TipoVaga { get; set; } = string.Empty;
    public DateTime DataEntrada { get; set; }
    public int QtdDias { get; set; }
    public decimal ValorFinal { get; set; }
    public string? FormaPagamento { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool Pago { get; set; }
}
