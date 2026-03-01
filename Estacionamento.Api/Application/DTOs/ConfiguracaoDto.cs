using System.ComponentModel.DataAnnotations;

namespace Estacionamento.Api.Application.DTOs;

public class AtualizarConfiguracaoDto
{
    [Range(0, 10000)]
    public int? TotalVagasCoberta { get; set; }

    [Range(0, 10000)]
    public int? TotalVagasDescoberta { get; set; }

    [MaxLength(20)]
    public string? TelefoneWhatsApp { get; set; }

    [MaxLength(1000)]
    public string? MensagemWhatsApp { get; set; }

    [Range(1, 168)]
    public int? HorasAntecedenciaConfirmacao { get; set; }
}

public class ConfiguracaoResponseDto
{
    public int Id { get; set; }
    public int TotalVagasCoberta { get; set; }
    public int TotalVagasDescoberta { get; set; }
    public string? TelefoneWhatsApp { get; set; }
    public string? MensagemWhatsApp { get; set; }
    public int HorasAntecedenciaConfirmacao { get; set; }
    public DateTime DataAtualizacao { get; set; }
}
