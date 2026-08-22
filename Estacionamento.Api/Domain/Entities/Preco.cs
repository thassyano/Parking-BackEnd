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

    // Faixas por periodo (periodo parcial, alem das diarias completas)
    public decimal ValorHorasAdicionaisAte6h { get; set; }   // periodo parcial de ate 6h
    public decimal ValorHorasAdicionaisAte12h { get; set; }  // periodo parcial acima de 6h ate 12h
    public decimal ValorDiaria { get; set; }                 // acima de 12h = diaria cheia

    public decimal DescontoPixDinheiro { get; set; } // R$ de desconto por diaria para Pix ou Dinheiro
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public bool Ativo { get; set; } = true;
}
