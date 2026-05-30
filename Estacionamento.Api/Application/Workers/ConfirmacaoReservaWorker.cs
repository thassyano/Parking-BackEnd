using Estacionamento.Api.Application.Services;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Helpers;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Application.Workers;

public class ConfirmacaoReservaWorker : BackgroundService
{
    private const string MotivosCancelamento = "Cancelada automaticamente por falta de confirmação via WhatsApp.";
    private static readonly TimeSpan IntervaloProducao = TimeSpan.FromHours(2);
    private static readonly TimeSpan IntervaloDesenvolvimento = TimeSpan.FromMinutes(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ConfirmacaoReservaWorker> _logger;

    public ConfirmacaoReservaWorker(
        IServiceScopeFactory scopeFactory,
        IHostEnvironment environment,
        ILogger<ConfirmacaoReservaWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _environment = environment;
        _logger = logger;
    }

    private TimeSpan IntervaloExecucao =>
        _environment.IsDevelopment() ? IntervaloDesenvolvimento : IntervaloProducao;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "ConfirmacaoReservaWorker iniciado. Intervalo: {Intervalo} ({Ambiente}).",
            IntervaloExecucao,
            _environment.EnvironmentName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarCicloAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no ConfirmacaoReservaWorker.");
            }

            await Task.Delay(IntervaloExecucao, stoppingToken);
        }
    }

    private async Task ProcessarCicloAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var services = ResolverServicos(scope);

        var config = await services.ConfiguracaoRepo.ObterAsync();
        if (config is null)
        {
            _logger.LogWarning("Configuração não encontrada. Ciclo ignorado.");
            return;
        }

        await CancelarNaoConfirmadasAsync(services.ReservaRepo, services.LogAtividadeService);
        await EnviarNotificacoesAsync(services.ReservaRepo, services.WhatsAppService, services.LogAtividadeService, config);
    }

    private async Task EnviarNotificacoesAsync(
        IReservaRepository reservaRepo,
        IWhatsAppService whatsAppService,
        ILogAtividadeService logAtividadeService,
        ConfiguracaoEstacionamento config)
    {
        var horasAntecedencia = config.HorasAntecedenciaConfirmacao > 0
            ? config.HorasAntecedenciaConfirmacao
            : 24;

        var reservas = (await reservaRepo.ObterParaEnvioConfirmacaoAsync(horasAntecedencia)).ToList();
        if (reservas.Count == 0) return;

        _logger.LogInformation("{count} reserva(s) para notificar.", reservas.Count);

        foreach (var reserva in reservas)
            await NotificarReservaAsync(reserva, reservaRepo, whatsAppService, logAtividadeService, config);
    }

    private async Task NotificarReservaAsync(
        Reserva reserva,
        IReservaRepository reservaRepo,
        IWhatsAppService whatsAppService,
        ILogAtividadeService logAtividadeService,
        ConfiguracaoEstacionamento config)
    {
        try
        {
            var enviado = await whatsAppService.EnviarConfirmacaoViaEvolutionAsync(reserva, config);

            if (enviado)
            {
                reserva.MensagemConfirmacaoEnviada = true;
                reserva.DataEnvioConfirmacao = DateTimeHelper.AgoraBrasilia();
                await reservaRepo.AtualizarAsync(reserva);
                _logger.LogInformation("Mensagem enviada: reserva {Id} ({Nome}).", reserva.Id, reserva.NomeCliente);
                await logAtividadeService.RegistrarAsync(LogAtividadeHttpExtensions.CriarRegistroSistema(
                    AcaoLog.WorkerWhatsAppEnviado,
                    $"WhatsApp confirmação enviado — reserva #{reserva.Id} ({reserva.NomeCliente})",
                    "Reserva",
                    reserva.Id));
            }
            else
            {
                _logger.LogWarning(
                    "Mensagem NÃO enviada para reserva {Id}. Verifique Evolution (container, instância conectada e configuração).",
                    reserva.Id);
                await logAtividadeService.RegistrarAsync(LogAtividadeHttpExtensions.CriarRegistroSistema(
                    AcaoLog.WorkerWhatsAppFalha,
                    $"Falha ao enviar WhatsApp — reserva #{reserva.Id}",
                    "Reserva",
                    reserva.Id,
                    sucesso: false));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao notificar reserva {Id}.", reserva.Id);
        }
    }

    private async Task CancelarNaoConfirmadasAsync(IReservaRepository reservaRepo, ILogAtividadeService logAtividadeService)
    {
        var reservas = (await reservaRepo.ObterParaCancelamentoAutomaticoAsync()).ToList();
        if (reservas.Count == 0) return;

        _logger.LogInformation("{count} reserva(s) para cancelamento automático.", reservas.Count);

        foreach (var reserva in reservas)
            await CancelarReservaAsync(reserva, reservaRepo, logAtividadeService);
    }

    private async Task CancelarReservaAsync(Reserva reserva, IReservaRepository reservaRepo, ILogAtividadeService logAtividadeService)
    {
        try
        {
            reserva.Status = StatusReserva.Cancelada;
            reserva.Observacoes = string.IsNullOrEmpty(reserva.Observacoes)
                ? MotivosCancelamento
                : $"{reserva.Observacoes} | {MotivosCancelamento}";

            await reservaRepo.AtualizarAsync(reserva);
            _logger.LogInformation("Reserva {Id} cancelada automaticamente.", reserva.Id);
            await logAtividadeService.RegistrarAsync(LogAtividadeHttpExtensions.CriarRegistroSistema(
                AcaoLog.WorkerCancelamentoAutomatico,
                $"Reserva #{reserva.Id} cancelada automaticamente (sem confirmação WhatsApp)",
                "Reserva",
                reserva.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao cancelar reserva {Id}.", reserva.Id);
        }
    }

    private static (IReservaRepository ReservaRepo, IConfiguracaoRepository ConfiguracaoRepo, IWhatsAppService WhatsAppService, ILogAtividadeService LogAtividadeService)
        ResolverServicos(IServiceScope scope) =>
    (
        scope.ServiceProvider.GetRequiredService<IReservaRepository>(),
        scope.ServiceProvider.GetRequiredService<IConfiguracaoRepository>(),
        scope.ServiceProvider.GetRequiredService<IWhatsAppService>(),
        scope.ServiceProvider.GetRequiredService<ILogAtividadeService>()
    );
}
