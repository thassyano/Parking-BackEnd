namespace Estacionamento.Api.Application.DTOs;

public class CupomEntradaDto
{
    public string NomeEstacionamento { get; set; } = string.Empty;
    public string? Endereco { get; set; }
    public string? Contato { get; set; }
    public string? Cnpj { get; set; }

    public int Numero { get; set; }
    public string PlacaVeiculo { get; set; } = string.Empty;
    public DateTime DataHoraEntrada { get; set; }
    public string TipoVaga { get; set; } = string.Empty;
    public int QtdDias { get; set; }
    public DateTime DataSaidaPrevista { get; set; }
    public decimal ValorDiaria { get; set; }
    public decimal ValorTotal { get; set; }
    public string Mensagem { get; set; } = "Para retirada do seu veículo é obrigatória a apresentação deste cupom ou documentos do veículo e condutor.";
}

public class CupomSaidaDto
{
    public string NomeEstacionamento { get; set; } = string.Empty;
    public string? Endereco { get; set; }
    public string? Contato { get; set; }
    public string? Cnpj { get; set; }

    public int Numero { get; set; }
    public string PlacaVeiculo { get; set; } = string.Empty;
    public DateTime DataHoraEntrada { get; set; }
    public DateTime DataSaidaPrevista { get; set; }
    public DateTime DataHoraSaida { get; set; }
    public string TipoVaga { get; set; } = string.Empty;

    // Cobranca pelo tempo real de permanencia
    public int QtdDias { get; set; }                          // diarias efetivamente cobradas
    public string Permanencia { get; set; } = string.Empty;   // ex.: "4 dias e 20:49:56"

    public decimal ValorDiaria { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal DescontoAplicado { get; set; }
    public decimal ValorHorasAdicionais { get; set; }         // valor do periodo parcial (faixa)

    // Traslado
    public bool ComTraslado { get; set; }
    public decimal ValorTraslado { get; set; }

    public decimal ValorFinal { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;
}
