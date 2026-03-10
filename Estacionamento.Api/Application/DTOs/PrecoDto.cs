using System.ComponentModel.DataAnnotations;

namespace Estacionamento.Api.Application.DTOs;

public class CriarPrecoDto
{
    [Required]
    public string TipoVaga { get; set; } = "Coberta"; // "Coberta" ou "Descoberta"

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor da diária deve ser maior que zero")]
    public decimal ValorDiaria { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DescontoPixDinheiro { get; set; } // R$ de desconto por diária

    public DateTime? DataInicio { get; set; }
}

public class PrecoResponseDto
{
    public int Id { get; set; }
    public string TipoVaga { get; set; } = string.Empty;
    public decimal ValorDiaria { get; set; }
    public decimal DescontoPixDinheiro { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public bool Ativo { get; set; }
}
