using ClosedXML.Excel;
using EToto.Application.Dto;

namespace EToto.Web.Services;

// #6a: exportação Excel do relatório consolidado de usuários.
public class RelatorioUsuariosExcelService
{
    public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public byte[] Gerar(IEnumerable<RelatorioUsuarioItemDto> itens)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Usuarios");

        var headers = new[]
        {
            "Login", "Nome completo", "Perfis", "Plantas",
            "Vínculo", "Empresa parceira",
            "Validade do acesso", "Status do acesso",
            "Treinamento concluído", "Validade do treinamento", "Status do treinamento",
            "Último login",
            "Criado por", "Criado em", "Alterado por", "Alterado em"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        var row = 2;
        foreach (var u in itens)
        {
            ws.Cell(row, 1).Value = u.Login;
            ws.Cell(row, 2).Value = u.NomeCompleto;
            ws.Cell(row, 3).Value = string.Join(", ", u.PerfisNomes);
            ws.Cell(row, 4).Value = string.Join(", ", u.PlantasNomes);
            ws.Cell(row, 5).Value = u.TipoVinculoNome;
            ws.Cell(row, 6).Value = u.NomeEmpresa ?? "";
            ws.Cell(row, 7).Value = u.DataValidadeAcesso?.ToLocalTime();
            ws.Cell(row, 7).Style.DateFormat.Format = "dd/mm/yyyy";
            ws.Cell(row, 8).Value = NomeStatus(u.StatusValidadeAcesso);
            ws.Cell(row, 9).Value = u.TreinamentoConcluido ? "Sim" : "Não";
            ws.Cell(row, 10).Value = u.DataValidadeTreinamento?.ToLocalTime();
            ws.Cell(row, 10).Style.DateFormat.Format = "dd/mm/yyyy";
            ws.Cell(row, 11).Value = u.ExigeTreinamento ? NomeStatus(u.StatusValidadeTreinamento) : "—";
            ws.Cell(row, 12).Value = u.DataUltimoLogin?.ToLocalTime();
            ws.Cell(row, 12).Style.DateFormat.Format = "dd/mm/yyyy HH:mm";
            ws.Cell(row, 13).Value = u.CriadoPorNome ?? "";
            ws.Cell(row, 14).Value = u.CriadoEm?.ToLocalTime();
            ws.Cell(row, 14).Style.DateFormat.Format = "dd/mm/yyyy HH:mm";
            ws.Cell(row, 15).Value = u.AlteradoPorNome ?? "";
            ws.Cell(row, 16).Value = u.AlteradoEm?.ToLocalTime();
            ws.Cell(row, 16).Style.DateFormat.Format = "dd/mm/yyyy HH:mm";
            row++;
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(1);

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static string NomeStatus(int status) => status switch
    {
        0 => "—",
        1 => "Vigente",
        2 => "Vencendo",
        3 => "Vencido",
        _ => "?"
    };
}
