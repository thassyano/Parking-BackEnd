using System.ComponentModel.DataAnnotations;

namespace Estacionamento.Api.Application.DTOs;

public class ConsultaOrcamentoDto
{
    [Required]
    public string TipoVaga { get; set; } = "Coberta";

    [Required]
    public DateTime DataEntrada { get; set; }

    [Range(1, 365)]
    public int QtdDias { get; set; } = 1;
}

public class OrcamentoResponseDto
{
    public string TipoVaga { get; set; } = string.Empty;
    public DateTime DataEntrada { get; set; }
    public int QtdDias { get; set; }
    public DateTime DataSaidaPrevista { get; set; }
    public decimal ValorDiaria { get; set; }
    public decimal ValorTotalCartao { get; set; }
    public decimal ValorTotalPixDinheiro { get; set; }
    public decimal DescontoPixDinheiroPorDia { get; set; }
    public decimal EconomiaTotal { get; set; }
    public bool VagasDisponiveis { get; set; }
    public int VagasRestantes { get; set; }
}
