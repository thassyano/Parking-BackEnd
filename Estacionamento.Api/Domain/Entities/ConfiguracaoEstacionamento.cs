namespace Estacionamento.Api.Domain.Entities;

public class ConfiguracaoEstacionamento
{
    public int Id { get; set; }
    public int TotalVagasCoberta { get; set; }
    public int TotalVagasDescoberta { get; set; }
    public string? TelefoneWhatsApp { get; set; }
    public string? MensagemWhatsApp { get; set; } // template da mensagem pós-reserva
    public int HorasAntecedenciaConfirmacao { get; set; } = 24;
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
}
