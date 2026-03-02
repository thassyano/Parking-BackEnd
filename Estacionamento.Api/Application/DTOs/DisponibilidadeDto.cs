namespace Estacionamento.Api.Application.DTOs;

public class DisponibilidadeResponseDto
{
    public DateTime Data { get; set; }
    public int VagasCobertaTotal { get; set; }
    public int VagasCobertaOcupadas { get; set; }
    public int VagasCobertaDisponiveis { get; set; }
    public int VagasDescobertaTotal { get; set; }
    public int VagasDescobertaOcupadas { get; set; }
    public int VagasDescobertaDisponiveis { get; set; }
}

public class DisponibilidadePeriodoDto
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public List<DisponibilidadeResponseDto> Dias { get; set; } = new();
}
