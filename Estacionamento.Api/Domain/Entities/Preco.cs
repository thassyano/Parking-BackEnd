namespace Estacionamento.Api.Domain.Entities;

public class Preco
{
    public int Id { get; set; }
    public decimal ValorHora { get; set; }
    public decimal ValorMinuto { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public bool Ativo { get; set; } = true;
}

