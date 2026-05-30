namespace Estacionamento.Api.Helpers;

public static class DateTimeHelper
{
    private static readonly TimeZoneInfo BrasiliaTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows()
            ? "E. South America Standard Time"
            : "America/Sao_Paulo");

    public static DateTime AgoraBrasilia() =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BrasiliaTimeZone);

    public static DateTime ParaBrasilia(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc
            ? TimeZoneInfo.ConvertTimeFromUtc(dt, BrasiliaTimeZone)
            : dt;

    public static void ValidarPeriodoReserva(DateTime dataInicio, DateTime dataFim)
    {
        var hoje = AgoraBrasilia().Date;

        if (dataInicio.Date < hoje)
            throw new InvalidOperationException("A data de entrada não pode ser anterior a hoje");

        if (dataFim.Date < dataInicio.Date)
            throw new InvalidOperationException("A data de saída deve ser maior ou igual à data de entrada");
    }
}
