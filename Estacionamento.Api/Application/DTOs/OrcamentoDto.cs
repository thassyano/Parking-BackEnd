using System.ComponentModel.DataAnnotations;

namespace Estacionamento.Api.Application.DTOs;

public class ConsultaOrcamentoDto
{
    [Required]
    public string TipoVaga { get; set; } = "Coberta";

    [Required]
    public DateTime DataReserva { get; set; }

    public int QtdDias { get; set; } = 1;
}

public class OrcamentoResponseDto
{
    public string TipoVaga { get; set; } = string.Empty;
    public DateTime DataReserva { get; set; }
    public int QtdDias { get; set; }
    public DateTime DataFim { get; set; }
    public decimal ValorDiaria { get; set; }
    public decimal ValorTotalCartao { get; set; }
    public decimal ValorTotalPix { get; set; }
    public decimal? DescontoPix { get; set; }
    public decimal EconomiaComPix { get; set; }
    public bool VagasDisponiveis { get; set; }
    public int VagasRestantes { get; set; }
}
