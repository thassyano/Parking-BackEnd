namespace Estacionamento.Api.Domain.Entities;

public class Vaga
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public bool Ocupada { get; set; }
    public DateTime? DataUltimaOcupacao { get; set; }
    public ICollection<Ocupacao> Ocupacoes { get; set; } = new List<Ocupacao>();
}

