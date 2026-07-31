using Azure.Storage.Blobs;
using EToto.Application.Interfaces;
using EToto.Application.Services;
using EToto.Domain.Interfaces;
using EToto.Infrastructure.Data;
using EToto.Infrastructure.Data.Repositories;
using EToto.Infrastructure.Data.UnitOfWork;
using EToto.Infrastructure.Storage;
using EToto.Web.Dto;
using EToto.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<LototoConfig>(
    builder.Configuration.GetSection("LoginSemSenha"));

// ===========================================
//  RAZOR COMPONENTS (.NET 8 / Blazor Server)
// ===========================================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

// ===========================================
//  AUTH (APENAS DENTRO DO BLAZOR)
// ===========================================
// Provider customizado que guarda o utilizador em memória
builder.Services.AddScoped<LototoAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<LototoAuthenticationStateProvider>());

// Autorização para usar [Authorize] nos componentes
builder.Services.AddAuthorizationCore();

// Serviço de autenticação de domínio (usa o provider acima)
builder.Services.AddScoped<AuthService>();

// #5a: contexto do executor para a auditoria (lê o UserId do AuthState do Blazor).
builder.Services.AddScoped<EToto.Domain.Interfaces.IExecutorContext, BlazorExecutorContext>();

// ===========================================
//  BANCO DE DADOS
// ===========================================
builder.Services.AddDbContext<LototoContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("Lototo");
    options.UseSqlServer(cs);
});

// ===========================================
//  REPOSITÓRIOS + UOW
// ===========================================

builder.Services.AddSingleton<BlobServiceClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connString = config.GetConnectionString("BlobStorage");
    return new BlobServiceClient(connString);
});

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IEquipamentoRepository, EquipamentoRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IPlantaRepository, PlantaRepository>();
builder.Services.AddScoped<PlantasService>();
builder.Services.AddScoped<UsuariosService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
builder.Services.AddScoped<IEquipamentoExcelParser, EquipmentExcelParser>();
builder.Services.AddScoped<IImageOcrService, AzureImageOcrService>();

builder.Services.AddScoped<ProtectedSessionStorage>();

// Application
builder.Services.AddScoped<IEquipamentoAppService, EquipamentoAppService>();
builder.Services.AddScoped<IPleRepository, PleRepository>();
builder.Services.AddScoped<IPleAppService, PleAppService>();
builder.Services.AddScoped<IAvaliacaoRiscoRepository, AvaliacaoRiscoRepository>();
builder.Services.AddScoped<IAvaliacaoRiscoAppService, AvaliacaoRiscoAppService>();

// #5b: Auditoria — consulta
builder.Services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
builder.Services.AddScoped<AuditoriaService>();

// #6a: Relatório consolidado de usuários (Excel + PDF)
builder.Services.AddScoped<RelatorioUsuariosService>();
builder.Services.AddScoped<RelatorioUsuariosExcelService>();
builder.Services.AddScoped<RelatorioUsuariosPdfService>();

// #6b: Campanhas de revalidação
builder.Services.AddScoped<ICampanhaRepository, CampanhaRepository>();
builder.Services.AddScoped<CampanhaRevalidacaoService>();
builder.Services.AddScoped<EToto.Application.Interfaces.IEmailService, EToto.Infrastructure.Services.LoggerEmailService>();
builder.Services.AddScoped<EToto.Web.Services.AvaliacaoRiscoPdfService>();

builder.Services.AddHttpClient();

builder.Services.AddScoped<EToto.Web.Services.LanguageService>();
builder.Services.AddSingleton<EToto.Web.Services.AmbienteInfo>();
builder.Services.AddScoped<EToto.Application.Services.PainelAlertasService>();
builder.Services.AddScoped<EToto.Web.Services.PlePdfService>();

var app = builder.Build();

// ===========================================
//  PIPELINE HTTP
// ===========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// se tiver página de not-found
app.UseStatusCodePagesWithReExecute("/not-found");

// Antiforgery + assets estáticos (padrão template .NET 8)
app.UseAntiforgery();
app.MapStaticAssets();

// Endpoint para gerar PDF da Avaliação de Risco
app.MapGet("/api/avaliacao-risco/pdf/{id:guid}", async (Guid id,
    EToto.Application.Interfaces.IAvaliacaoRiscoAppService arService,
    EToto.Web.Services.AvaliacaoRiscoPdfService pdfService) =>
{
    var ar = await arService.GetByIdAsync(id);
    if (ar is null) return Results.NotFound();
    var pdf = pdfService.GerarPdf(ar);
    return Results.File(pdf, "application/pdf", $"{ar.Numero}.pdf");
});

// Endpoint para gerar PDF do PLE
app.MapGet("/api/ple/pdf/{id:guid}", async (Guid id,
    EToto.Application.Interfaces.IPleAppService pleService,
    EToto.Web.Services.PlePdfService pdfService) =>
{
    var ple = await pleService.GetByIdWithHistoricoAsync(id);
    if (ple is null) return Results.NotFound();
    var pdf = pdfService.GerarPdf(ple);
    return Results.File(pdf, "application/pdf", $"{ple.Numero}.pdf");
});

// #6a: endpoints de exportação do Relatório de Usuários
app.MapGet("/api/relatorio-usuarios/excel", async (
    int? plantaId, int? perfil, int? tipoVinculo, int? status,
    EToto.Application.Services.RelatorioUsuariosService svc,
    EToto.Web.Services.RelatorioUsuariosExcelService excel) =>
{
    var itens = await svc.GerarAsync(new EToto.Application.Dto.RelatorioUsuariosFiltro
    {
        PlantaId = plantaId, Perfil = perfil, TipoVinculo = tipoVinculo, StatusValidade = status
    });
    var bytes = excel.Gerar(itens);
    return Results.File(bytes, EToto.Web.Services.RelatorioUsuariosExcelService.ContentType,
        $"usuarios-{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
});

app.MapGet("/api/relatorio-usuarios/pdf", async (
    int? plantaId, int? perfil, int? tipoVinculo, int? status,
    EToto.Application.Services.RelatorioUsuariosService svc,
    EToto.Web.Services.RelatorioUsuariosPdfService pdf) =>
{
    var filtro = new EToto.Application.Dto.RelatorioUsuariosFiltro
    {
        PlantaId = plantaId, Perfil = perfil, TipoVinculo = tipoVinculo, StatusValidade = status
    };
    var itens = await svc.GerarAsync(filtro);
    var bytes = pdf.Gerar(itens, filtro);
    return Results.File(bytes, EToto.Web.Services.RelatorioUsuariosPdfService.ContentType,
        $"usuarios-{DateTime.Now:yyyyMMdd-HHmm}.pdf");
});

// Mapeia os Razor Components (Blazor Server)
app.MapRazorComponents<EToto.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
