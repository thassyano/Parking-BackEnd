using ClosedXML.Excel;
using Estacionamento.Api.Application.DTOs;
using Estacionamento.Api.Domain.Entities;
using Estacionamento.Api.Infrastructure.Repositories;

namespace Estacionamento.Api.Application.Services;

public interface ICaixaService
{
    Task<FechamentoCaixaResponseDto> GerarFechamentoAsync(FiltroFechamentoCaixaDto filtro);
    Task<byte[]> ExportarExcelAsync(FiltroFechamentoCaixaDto filtro);
}

public class CaixaService : ICaixaService
{
    private readonly IReservaRepository _reservaRepository;

    public CaixaService(IReservaRepository reservaRepository)
    {
        _reservaRepository = reservaRepository;
    }

    public async Task<FechamentoCaixaResponseDto> GerarFechamentoAsync(FiltroFechamentoCaixaDto filtro)
    {
        var reservas = await _reservaRepository.ObterPorPeriodoAsync(filtro.DataInicio, filtro.DataFim);
        var lista = reservas.ToList();

        var statusFinalizados = new[] { StatusReserva.CheckoutRealizado, StatusReserva.Confirmada, StatusReserva.CheckinRealizado };

        var reservasPagas = lista.Where(r => statusFinalizados.Contains(r.Status)).ToList();

        return new FechamentoCaixaResponseDto
        {
            DataInicio = filtro.DataInicio,
            DataFim = filtro.DataFim,
            TotalReservas = lista.Count,
            ReservasConfirmadas = lista.Count(r => r.Status == StatusReserva.Confirmada || r.Status == StatusReserva.CheckinRealizado || r.Status == StatusReserva.CheckoutRealizado),
            ReservasCanceladas = lista.Count(r => r.Status == StatusReserva.Cancelada),
            CheckinsRealizados = lista.Count(r => r.DataCheckin.HasValue),
            CheckoutsRealizados = lista.Count(r => r.DataCheckout.HasValue),
            ReceitaTotal = reservasPagas.Sum(r => r.ValorFinal),
            ReceitaPix = reservasPagas.Where(r => r.FormaPagamento == FormaPagamento.Pix).Sum(r => r.ValorFinal),
            ReceitaCartaoCredito = reservasPagas.Where(r => r.FormaPagamento == FormaPagamento.CartaoCredito).Sum(r => r.ValorFinal),
            ReceitaCartaoDebito = reservasPagas.Where(r => r.FormaPagamento == FormaPagamento.CartaoDebito).Sum(r => r.ValorFinal),
            ReceitaDinheiro = reservasPagas.Where(r => r.FormaPagamento == FormaPagamento.Dinheiro).Sum(r => r.ValorFinal),
            VagasCobertas = lista.Count(r => r.TipoVaga == TipoVaga.Coberta && statusFinalizados.Contains(r.Status)),
            VagasDescobertas = lista.Count(r => r.TipoVaga == TipoVaga.Descoberta && statusFinalizados.Contains(r.Status)),
            Reservas = lista.Select(r => new ReservaResumoCaixaDto
            {
                ReservaId = r.Id,
                ClienteNome = r.Cliente.Nome,
                ClienteTelefone = r.Cliente.Telefone,
                PlacaVeiculo = r.Cliente.PlacaVeiculo,
                TipoVaga = r.TipoVaga.ToString(),
                DataReserva = r.DataReserva,
                QtdDias = r.QtdDias,
                ValorFinal = r.ValorFinal,
                FormaPagamento = r.FormaPagamento?.ToString(),
                Status = r.Status.ToString()
            }).ToList()
        };
    }

    public async Task<byte[]> ExportarExcelAsync(FiltroFechamentoCaixaDto filtro)
    {
        var fechamento = await GerarFechamentoAsync(filtro);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Fechamento de Caixa");

        ws.Cell("A1").Value = "Fechamento de Caixa";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 16;

        ws.Cell("A3").Value = "Período:";
        ws.Cell("B3").Value = $"{fechamento.DataInicio:dd/MM/yyyy} a {fechamento.DataFim:dd/MM/yyyy}";

        ws.Cell("A5").Value = "Resumo";
        ws.Cell("A5").Style.Font.Bold = true;
        ws.Cell("A6").Value = "Total de Reservas:";
        ws.Cell("B6").Value = fechamento.TotalReservas;
        ws.Cell("A7").Value = "Confirmadas:";
        ws.Cell("B7").Value = fechamento.ReservasConfirmadas;
        ws.Cell("A8").Value = "Canceladas:";
        ws.Cell("B8").Value = fechamento.ReservasCanceladas;
        ws.Cell("A9").Value = "Check-ins:";
        ws.Cell("B9").Value = fechamento.CheckinsRealizados;
        ws.Cell("A10").Value = "Check-outs:";
        ws.Cell("B10").Value = fechamento.CheckoutsRealizados;

        ws.Cell("A12").Value = "Receita por Forma de Pagamento";
        ws.Cell("A12").Style.Font.Bold = true;
        ws.Cell("A13").Value = "Pix:";
        ws.Cell("B13").Value = fechamento.ReceitaPix;
        ws.Cell("B13").Style.NumberFormat.Format = "R$ #,##0.00";
        ws.Cell("A14").Value = "Cartão Crédito:";
        ws.Cell("B14").Value = fechamento.ReceitaCartaoCredito;
        ws.Cell("B14").Style.NumberFormat.Format = "R$ #,##0.00";
        ws.Cell("A15").Value = "Cartão Débito:";
        ws.Cell("B15").Value = fechamento.ReceitaCartaoDebito;
        ws.Cell("B15").Style.NumberFormat.Format = "R$ #,##0.00";
        ws.Cell("A16").Value = "Dinheiro:";
        ws.Cell("B16").Value = fechamento.ReceitaDinheiro;
        ws.Cell("B16").Style.NumberFormat.Format = "R$ #,##0.00";
        ws.Cell("A17").Value = "TOTAL:";
        ws.Cell("A17").Style.Font.Bold = true;
        ws.Cell("B17").Value = fechamento.ReceitaTotal;
        ws.Cell("B17").Style.NumberFormat.Format = "R$ #,##0.00";
        ws.Cell("B17").Style.Font.Bold = true;

        var row = 19;
        ws.Cell(row, 1).Value = "Detalhamento de Reservas";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;

        var headers = new[] { "ID", "Cliente", "Telefone", "Placa", "Tipo Vaga", "Data Reserva", "Dias", "Valor", "Pagamento", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(row, i + 1).Value = headers[i];
            ws.Cell(row, i + 1).Style.Font.Bold = true;
            ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }
        row++;

        foreach (var reserva in fechamento.Reservas)
        {
            ws.Cell(row, 1).Value = reserva.ReservaId;
            ws.Cell(row, 2).Value = reserva.ClienteNome;
            ws.Cell(row, 3).Value = reserva.ClienteTelefone;
            ws.Cell(row, 4).Value = reserva.PlacaVeiculo ?? "-";
            ws.Cell(row, 5).Value = reserva.TipoVaga;
            ws.Cell(row, 6).Value = reserva.DataReserva.ToString("dd/MM/yyyy");
            ws.Cell(row, 7).Value = reserva.QtdDias;
            ws.Cell(row, 8).Value = reserva.ValorFinal;
            ws.Cell(row, 8).Style.NumberFormat.Format = "R$ #,##0.00";
            ws.Cell(row, 9).Value = reserva.FormaPagamento ?? "-";
            ws.Cell(row, 10).Value = reserva.Status;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
