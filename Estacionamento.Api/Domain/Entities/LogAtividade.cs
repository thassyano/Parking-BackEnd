namespace Estacionamento.Api.Domain.Entities;

public class LogAtividade
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; } = Helpers.DateTimeHelper.AgoraBrasilia();
    public int? AdminId { get; set; }
    public string? AdminUsuario { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string? Entidade { get; set; }
    public int? EntidadeId { get; set; }
    public string Detalhes { get; set; } = string.Empty;
    public bool Sucesso { get; set; } = true;
    public string Origem { get; set; } = "Admin";
}
