using System.ComponentModel.DataAnnotations;

namespace Estacionamento.Api.Application.DTOs;

public class AtualizarConfiguracaoDto
{
    [MaxLength(200)]
    public string? NomeEstacionamento { get; set; }

    [MaxLength(300)]
    public string? Endereco { get; set; }

    [MaxLength(20)]
    public string? Contato { get; set; }

    [MaxLength(20)]
    public string? Cnpj { get; set; }

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
    public string NomeEstacionamento { get; set; } = string.Empty;
    public string? Endereco { get; set; }
    public string? Contato { get; set; }
    public string? Cnpj { get; set; }
    public int TotalVagasCoberta { get; set; }
    public int TotalVagasDescoberta { get; set; }
    public string? TelefoneWhatsApp { get; set; }
    public string? MensagemWhatsApp { get; set; }
    public int HorasAntecedenciaConfirmacao { get; set; }
    public DateTime DataAtualizacao { get; set; }
}
