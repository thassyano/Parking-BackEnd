using System.ComponentModel.DataAnnotations;

namespace Estacionamento.Api.Application.DTOs;

public class CriarOcupacaoDto
{
    [Required(ErrorMessage = "O ID da vaga é obrigatório")]
    public int VagaId { get; set; }

    [Required(ErrorMessage = "A placa do veículo é obrigatória")]
    [StringLength(10, MinimumLength = 7, ErrorMessage = "A placa deve ter entre 7 e 10 caracteres")]
    public string PlacaVeiculo { get; set; } = string.Empty;
}

