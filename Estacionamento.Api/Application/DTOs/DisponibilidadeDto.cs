namespace Estacionamento.Api.Application.DTOs;

public class DisponibilidadeDto
{
    public int TotalVagas { get; set; }
    public int VagasOcupadas { get; set; }
    public int VagasDisponiveis { get; set; }
    public List<VagaDisponibilidadeDto> Vagas { get; set; } = new();
}

public class VagaDisponibilidadeDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public bool Ocupada { get; set; }
    public string? PlacaVeiculo { get; set; }
    public DateTime? DataEntrada { get; set; }
}

