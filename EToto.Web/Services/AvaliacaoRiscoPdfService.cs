using EToto.Application.Dto;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EToto.Web.Services;

public class AvaliacaoRiscoPdfService
{
    private readonly IWebHostEnvironment _env;

    public AvaliacaoRiscoPdfService(IWebHostEnvironment env)
    {
        _env = env;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GerarPdf(AvaliacaoRiscoDto ar)
    {
        byte[]? logo = null;
        var p = Path.Combine(_env.WebRootPath, "images", "etoto.png");
        if (File.Exists(p)) logo = File.ReadAllBytes(p);

        var doc = Document.Create(c =>
        {
            c.Page(pg => BuildPage(pg, ar, logo));
        });

        return doc.GeneratePdf();
    }

    private static void BuildPage(PageDescriptor page, AvaliacaoRiscoDto ar, byte[]? logo)
    {
        page.Size(PageSizes.A4.Landscape());
        page.MarginLeft(14);
        page.MarginRight(14);
        page.MarginTop(8);
        page.MarginBottom(8);
        page.DefaultTextStyle(x => x.FontSize(7).FontFamily("Arial"));

        page.Content().Column(col =>
        {
            col.Spacing(0);

            // ═══ CABEÇALHO ═══
            col.Item().Row(row =>
            {
                row.RelativeItem(3).AlignLeft().AlignMiddle().Column(c =>
                {
                    if (logo != null) c.Item().MaxHeight(30).MaxWidth(160).Image(logo).FitArea();
                });
                row.RelativeItem(7).AlignCenter().AlignMiddle().Column(c =>
                {
                    c.Item().AlignCenter().Text("Apêndice H").FontSize(12).ExtraBold();
                    c.Item().AlignCenter().Text("Avaliação de Risco para o Métodos Alternativos").FontSize(8).Bold();
                });
                row.RelativeItem(2).AlignRight().AlignMiddle().Text(ar.Numero).FontSize(9).ExtraBold();
            });

            var plantasNomes = string.Join(" / ", ar.Equipamentos
                .Where(e => !string.IsNullOrWhiteSpace(e.PlantaNome))
                .Select(e => e.PlantaNome)
                .Distinct()
                .OrderBy(n => n));
            if (string.IsNullOrWhiteSpace(plantasNomes)) plantasNomes = "-";

            col.Item().PaddingTop(4).Table(t =>
            {
                t.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(3); c.RelativeColumn(3); c.RelativeColumn(2); });
                t.Cell().Border(0.5f).Padding(2).PaddingLeft(3).Text(tx => { tx.Span("Planta: ").Bold(); tx.Span(plantasNomes); });
                t.Cell().Border(0.5f).Padding(2).PaddingLeft(3).Text(tx => { tx.Span("Departamento: ").Bold(); tx.Span(ar.Departamento); });
                t.Cell().Border(0.5f).Padding(2).PaddingLeft(3).Text(tx => { tx.Span("Operação: ").Bold(); tx.Span(ar.Operacao); });
                t.Cell().Border(0.5f).Padding(2).PaddingLeft(3).Text(tx => { tx.Span("Data: ").Bold(); tx.Span(ar.Data.ToString("dd/MM/yyyy")); });
            });

            col.Item().Table(t =>
            {
                t.ColumnsDefinition(c => { c.RelativeColumn(); });
                t.Cell().Border(0.5f).Padding(2).PaddingLeft(3).Text(tx => { tx.Span("Tarefa: ").Bold(); tx.Span(ar.Tarefa); });
            });

            // ═══ EQUIPAMENTOS ═══
            if (ar.Equipamentos.Any())
            {
                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(5); });
                    Hdr(t, "TAG", 1, 1);
                    Hdr(t, "Equipamento", 1, 1);
                    foreach (var eq in ar.Equipamentos)
                    {
                        t.Cell().Border(0.5f).Padding(2).PaddingLeft(3).Text(eq.Tag).FontSize(7).Bold().FontFamily("Courier New");
                        t.Cell().Border(0.5f).Padding(2).PaddingLeft(3).Text(eq.Nome).FontSize(7);
                    }
                });
            }

            // ═══ TABELA DE RISCOS ═══
            col.Item().PaddingTop(6).Table(t =>
            {
                t.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(1.5f); // Tarefa
                    c.RelativeColumn(1.5f); // Perigos
                    c.RelativeColumn(0.6f); // Nº expostos
                    c.RelativeColumn(1);    // Grav antes
                    c.RelativeColumn(1);    // Prob antes
                    c.RelativeColumn(0.8f); // Risco antes
                    c.RelativeColumn(4);    // Medidas (maior)
                    c.RelativeColumn(1);    // Grav depois
                    c.RelativeColumn(1);    // Prob depois
                    c.RelativeColumn(0.8f); // Risco depois
                });

                // Header row 1
                Hdr(t, "Tarefa", 1, 2);
                Hdr(t, "Perigos", 1, 2);
                Hdr(t, "Número de\npessoas expostas\nao risco", 1, 2);
                HdrC(t, "Antes da redução de riscos", 2, 1, "#C5D9F1");
                HdrC(t, "Nível de risco", 1, 2, "#C5D9F1");
                HdrC(t, "Proteção e Medidas\n(Métodos Alternativos)", 1, 2, "#D9D9D9");
                HdrC(t, "Após a redução do risco", 2, 1, "#D8E4BC");
                HdrC(t, "Nível de\nrisco", 1, 2, "#D8E4BC");

                // Header row 2
                HdrC(t, "Severidade", 1, 1, "#C5D9F1");
                HdrC(t, "Probabilidade", 1, 1, "#C5D9F1");
                HdrC(t, "Severidade", 1, 1, "#D8E4BC");
                HdrC(t, "Probabilidade", 1, 1, "#D8E4BC");

                // Data rows
                foreach (var item in ar.Itens)
                {
                    t.Cell().Border(0.5f).Padding(2).Text(item.Tarefa).FontSize(7);
                    t.Cell().Border(0.5f).Padding(2).Text(item.Perigo).FontSize(7);
                    t.Cell().Border(0.5f).Padding(2).AlignCenter().Text(item.NumeroExpostos.ToString()).FontSize(7);
                    t.Cell().Border(0.5f).Padding(2).AlignCenter().Text(item.GravidadeAntes).FontSize(6.5f);
                    t.Cell().Border(0.5f).Padding(2).AlignCenter().Text(item.ProbabilidadeAntes).FontSize(6.5f);
                    RiskCell(t, item.NivelRiscoAntes);
                    t.Cell().Border(0.5f).Padding(2).Text(item.MedidasProtecao).FontSize(6).LineHeight(1.15f);
                    t.Cell().Border(0.5f).Padding(2).AlignCenter().Text(item.GravidadeDepois).FontSize(6.5f);
                    t.Cell().Border(0.5f).Padding(2).AlignCenter().Text(item.ProbabilidadeDepois).FontSize(6.5f);
                    RiskCell(t, item.NivelRiscoDepois);
                }

                // Sem linhas vazias — tamanho dinâmico
            });

            // ═══ OBSERVAÇÕES ═══
            if (!string.IsNullOrWhiteSpace(ar.Observacoes))
            {
                col.Item().PaddingTop(4).Border(0.5f).Padding(3)
                    .Text($"Observações: {ar.Observacoes}").FontSize(7);
            }

            // ═══ NOTA ═══
            col.Item().PaddingTop(3).Text("(Estimativa de gravidade e probabilidade baseada em máquina sem proteções em vigor. Todos os riscos inicialmente determinados como intoleráveis sem salvaguarda adicional.")
                .FontSize(5.5f).Italic();

            // ═══ RODAPÉ ═══
            col.Item().PaddingTop(2).Row(row =>
            {
                row.RelativeItem().AlignLeft().Text($"Criado por: {ar.CriadoPorNome} — {ar.DataCriacao:dd/MM/yyyy HH:mm}").FontSize(6).FontColor("#999");
                row.RelativeItem().AlignCenter().Text(plantasNomes).FontSize(6).FontColor("#999");
                row.RelativeItem().AlignRight().Text(ar.Numero).FontSize(6).FontColor("#999");
            });
        });
    }

    private static void Hdr(TableDescriptor t, string text, uint colSpan, uint rowSpan)
    {
        t.Cell().ColumnSpan(colSpan).RowSpan(rowSpan).Border(0.5f).Background("#D9D9D9")
            .Padding(2).AlignCenter().AlignMiddle().Text(text).FontSize(6.5f).Bold();
    }

    private static void HdrC(TableDescriptor t, string text, uint colSpan, uint rowSpan, string bg)
    {
        t.Cell().ColumnSpan(colSpan).RowSpan(rowSpan).Border(0.5f).Background(bg)
            .Padding(2).AlignCenter().AlignMiddle().Text(text).FontSize(6.5f).Bold();
    }

    private static void RiskCell(TableDescriptor t, string risco)
    {
        var r = risco.ToLowerInvariant();
        string bg = "#FFFFFF";
        string fg = "#000000";

        if (r.Contains("alto") || r.Contains("high")) { bg = "#CC0000"; fg = "#FFFFFF"; }
        else if (r.Contains("méd") || r.Contains("med")) { bg = "#E36C09"; fg = "#FFFFFF"; }
        else if (r.Contains("baix") || r.Contains("low")) { bg = "#E6B800"; fg = "#111111"; }
        else if (r.Contains("insig")) { bg = "#2E8B2E"; fg = "#FFFFFF"; }

        t.Cell().Border(0.5f).Background(bg).Padding(2).AlignCenter().AlignMiddle()
            .Text(risco).FontSize(7).Bold().FontColor(fg);
    }
}
