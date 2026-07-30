using EToto.Application.Dto;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EToto.Web.Services;

// #6a: exportação PDF do relatório consolidado de usuários. Segue padrão do PlePdfService.
public class RelatorioUsuariosPdfService
{
    public const string ContentType = "application/pdf";

    private readonly IWebHostEnvironment _env;

    public RelatorioUsuariosPdfService(IWebHostEnvironment env)
    {
        _env = env;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Gerar(IReadOnlyList<RelatorioUsuarioItemDto> itens, RelatorioUsuariosFiltro filtro)
    {
        byte[]? logo = null;
        var p = Path.Combine(_env.WebRootPath, "images", "etoto.png");
        if (File.Exists(p)) logo = File.ReadAllBytes(p);

        var doc = Document.Create(c =>
        {
            c.Page(pg =>
            {
                pg.Size(PageSizes.A4.Landscape());
                pg.Margin(18);
                pg.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                pg.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().AlignLeft().AlignMiddle().Column(c2 =>
                        {
                            if (logo != null)
                                c2.Item().MaxHeight(28).MaxWidth(120).Image(logo).FitArea();
                        });
                        row.RelativeItem().AlignCenter().AlignMiddle()
                            .Text("Relatório de Usuários Ativos").FontSize(16).Bold();
                        row.RelativeItem().AlignRight().AlignMiddle()
                            .Text($"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8);
                    });
                    col.Item().PaddingTop(4).Text(MontarResumoFiltros(filtro, itens.Count)).FontSize(8);
                });

                pg.Content().PaddingTop(6).Table(t =>
                {
                    t.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2);  // Login
                        cols.RelativeColumn(3);  // Nome
                        cols.RelativeColumn(3);  // Perfis
                        cols.RelativeColumn(3);  // Plantas
                        cols.RelativeColumn(2);  // Vínculo
                        cols.RelativeColumn(2);  // Validade acesso
                        cols.RelativeColumn(2);  // Treinamento
                        cols.RelativeColumn(2);  // Último login
                    });

                    t.Header(h =>
                    {
                        Cabeca(h.Cell(), "Login");
                        Cabeca(h.Cell(), "Nome");
                        Cabeca(h.Cell(), "Perfis");
                        Cabeca(h.Cell(), "Plantas");
                        Cabeca(h.Cell(), "Vínculo");
                        Cabeca(h.Cell(), "Validade acesso");
                        Cabeca(h.Cell(), "Treinamento");
                        Cabeca(h.Cell(), "Último login");
                    });

                    foreach (var u in itens)
                    {
                        Linha(t.Cell(), u.Login);
                        Linha(t.Cell(), u.NomeCompleto);
                        Linha(t.Cell(), string.Join(", ", u.PerfisNomes));
                        Linha(t.Cell(), string.Join(", ", u.PlantasNomes));
                        Linha(t.Cell(), MontarVinculo(u));
                        Linha(t.Cell(), MontarValidade(u.DataValidadeAcesso, u.StatusValidadeAcesso));
                        Linha(t.Cell(),
                            u.ExigeTreinamento
                                ? MontarValidade(u.DataValidadeTreinamento, u.StatusValidadeTreinamento)
                                : "—");
                        Linha(t.Cell(),
                            u.DataUltimoLogin?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "—");
                    }
                });

                pg.Footer().AlignRight().Text(t =>
                {
                    t.Span("Página ");
                    t.CurrentPageNumber();
                    t.Span(" de ");
                    t.TotalPages();
                });
            });
        });

        return doc.GeneratePdf();
    }

    private static void Cabeca(IContainer cell, string texto)
    {
        cell.Background("#E5E7EB").Padding(3).Text(texto).Bold();
    }

    private static void Linha(IContainer cell, string texto)
    {
        cell.BorderBottom(0.5f).BorderColor("#D1D5DB").Padding(3).Text(texto ?? "");
    }

    private static string MontarVinculo(RelatorioUsuarioItemDto u)
    {
        if (u.TipoVinculo == 2) // Parceiro
        {
            return string.IsNullOrWhiteSpace(u.NomeEmpresa)
                ? "Parceiro"
                : $"Parceiro · {u.NomeEmpresa}";
        }
        return "Funcionário";
    }

    private static string MontarValidade(DateTime? data, int status)
    {
        if (!data.HasValue) return "—";
        var rotulo = status switch
        {
            1 => "Vigente",
            2 => "Vencendo",
            3 => "Vencido",
            _ => ""
        };
        return $"{data.Value.ToLocalTime():dd/MM/yyyy} · {rotulo}";
    }

    private static string MontarResumoFiltros(RelatorioUsuariosFiltro f, int qtd)
    {
        var partes = new List<string> { $"Total: {qtd}" };
        if (f.PlantaId.HasValue) partes.Add($"Planta filtrada: #{f.PlantaId}");
        if (f.Perfil.HasValue) partes.Add($"Perfil: {(EToto.Domain.Enums.PerfilUsuario)f.Perfil}");
        if (f.TipoVinculo.HasValue) partes.Add($"Vínculo: {(EToto.Domain.Enums.TipoVinculo)f.TipoVinculo}");
        if (f.StatusValidade.HasValue) partes.Add($"Status: {(EToto.Domain.Enums.StatusValidadeAcesso)f.StatusValidade}");
        return string.Join(" · ", partes);
    }
}
