namespace Estacionamento.Api.Domain.Entities;

public class Admin
{
    public int Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; } = Helpers.DateTimeHelper.AgoraBrasilia();
    public bool Ativo { get; set; } = true;
}
