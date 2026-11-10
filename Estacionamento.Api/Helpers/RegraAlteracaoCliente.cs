namespace Estacionamento.Api.Helpers;

/// <summary>
/// Regra de quando o cliente pode alterar a própria reserva pelo site.
/// O robô de confirmação dispara a mensagem quando faltam <c>HorasAntecedenciaConfirmacao</c> horas para a entrada
/// (padrão 24h); o cliente só altera sozinho até 1 hora ANTES disso (padrão 25h), para não mexer na reserva
/// quando a mensagem de confirmação já saiu ou está para sair.
/// </summary>
public static class RegraAlteracaoCliente
{
    public const int HorasAntecedenciaPadrao = 24;
    public const int MargemHoras = 1;

    /// <summary>Horas mínimas de antecedência da entrada para o cliente alterar (ex.: 24 + 1 = 25).</summary>
    public static int HorasLimite(int horasAntecedenciaConfirmacao) =>
        (horasAntecedenciaConfirmacao > 0 ? horasAntecedenciaConfirmacao : HorasAntecedenciaPadrao) + MargemHoras;

    /// <summary>Último instante em que o cliente ainda pode alterar.</summary>
    public static DateTime Limite(DateTime dataEntrada, int horasLimite) => dataEntrada.AddHours(-horasLimite);

    /// <summary>Pode alterar quando ainda não passou do limite e a mensagem de confirmação ainda não foi enviada.</summary>
    public static bool PodeAlterar(DateTime agora, DateTime dataEntrada, int horasLimite, bool mensagemConfirmacaoEnviada) =>
        !mensagemConfirmacaoEnviada && agora <= Limite(dataEntrada, horasLimite);

    public static string MensagemBloqueio(int horasLimite) =>
        $"Alterações pelo site só são possíveis até {horasLimite} horas antes da entrada. " +
        "Para ajustar esta reserva, fale com o estacionamento.";
}
