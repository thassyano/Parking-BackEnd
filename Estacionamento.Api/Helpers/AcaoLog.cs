namespace Estacionamento.Api.Helpers;

public static class AcaoLog
{
    public const string LoginSucesso = "LoginSucesso";
    public const string LoginFalha = "LoginFalha";

    public const string AdminCriado = "AdminCriado";
    public const string AdminAtualizado = "AdminAtualizado";
    public const string AdminAtivado = "AdminAtivado";
    public const string AdminDesativado = "AdminDesativado";
    public const string AdminExcluido = "AdminExcluido";

    public const string ReservaOnline = "ReservaOnline";
    public const string ReservaPresencial = "ReservaPresencial";
    public const string ReservaAlterada = "ReservaAlterada";
    public const string ReservaPlacaAssociada = "ReservaPlacaAssociada";
    public const string ReservaCheckin = "ReservaCheckin";
    public const string ReservaCheckout = "ReservaCheckout";
    public const string ReservaCancelada = "ReservaCancelada";
    public const string ReservaConfirmadaCliente = "ReservaConfirmadaCliente";

    public const string ConfiguracaoAtualizada = "ConfiguracaoAtualizada";
    public const string ConfiguracaoTesteEvolution = "ConfiguracaoTesteEvolution";
    public const string PrecoCriado = "PrecoCriado";

    public const string WorkerWhatsAppEnviado = "WorkerWhatsAppEnviado";
    public const string WorkerWhatsAppFalha = "WorkerWhatsAppFalha";
    public const string WorkerCancelamentoAutomatico = "WorkerCancelamentoAutomatico";

    public static readonly string[] Todas =
    [
        LoginSucesso, LoginFalha,
        AdminCriado, AdminAtualizado, AdminAtivado, AdminDesativado, AdminExcluido,
        ReservaOnline, ReservaPresencial, ReservaAlterada, ReservaPlacaAssociada,
        ReservaCheckin, ReservaCheckout, ReservaCancelada, ReservaConfirmadaCliente,
        ConfiguracaoAtualizada, ConfiguracaoTesteEvolution, PrecoCriado,
        WorkerWhatsAppEnviado, WorkerWhatsAppFalha, WorkerCancelamentoAutomatico
    ];
}

public static class OrigemLog
{
    public const string Admin = "Admin";
    public const string Cliente = "Cliente";
    public const string Sistema = "Sistema";
    public const string Worker = "Worker";
}
