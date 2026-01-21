namespace Estacionamento.Api.Domain.Entities;

public class Ocupacao
{
    public int Id { get; set; }
    public int VagaId { get; set; }
    public Vaga Vaga { get; set; } = null!;
    public string PlacaVeiculo { get; set; } = string.Empty;
    public DateTime DataEntrada { get; set; }
    public DateTime? DataSaida { get; set; }
    public decimal? ValorPago { get; set; }
    public bool Ativa { get; set; } = true;
}

