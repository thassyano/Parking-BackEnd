namespace Estacionamento.Api.Helpers;

/// <summary>Resultado do calculo da estadia pelo tempo real de permanencia.</summary>
public record ResultadoEstadia(
    decimal ValorEstadia,
    decimal ValorHorasAdicionais,
    int DiariasCompletas,
    int DiariasCobradas,
    TimeSpan Permanencia);

/// <summary>
/// Calcula o valor da estadia pelo tempo REAL de permanencia.
/// Cada 24h completas = 1 diaria cheia. O periodo parcial final entra numa faixa:
/// ate 6h, acima de 6h ate 12h, ou acima de 12h (= diaria cheia).
/// </summary>
public static class CalculadoraEstadia
{
    public static ResultadoEstadia Calcular(
        DateTime entrada,
        DateTime saida,
        decimal valorAte6h,
        decimal valorAte12h,
        decimal valorDiaria)
    {
        var permanencia = saida - entrada;
        if (permanencia < TimeSpan.Zero)
            permanencia = TimeSpan.Zero;

        var totalHoras = (decimal)permanencia.TotalHours;
        var diariasCompletas = (int)Math.Floor(totalHoras / 24m);
        var horasRestantes = totalHoras - (diariasCompletas * 24m);

        decimal adicionais = 0m;
        int diariasCobradas = diariasCompletas;

        if (horasRestantes > 0m)
        {
            if (horasRestantes <= 6m)
                adicionais = valorAte6h;
            else if (horasRestantes <= 12m)
                adicionais = valorAte12h;
            else
                adicionais = valorDiaria; // acima de 12h = diaria cheia

            diariasCobradas += 1; // periodo parcial conta como 1 diaria (regra do traslado)
        }

        if (diariasCobradas == 0)
            diariasCobradas = 1; // permanencia zero -> minimo de 1

        var valorEstadia = (diariasCompletas * valorDiaria) + adicionais;

        return new ResultadoEstadia(valorEstadia, adicionais, diariasCompletas, diariasCobradas, permanencia);
    }

    /// <summary>Formata a permanencia como "X dias e HH:MM:SS" (ou apenas HH:MM:SS quando &lt; 1 dia).</summary>
    public static string FormatarPermanencia(TimeSpan p)
    {
        if (p < TimeSpan.Zero) p = TimeSpan.Zero;
        var resto = $"{p.Hours:D2}:{p.Minutes:D2}:{p.Seconds:D2}";
        return p.Days > 0 ? $"{p.Days} dia{(p.Days > 1 ? "s" : "")} e {resto}" : resto;
    }
}
