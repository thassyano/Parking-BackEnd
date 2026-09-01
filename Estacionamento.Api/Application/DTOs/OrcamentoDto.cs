using System.ComponentModel.DataAnnotations;

namespace Estacionamento.Api.Application.DTOs;

public class ConsultaOrcamentoDto
{
    [Required]
    public string TipoVaga { get; set; } = "Coberta";

    [Required]
    public DateTime DataEntrada { get; set; }

    [Range(0, 365)]
    public int QtdDias { get; set; } = 1;

    // Opcional: quando enviado, o valor considera horas (faixa do periodo parcial).
    // Sem ele, cai no comportamento antigo (QtdDias x diaria).
    public DateTime? DataSaidaPrevista { get; set; }
}

public class OrcamentoResponseDto
{
    public string TipoVaga { get; set; } = string.Empty;
    public DateTime DataEntrada { get; set; }
    public int QtdDias { get; set; }
    public DateTime DataSaidaPrevista { get; set; }
    public string TipoPrecificacao { get; set; } = "Diaria"; // HorasAte6h | HorasAte12h | Diaria
    public decimal ValorDiaria { get; set; }
    public int DiariasCompletas { get; set; }
    public decimal ValorHorasAdicionais { get; set; }
    public decimal ValorTotalCartao { get; set; }
    public decimal ValorTotalPixDinheiro { get; set; }
    public decimal DescontoPixDinheiroPorDia { get; set; }
    public decimal EconomiaTotal { get; set; }
    public bool VagasDisponiveis { get; set; }
    public int VagasRestantes { get; set; }
}
