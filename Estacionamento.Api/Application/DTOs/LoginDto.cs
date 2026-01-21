using System.ComponentModel.DataAnnotations;

namespace Estacionamento.Api.Application.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "O usuário é obrigatório")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória")]
    public string Senha { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
}

