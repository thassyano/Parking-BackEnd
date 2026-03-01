using System.ComponentModel.DataAnnotations;

namespace Estacionamento.Api.Application.DTOs;

public class CriarClienteDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório")]
    [MaxLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(14)]
    public string? Cpf { get; set; }

    [MaxLength(10)]
    public string? PlacaVeiculo { get; set; }

    [MaxLength(50)]
    public string? ModeloVeiculo { get; set; }

    [MaxLength(30)]
    public string? CorVeiculo { get; set; }
}

public class AtualizarClienteDto
{
    [MaxLength(100)]
    public string? Nome { get; set; }

    [MaxLength(20)]
    public string? Telefone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(14)]
    public string? Cpf { get; set; }

    [MaxLength(10)]
    public string? PlacaVeiculo { get; set; }

    [MaxLength(50)]
    public string? ModeloVeiculo { get; set; }

    [MaxLength(30)]
    public string? CorVeiculo { get; set; }
}

public class ClienteResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Cpf { get; set; }
    public string? PlacaVeiculo { get; set; }
    public string? ModeloVeiculo { get; set; }
    public string? CorVeiculo { get; set; }
    public DateTime DataCadastro { get; set; }
    public bool Ativo { get; set; }
    public int TotalReservas { get; set; }
}
