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
}
