namespace Estacionamento.Api.Application.DTOs;

public class WhatsAppRedirectDto
{
    public string Url { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string TelefoneEstacionamento { get; set; } = string.Empty;
}
