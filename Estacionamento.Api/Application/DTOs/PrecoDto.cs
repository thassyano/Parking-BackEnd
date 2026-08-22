using System.ComponentModel.DataAnnotations;

namespace Estacionamento.Api.Application.DTOs;

public class CriarPrecoDto
{
    [Required]
    public string TipoVaga { get; set; } = "Coberta"; // "Coberta" ou "Descoberta"

    [Range(0, double.MaxValue, ErrorMessage = "O valor da faixa ate 6h nao pode ser negativo")]
    public decimal ValorHorasAdicionaisAte6h { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "O valor da faixa ate 12h nao pode ser negativo")]
    public decimal ValorHorasAdicionaisAte12h { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor da diaria deve ser maior que zero")]
    public decimal ValorDiaria { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DescontoPixDinheiro { get; set; } // R$ de desconto por diaria

    [Required(ErrorMessage = "A data de inicio e obrigatoria")]
    public DateTime DataInicio { get; set; }

    public DateTime? DataFim { get; set; }
}

public class PrecoResponseDto
{
    public int Id { get; set; }
    public string TipoVaga { get; set; } = string.Empty;
    public decimal ValorHorasAdicionaisAte6h { get; set; }
    public decimal ValorHorasAdicionaisAte12h { get; set; }
    public decimal ValorDiaria { get; set; }
    public decimal DescontoPixDinheiro { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public bool Ativo { get; set; }
}
