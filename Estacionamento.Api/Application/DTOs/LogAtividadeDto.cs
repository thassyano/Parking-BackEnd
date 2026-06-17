namespace Estacionamento.Api.Application.DTOs;

public class RegistrarLogAtividadeDto
{
    public int? AdminId { get; set; }
    public string? AdminUsuario { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string? Entidade { get; set; }
    public int? EntidadeId { get; set; }
    public string Detalhes { get; set; } = string.Empty;
    public bool Sucesso { get; set; } = true;
    public string Origem { get; set; } = Helpers.OrigemLog.Admin;
}

public class FiltroLogAtividadeDto
{
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? Acao { get; set; }
    public string? AdminUsuario { get; set; }
    public string? Origem { get; set; }
    public bool? Sucesso { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 50;
}

public class LogAtividadeResponseDto
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; }
    public int? AdminId { get; set; }
    public string? AdminUsuario { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string? Entidade { get; set; }
    public int? EntidadeId { get; set; }
    public string Detalhes { get; set; } = string.Empty;
    public bool Sucesso { get; set; }
    public string Origem { get; set; } = string.Empty;
}

public class LogAtividadePaginadoDto
{
    public List<LogAtividadeResponseDto> Itens { get; set; } = [];
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalPaginas { get; set; }
}
