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
    public int ReservasConfirmadas { get; set; }
    public int ReservasCanceladas { get; set; }
    public int CheckinsRealizados { get; set; }
    public int CheckoutsRealizados { get; set; }

    public decimal ReceitaTotal { get; set; }
    public decimal ReceitaPix { get; set; }
    public decimal ReceitaCartaoCredito { get; set; }
    public decimal ReceitaCartaoDebito { get; set; }
    public decimal ReceitaDinheiro { get; set; }

    public int VagasCobertas { get; set; }
    public int VagasDescobertas { get; set; }

    public List<ReservaResumoCaixaDto> Reservas { get; set; } = new();
}

public class ReservaResumoCaixaDto
{
    public int ReservaId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteTelefone { get; set; } = string.Empty;
    public string? PlacaVeiculo { get; set; }
    public string TipoVaga { get; set; } = string.Empty;
    public DateTime DataReserva { get; set; }
    public int QtdDias { get; set; }
    public decimal ValorFinal { get; set; }
    public string? FormaPagamento { get; set; }
    public string Status { get; set; } = string.Empty;
}
