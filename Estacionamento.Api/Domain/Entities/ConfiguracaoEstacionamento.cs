namespace Estacionamento.Api.Domain.Entities;

public class ConfiguracaoEstacionamento
{
    public int Id { get; set; }

    // Dados do estabelecimento (para cupom)
    public string NomeEstacionamento { get; set; } = string.Empty;
    public string? Endereco { get; set; }
    public string? Contato { get; set; }
    public string? Cnpj { get; set; }

    // Vagas
    public int TotalVagasCoberta { get; set; }
    public int TotalVagasDescoberta { get; set; }

    // Traslado
    public decimal ValorTraslado { get; set; } = 30m;              // valor cobrado pelo traslado
    public int TrasladoGratisAPartirDiarias { get; set; } = 2;     // gratis a partir de N diarias

    // WhatsApp
    public string? TelefoneWhatsApp { get; set; }
    public string? MensagemWhatsApp { get; set; }

    // Evolution API (envio ativo de mensagens)
    public string? EvolutionApiUrl { get; set; }
    public string? EvolutionApiKey { get; set; }
    public string? EvolutionInstanceName { get; set; }

    // URL base do front-end para link de confirmacao
    public string? UrlConfirmacaoFrontend { get; set; }

    public int HorasAntecedenciaConfirmacao { get; set; } = 24;
    public DateTime DataAtualizacao { get; set; } = Helpers.DateTimeHelper.AgoraBrasilia();
}
