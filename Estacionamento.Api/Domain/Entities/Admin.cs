namespace Estacionamento.Api.Domain.Entities;

public class Admin
{
    public int Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public bool Ativo { get; set; } = true;
}

