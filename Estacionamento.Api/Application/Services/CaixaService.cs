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
        var pagas = lista.Where(r => r.Pago).ToList();

        return new FechamentoCaixaResponseDto
        {
            DataInicio = filtro.DataInicio,
            DataFim = filtro.DataFim,
            TotalReservas = lista.Count,
            ReservasPagas = pagas.Count,
            ReservasCanceladas = lista.Count(r => r.Status == StatusReserva.Cancelada),
            ReservasPendentes = lista.Count(r => !r.Pago && r.Status != StatusReserva.Cancelada),
            ReceitaTotal = pagas.Sum(r => r.ValorFinal),
            ReceitaPix = pagas.Where(r => r.FormaPagamento == FormaPagamento.Pix).Sum(r => r.ValorFinal),
            ReceitaCartao = pagas.Where(r => r.FormaPagamento == FormaPagamento.Cartao).Sum(r => r.ValorFinal),
            ReceitaDinheiro = pagas.Where(r => r.FormaPagamento == FormaPagamento.Dinheiro).Sum(r => r.ValorFinal),
            VagasCobertas = pagas.Count(r => r.TipoVaga == TipoVaga.Coberta),
            VagasDescobertas = pagas.Count(r => r.TipoVaga == TipoVaga.Descoberta),
            Reservas = lista.Select(r => new ReservaResumoCaixaDto
            {
                ReservaId = r.Id,
                NomeCliente = r.NomeCliente,
                TelefoneCliente = r.TelefoneCliente,
                PlacaVeiculo = r.PlacaVeiculo,
                TipoVaga = r.TipoVaga.ToString(),
                DataEntrada = r.DataEntrada,
                QtdDias = r.QtdDias,
                ValorFinal = r.ValorFinal,
                FormaPagamento = r.FormaPagamento?.ToString(),
                Status = r.Status.ToString(),
                Pago = r.Pago
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
        ws.Cell("A3").Value = "Periodo:";
        ws.Cell("B3").Value = $"{fechamento.DataInicio:dd/MM/yyyy} a {fechamento.DataFim:dd/MM/yyyy}";

        ws.Cell("A5").Value = "Resumo";
        ws.Cell("A5").Style.Font.Bold = true;
        ws.Cell("A6").Value = "Total de Reservas:";  ws.Cell("B6").Value = fechamento.TotalReservas;
        ws.Cell("A7").Value = "Pagas:";              ws.Cell("B7").Value = fechamento.ReservasPagas;
        ws.Cell("A8").Value = "Canceladas:";         ws.Cell("B8").Value = fechamento.ReservasCanceladas;
        ws.Cell("A9").Value = "Pendentes:";          ws.Cell("B9").Value = fechamento.ReservasPendentes;

        ws.Cell("A11").Value = "Receita por Forma de Pagamento";
        ws.Cell("A11").Style.Font.Bold = true;
        ws.Cell("A12").Value = "Pix:";       ws.Cell("B12").Value = fechamento.ReceitaPix;       ws.Cell("B12").Style.NumberFormat.Format = "R$ #,##0.00";
        ws.Cell("A13").Value = "Cartao:";    ws.Cell("B13").Value = fechamento.ReceitaCartao;    ws.Cell("B13").Style.NumberFormat.Format = "R$ #,##0.00";
        ws.Cell("A14").Value = "Dinheiro:";  ws.Cell("B14").Value = fechamento.ReceitaDinheiro;  ws.Cell("B14").Style.NumberFormat.Format = "R$ #,##0.00";
        ws.Cell("A15").Value = "TOTAL:";     ws.Cell("B15").Value = fechamento.ReceitaTotal;     ws.Cell("B15").Style.NumberFormat.Format = "R$ #,##0.00";
        ws.Cell("A15").Style.Font.Bold = true;
        ws.Cell("B15").Style.Font.Bold = true;

        var row = 17;
        var headers = new[] { "ID", "Cliente", "Telefone", "Placa", "Tipo Vaga", "Entrada", "Dias", "Valor", "Pagamento", "Status", "Pago" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(row, i + 1).Value = headers[i];
            ws.Cell(row, i + 1).Style.Font.Bold = true;
            ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }
        row++;

        foreach (var r in fechamento.Reservas)
        {
            ws.Cell(row, 1).Value = r.ReservaId;
            ws.Cell(row, 2).Value = r.NomeCliente;
            ws.Cell(row, 3).Value = r.TelefoneCliente;
            ws.Cell(row, 4).Value = r.PlacaVeiculo ?? "-";
            ws.Cell(row, 5).Value = r.TipoVaga;
            ws.Cell(row, 6).Value = r.DataEntrada.ToString("dd/MM/yyyy");
            ws.Cell(row, 7).Value = r.QtdDias;
            ws.Cell(row, 8).Value = r.ValorFinal;
            ws.Cell(row, 8).Style.NumberFormat.Format = "R$ #,##0.00";
            ws.Cell(row, 9).Value = r.FormaPagamento ?? "-";
            ws.Cell(row, 10).Value = r.Status;
            ws.Cell(row, 11).Value = r.Pago ? "Sim" : "Nao";
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
