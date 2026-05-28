using System.ComponentModel.DataAnnotations;
using Estacionamento.Api.Domain.Entities;

namespace Estacionamento.Api.Application.DTOs;

public class CriarAdminDto
{
    [Required(ErrorMessage = "O usuário é obrigatório")]
    [MaxLength(50)]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Nome { get; set; }
}

public class AtualizarAdminDto
{
    [Required(ErrorMessage = "O usuário é obrigatório")]
    [MaxLength(50)]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Nome { get; set; }

    [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
    public string? Senha { get; set; }
}

public class AdminResponseDto
{
    public int Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public PerfilAdmin Perfil { get; set; }
    public DateTime DataCriacao { get; set; }
    public bool Ativo { get; set; }
}
