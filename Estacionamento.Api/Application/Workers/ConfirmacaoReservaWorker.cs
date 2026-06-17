using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Application.Services;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Helpers;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Application.Workers;

public class ConfirmacaoReservaWorker : BackgroundService
{
    private const string MotivosCancelamento = "Cancelada automaticamente por falta de confirmação via WhatsApp.";
    private static readonly TimeSpan IntervaloExecucao = TimeSpan.FromHours(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ConfirmacaoReservaWorker> _logger;

    public ConfirmacaoReservaWorker(IServiceScopeFactory scopeFactory, ILogger<ConfirmacaoReservaWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ConfirmacaoReservaWorker iniciado. Intervalo: {h}h.", IntervaloExecucao.TotalHours);

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

        await CancelarNaoConfirmadasAsync(services.ReservaRepo, services.LogService);
        await EnviarNotificacoesAsync(services.ReservaRepo, services.WhatsAppService, services.LogService, config);
    }

    private async Task EnviarNotificacoesAsync(
        IReservaRepository reservaRepo,
        IWhatsAppService whatsAppService,
        ILogAtividadeService logService,
        ConfiguracaoEstacionamento config)
    {
        var horasAntecedencia = config.HorasAntecedenciaConfirmacao > 0
            ? config.HorasAntecedenciaConfirmacao
            : 24;

        var reservas = (await reservaRepo.ObterParaEnvioConfirmacaoAsync(horasAntecedencia)).ToList();
        if (reservas.Count == 0) return;

        _logger.LogInformation("{count} reserva(s) para notificar.", reservas.Count);

        foreach (var reserva in reservas)
            await NotificarReservaAsync(reserva, reservaRepo, whatsAppService, logService, config);
    }

    private async Task NotificarReservaAsync(
        Reserva reserva,
        IReservaRepository reservaRepo,
        IWhatsAppService whatsAppService,
        ILogAtividadeService logService,
        ConfiguracaoEstacionamento config)
    {
        try
        {
            var enviado = await whatsAppService.EnviarConfirmacaoViaEvolutionAsync(reserva, config);

            reserva.MensagemConfirmacaoEnviada = true;
            reserva.DataEnvioConfirmacao = DateTimeHelper.AgoraBrasilia();
            await reservaRepo.AtualizarAsync(reserva);

            if (enviado)
            {
                _logger.LogInformation("Mensagem enviada: reserva {Id} ({Nome}).", reserva.Id, reserva.NomeCliente);
                await logService.RegistrarAsync(LogAtividadeHttpExtensions.CriarRegistroSistema(
                    AcaoLog.WorkerWhatsAppEnviado,
                    $"Confirmação WhatsApp enviada para reserva #{reserva.Id} ({reserva.NomeCliente})",
                    entidade: "Reserva",
                    entidadeId: reserva.Id));
            }
            else
            {
                _logger.LogWarning("Evolution API ausente. Reserva {Id} marcada sem envio.", reserva.Id);
                await logService.RegistrarAsync(LogAtividadeHttpExtensions.CriarRegistroSistema(
                    AcaoLog.WorkerWhatsAppFalha,
                    $"Evolution API não configurada. Reserva #{reserva.Id} marcada sem envio.",
                    entidade: "Reserva",
                    entidadeId: reserva.Id,
                    sucesso: false));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao notificar reserva {Id}.", reserva.Id);
            await logService.RegistrarAsync(LogAtividadeHttpExtensions.CriarRegistroSistema(
                AcaoLog.WorkerWhatsAppFalha,
                $"Erro ao notificar reserva #{reserva.Id}: {ex.Message}",
                entidade: "Reserva",
                entidadeId: reserva.Id,
                sucesso: false));
        }
    }

    private async Task CancelarNaoConfirmadasAsync(IReservaRepository reservaRepo, ILogAtividadeService logService)
    {
        var reservas = (await reservaRepo.ObterParaCancelamentoAutomaticoAsync()).ToList();
        if (reservas.Count == 0) return;

        _logger.LogInformation("{count} reserva(s) para cancelamento automático.", reservas.Count);

        foreach (var reserva in reservas)
            await CancelarReservaAsync(reserva, reservaRepo, logService);
    }

    private async Task CancelarReservaAsync(Reserva reserva, IReservaRepository reservaRepo, ILogAtividadeService logService)
    {
        try
        {
            reserva.Status = StatusReserva.Cancelada;
            reserva.Observacoes = string.IsNullOrEmpty(reserva.Observacoes)
                ? MotivosCancelamento
                : $"{reserva.Observacoes} | {MotivosCancelamento}";

            await reservaRepo.AtualizarAsync(reserva);
            _logger.LogInformation("Reserva {Id} cancelada automaticamente.", reserva.Id);

            await logService.RegistrarAsync(LogAtividadeHttpExtensions.CriarRegistroSistema(
                AcaoLog.WorkerCancelamentoAutomatico,
                $"Reserva #{reserva.Id} ({reserva.NomeCliente}) cancelada automaticamente (sem confirmação WhatsApp)",
                entidade: "Reserva",
                entidadeId: reserva.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao cancelar reserva {Id}.", reserva.Id);
        }
    }

    private static (IReservaRepository ReservaRepo, IConfiguracaoRepository ConfiguracaoRepo, IWhatsAppService WhatsAppService, ILogAtividadeService LogService)
        ResolverServicos(IServiceScope scope) =>
    (
        scope.ServiceProvider.GetRequiredService<IReservaRepository>(),
        scope.ServiceProvider.GetRequiredService<IConfiguracaoRepository>(),
        scope.ServiceProvider.GetRequiredService<IWhatsAppService>(),
        scope.ServiceProvider.GetRequiredService<ILogAtividadeService>()
    );
}
