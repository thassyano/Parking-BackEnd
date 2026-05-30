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

    // Horas adicionais de permanência (cobradas na saída, sem desconto)
    public decimal ValorHorasAdicionaisAte6h { get; set; }  // até 6h além da saída prevista
    public decimal ValorHorasAdicionaisAte12h { get; set; } // de 6h+1min até 12h além da saída prevista
    // Acima de 12h extras: cobra ValorDiaria cheia

    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public bool Ativo { get; set; } = true;
}
