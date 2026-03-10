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
    public decimal DescontoPixDinheiro { get; set; } // R$ de desconto por diária para Pix ou Dinheiro
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public bool Ativo { get; set; } = true;
}
