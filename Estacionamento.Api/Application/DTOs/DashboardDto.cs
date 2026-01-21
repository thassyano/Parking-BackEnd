namespace Estacionamento.Api.Application.DTOs;

public class DashboardDto
{
    public int TotalVagas { get; set; }
    public int VagasOcupadas { get; set; }
    public int VagasDisponiveis { get; set; }
    public decimal ReceitaHoje { get; set; }
    public decimal ReceitaMes { get; set; }
    public int OcupacoesHoje { get; set; }
    public List<OcupacaoResumoDto> OcupacoesAtivas { get; set; } = new();
}

public class OcupacaoResumoDto
{
    public int Id { get; set; }
    public string NumeroVaga { get; set; } = string.Empty;
    public string PlacaVeiculo { get; set; } = string.Empty;
    public DateTime DataEntrada { get; set; }
    public TimeSpan TempoEstacionado { get; set; }
}

