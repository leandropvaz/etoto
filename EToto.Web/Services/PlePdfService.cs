using EToto.Application.Dto;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EToto.Web.Services;

public class PlePdfService
{
    private readonly IWebHostEnvironment _env;

    public PlePdfService(IWebHostEnvironment env)
    {
        _env = env;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GerarPdf(PleDto ple)
    {
        byte[]? logo = null;
        var p = Path.Combine(_env.WebRootPath, "images", "etoto.png");
        if (File.Exists(p)) logo = File.ReadAllBytes(p);

        var doc = Document.Create(c =>
        {
            c.Page(pg => Via(pg, ple, "Autorização de Isolamento", "", false, logo, "Página 1 de 1"));
        });

        return doc.GeneratePdf();
    }

    private static void Via(PageDescriptor page, PleDto ple, string viaFull, string viaShort, bool isVia2, byte[]? logo, string pagina)
    {
        page.Size(PageSizes.A4);
        page.MarginLeft(14);
        page.MarginRight(14);
        page.MarginTop(3);
        page.MarginBottom(3);
        page.DefaultTextStyle(x => x.FontSize(6.5f).FontFamily("Arial"));

        page.Content().Column(col =>
        {
            col.Spacing(0);

            // ═══ BARRA VERMELHA ═══
            col.Item().Height(5).Background("#CC0000");

            // ═══ LOGO + LOTOTO ═══
            col.Item().PaddingTop(2).PaddingBottom(1).Row(row =>
            {
                row.RelativeItem(4).AlignLeft().AlignMiddle().Column(c =>
                {
                    if (logo != null)
                        c.Item().MaxHeight(26).MaxWidth(150).Image(logo).FitArea();
                });
                row.RelativeItem(6).AlignCenter().AlignMiddle()
                    .Text("LOTOTO").FontSize(26).ExtraBold().LetterSpacing(0.12f);
            });

            // ═══ SUBTÍTULO ═══
            col.Item().Border(1.5f).PaddingVertical(1).AlignCenter()
                .Text("AUTORIZAÇÃO DE ISOLAMENTO DE ENERGIAS PERIGOSAS").FontSize(8).ExtraBold();

            // ═══ DADOS ═══
            col.Item().Table(t =>
            {
                t.ColumnsDefinition(c => { c.RelativeColumn(4); c.RelativeColumn(3); c.RelativeColumn(3); });
                t.Cell().Border(0.5f).Padding(1).PaddingLeft(2).AlignMiddle().Text(tx => { tx.Span("Nome da unidade ").Bold(); tx.Span(ple.PlantaNome); });
                t.Cell().Border(0.5f).Padding(1).PaddingLeft(2).AlignMiddle().Text(tx => { tx.Span("Área Fabril: ").Bold(); tx.Span(ple.PlantaNome); });
                t.Cell().Border(0.5f).Padding(1).AlignCenter().Column(c =>
                {
                    c.Item().AlignCenter().Text("Nº Autorização").FontSize(5.5f);
                    c.Item().AlignCenter().Text(ple.Numero).FontSize(9).ExtraBold().FontColor("#CC0000");
                });
            });

            // ═══ TAG + EQUIPAMENTO (resumo distinct, concatenado "TAG - Nome" sem quebra em colunas) ═══
            var tagEquipDistinct = string.Join(", ", ple.Equipamentos
                .Where(e => !string.IsNullOrWhiteSpace(e.Tag) || !string.IsNullOrWhiteSpace(e.Nome))
                .Select(e => string.IsNullOrWhiteSpace(e.Nome) ? e.Tag : $"{e.Tag} - {e.Nome}")
                .Distinct());

            col.Item().Border(0.5f).Padding(1).PaddingHorizontal(2)
                .Text(tx => { tx.DefaultTextStyle(s => s.FontSize(6.5f)); tx.Span("TAG + Equipamento: ").Bold(); tx.Span(tagEquipDistinct); });

            // ═══ EQUIPAMENTOS (preenche a maior parte da página) ═══
            // Total de linhas calculado para caber em 1 página A4 considerando
            // todos os outros blocos (líder, cadeados, bloqueio/teste/desbloqueio, aviso, rodapé).
            var eqCount = ple.Equipamentos.Count;
            const int totalRows = 33;
            var emptyEq = Math.Max(0, totalRows - eqCount);
            const float rowHeight = 12f;

            col.Item().Table(t =>
            {
                t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(4); c.RelativeColumn(3); c.RelativeColumn(3); });
                Hdr(t, "TAG. Disp"); Hdr(t, "Desc. Energia"); Hdr(t, "TIPO DE ENERGIA"); Hdr(t, "TIPO DE BLOQUEIO");

                var eqBg = isVia2 ? "#DCE6F1" : "#FFFFFF";
                foreach (var eq in ple.Equipamentos)
                {
                    t.Cell().Border(0.5f).Background(eqBg).Padding(1).PaddingLeft(2).MinHeight(rowHeight).AlignMiddle().Text(eq.TagDispositivo).FontSize(6).Bold().FontFamily("Courier New");
                    t.Cell().Border(0.5f).Background(eqBg).Padding(1).PaddingLeft(2).MinHeight(rowHeight).AlignMiddle().Text(eq.DescEnergia).FontSize(6);
                    t.Cell().Border(0.5f).Background(eqBg).Padding(1).PaddingLeft(2).MinHeight(rowHeight).AlignMiddle().Text(eq.TipoEnergia).FontSize(6);
                    t.Cell().Border(0.5f).Background(eqBg).Padding(1).PaddingLeft(2).MinHeight(rowHeight).AlignMiddle().Text(eq.TipoBloqueio).FontSize(6);
                }
                for (int i = 0; i < emptyEq; i++)
                    for (int c = 0; c < 4; c++)
                        t.Cell().Border(0.5f).Background(eqBg).Height(rowHeight);
            });

            // ═══ LÍDER DO BLOQUEIO ═══
            col.Item().Table(t =>
            {
                t.ColumnsDefinition(c => { c.RelativeColumn(5); c.RelativeColumn(5); });
                t.Cell().ColumnSpan(2).Border(0.5f).Padding(1).PaddingLeft(2).Text("LÍDER DO BLOQUEIO:").Bold();
                t.Cell().Border(0.5f).Padding(1).PaddingLeft(2).Text(tx => { tx.Span("Nome: ").Bold(); tx.Span(ple.CriadoPorNome); });
                t.Cell().Border(0.5f).Padding(1).AlignRight().PaddingRight(2).Text(tx => { tx.Span("Nº Cadeado(s): ").Bold(); tx.Span("___________"); });
                t.Cell().Border(0.5f).Padding(1).PaddingLeft(2).Text(tx => { tx.Span("Data/hora: ").Bold(); tx.Span(ple.DataInicio.ToString("dd/MM/yyyy HH:mm")); });
                t.Cell().Border(0.5f).Padding(1);
            });

            // ═══ CADEADOS ═══
            col.Item().Table(t =>
            {
                t.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(1.1f);  // CADEADO Nº
                    c.RelativeColumn(1.2f);  // EMPRESA
                    c.RelativeColumn(2.2f);  // NOME LIDERADO
                    c.RelativeColumn(1.4f);  // Assinatura bloq
                    c.RelativeColumn(0.9f);  // Data/hora bloq
                    c.RelativeColumn(1.4f);  // Assinatura desb
                    c.RelativeColumn(0.9f);  // Data/hora desb
                });

                // Header linha 1
                Hdr(t, "CADEADO Nº.");
                Hdr(t, "EMPRESA");
                Hdr(t, "NOME do LIDERADO DO BLOQUEIO");
                HdrC(t, "Assinatura", "#DCE6F1");
                HdrC(t, "Data/hora", "#DCE6F1");
                HdrC(t, "Assinatura", "#D8E4BC");
                HdrC(t, "Data/hora", "#D8E4BC");

                if (!isVia2)
                {
                    for (int r = 0; r < 8; r++)
                        for (int c = 0; c < 7; c++)
                            t.Cell().Border(0.5f).Height(12);
                }
                else
                {
                    // Via 2: 3 cols esquerda vazias, 4 cols direita com NOTA
                    for (int r = 0; r < 8; r++)
                    {
                        for (int c = 0; c < 3; c++)
                            t.Cell().Border(0.5f).Height(12);

                        if (r == 0)
                        {
                            t.Cell().ColumnSpan(4).RowSpan(8).Border(0.5f).Padding(5).AlignMiddle().Column(c =>
                            {
                                c.Item().AlignCenter().Text("NOTA").FontSize(14).ExtraBold().LetterSpacing(0.08f);
                                c.Item().PaddingTop(3).Text(
                                    "Este campo deve ser preenchido apenas na 1ª via do documento pelo líder de bloqueio, coletando as assinaturas no momento da execução do bloqueio e desbloqueio do equipamento. Quando de sua devolução o responsável pelo comando central (ou operador da máquina) deverá conferir se todos os campos estão devidamente assinados recebendo a liberação do equipamento.")
                                    .FontSize(6).LineHeight(1.3f);
                            });
                        }
                    }
                }
            });

            // ═══ BLOQUEIO / TESTE / DESBLOQUEIO ═══
            col.Item().Table(t =>
            {
                t.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(5); c.RelativeColumn(2); });

                // BLOQUEIO
                t.Cell().RowSpan(2).Border(0.5f).Background("#E8E8E8").Padding(1).PaddingLeft(2).Column(c =>
                {
                    c.Item().Text("BLOQUEIO").FontSize(6).ExtraBold();
                    c.Item().Text("Caso o equipamento já esteja bloqueado, é obrigatório a assinatura do responsável pelo equipamento. Ficando dispensado a assinatura do eletricista. Colocar observação \"Equipamento Bloqueado\".").FontSize(5).LineHeight(1.1f);
                });
                SigRow(t, "Ass. Comando Central/Sala de Operações", "Data/Hora:", "#D8E4BC");
                SigRow(t, "Ass. Eletricista", "Data/Hora", "#DCE6F1");

                // TESTE
                t.Cell().Border(0.5f).Background("#E8E8E8").Padding(1).PaddingLeft(2).AlignMiddle()
                    .Text("TESTE DE EFETIVIDADE").FontSize(6).ExtraBold();
                SigRow(t, "Ass. Líder de bloqueio", "Data/Hora:", "#D8E4BC");

                // DESBLOQUEIO
                t.Cell().RowSpan(3).Border(0.5f).Background("#E8E8E8").Padding(1).PaddingLeft(2).Column(c =>
                {
                    c.Item().Text("DESBLOQUEIO").FontSize(6).ExtraBold();
                    c.Item().Text("Caso o equipamento tenha que permanecer bloqueado por outra equipe, não é necessário a assinatura do eletricista. Colocar observação \"Equipamento Bloqueado\".").FontSize(5).LineHeight(1.1f);
                });
                SigRow(t, "Ass. Eletricista", "Data/Hora", "#F4CCCC");
                SigRow(t, "Ass. Líder de bloqueio", "Data/Hora:", "#D8E4BC");
                SigRow(t, "Ass. Comando Central/Sala de Operações", "Data/Hora:", "#D8E4BC");
            });

            // ═══ AVISO ═══
            col.Item().Border(0.5f).Padding(1).PaddingHorizontal(2)
                .Text("Após o término do trabalho a 1ª via e a 2ª via deste documento devem ser juntadas e arquivadas no quadro LOTOTO na sala de comando de operações obrigatoriamente para seu controle.")
                .FontSize(5).Bold().LineHeight(1.15f);

            // ═══ RODAPÉ ═══
            col.Item().PaddingTop(1).Row(row =>
            {
                row.RelativeItem().AlignLeft().Text("REVISÃO : 001").FontSize(5).Italic().FontColor("#999");
                row.RelativeItem().AlignCenter().Text(pagina).FontSize(5).Italic().FontColor("#999");
                row.RelativeItem().AlignCenter().Text(ple.Numero).FontSize(5).Italic().FontColor("#999");
                row.RelativeItem().AlignRight().Text(viaFull).FontSize(5).Italic().FontColor("#999");
            });
        });
    }

    // ── Header cinza ──
    private static void Hdr(TableDescriptor t, string text) =>
        t.Cell().Border(0.5f).Background("#D9D9D9").Padding(1).AlignCenter().AlignMiddle().Text(text).FontSize(5.5f).Bold();

    // ── Header colorido ──
    private static void HdrC(TableDescriptor t, string text, string bg) =>
        t.Cell().Border(0.5f).Background(bg).Padding(1).AlignCenter().AlignMiddle().Text(text).FontSize(5.5f).Bold();

    // ── Linha assinatura com Data/Hora colorido ──
    private static void SigRow(TableDescriptor t, string label, string dh, string dhBg)
    {
        t.Cell().Border(0.5f).Height(12).Background(dhBg).Padding(1).PaddingLeft(2).AlignMiddle().Text(label).FontSize(6).Bold();
        t.Cell().Border(0.5f).Height(12).Background(dhBg).Padding(1).PaddingLeft(2).AlignMiddle().Text(dh).FontSize(6).Bold();
    }
}
