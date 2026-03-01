namespace Estacionamento.Api.Domain.Entities;

public enum TipoVaga
{
    Coberta,
    Descoberta
}

public class Preco
{
    public int Id { get; set; }
    public TipoVaga TipoVaga { get; set; }
    public decimal ValorDiaria { get; set; }
    public decimal? DescontoPix { get; set; } // percentual de desconto para Pix (ex: 5.0 = 5%)
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public bool Ativo { get; set; } = true;
}
