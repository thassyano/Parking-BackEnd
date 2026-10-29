using System.ComponentModel.DataAnnotations;

namespace Estacionamento.Api.Application.DTOs;

public class CriarBloqueioReservaDto
{
    [Required(ErrorMessage = "Informe a data/hora inicial")]
    public DateTime DataInicio { get; set; }

    [Required(ErrorMessage = "Informe a data/hora final")]
    public DateTime DataFim { get; set; }

    [MaxLength(200, ErrorMessage = "O motivo deve ter no máximo 200 caracteres")]
    public string? Motivo { get; set; }
}

public class BloqueioReservaResponseDto
{
    public int Id { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string? Motivo { get; set; }
    public bool Ativo { get; set; }
    public string? CriadoPor { get; set; }
    public DateTime DataCriacao { get; set; }
}

/// <summary>Resposta pública: o cliente só precisa saber se pode reservar e o aviso.</summary>
public class BloqueioStatusDto
{
    public bool Bloqueado { get; set; }
    public string? Mensagem { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
}
