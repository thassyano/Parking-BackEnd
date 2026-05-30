using Estacionamento.Api.Domain.Entities;

namespace Estacionamento.Api.Helpers;

public enum TipoPrecificacao
{
    HorasAte6h,
    HorasAte12h,
    Diaria
}

/// <summary>
/// Calcula o valor de uma permanência com base nas horas reais entre entrada e saída.
///
/// Regras:
///   ≤ 6h  → ValorHorasAdicionaisAte6h
///   ≤ 12h → ValorHorasAdicionaisAte12h
///   > 12h → blocos de 24h (diárias) + eventual fração:
///               fração ≤ 6h  → + ValorHorasAdicionaisAte6h
///               fração ≤ 12h → + ValorHorasAdicionaisAte12h
///               fração > 12h → + ValorDiaria (diária cheia)
/// </summary>
public static class PrecificacaoHelper
{
    public static (decimal valorTotal, int qtdDias, TipoPrecificacao tipo) Calcular(Preco preco, DateTime dataEntrada, DateTime dataSaida)
    {
        var totalHoras = (dataSaida - dataEntrada).TotalHours;

        if (totalHoras <= 0)
            throw new InvalidOperationException("A data de saída deve ser posterior à data de entrada");

        // Permanência de até 6h
        if (totalHoras <= 6)
            return (preco.ValorHorasAdicionaisAte6h, 0, TipoPrecificacao.HorasAte6h);

        // Permanência de até 12h
        if (totalHoras <= 12)
            return (preco.ValorHorasAdicionaisAte12h, 0, TipoPrecificacao.HorasAte12h);

        // Permanência por diárias (> 12h)
        var diasCheios = (int)(totalHoras / 24);
        var horasRestantes = totalHoras - diasCheios * 24;

        decimal valorFracao = 0;
        if (horasRestantes > 12)
        {
            diasCheios++;  // fração > 12h vira diária cheia
        }
        else if (horasRestantes > 6)
        {
            valorFracao = preco.ValorHorasAdicionaisAte12h;
        }
        else if (horasRestantes > 0)
        {
            valorFracao = preco.ValorHorasAdicionaisAte6h;
        }

        var qtdDias = Math.Max(1, diasCheios);
        var valorTotal = preco.ValorDiaria * diasCheios + valorFracao;

        return (valorTotal, qtdDias, TipoPrecificacao.Diaria);
    }
}
