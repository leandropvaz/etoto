using EToto.Application.Interfaces;
using EToto.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace EToto.ImportTool;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        // Configuração do Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/import-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            PrintHeader();

            var host = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    // Registrar infraestrutura
                    services.AddInfrastructure(context.Configuration);

                    // Registrar serviços de importação
                    services.AddScoped<IBatchImportService, BatchImportService>();
                    services.AddScoped<IUsuarioPerfilImportService, UsuarioPerfilImportService>();
                    services.AddScoped<IUsuarioVinculoImportService, UsuarioVinculoImportService>();
                    services.AddScoped<IUsuarioImportService, UsuarioImportService>();
                    services.AddScoped<ILideresLototoImportService, LideresLototoImportService>();

                    // Sobrescreve o AnonymousExecutorContext default da Infrastructure por uma
                    // versao mutavel — os metodos do menu setam o UsuarioIdAtual antes do import
                    // para que a auditoria de SaveChangesAsync registre quem rodou.
                    services.AddScoped<EToto.Domain.Interfaces.IExecutorContext, MutableExecutorContext>();
                })
                .Build();

            var running = true;

            while (running)
            {
                PrintMenu();
                var option = Console.ReadKey(intercept: true).KeyChar;
                Console.WriteLine($"\n\nOpção selecionada: {option}\n");

                switch (option)
                {
                    case '1':
                        await ExecuteLocalImportAsync(host);
                        break;

                    case '2':
                        await ExecuteBlobDownloadAsync(host);
                        break;

                    case '3':
                        await ExecuteBlobDownloadAndImportAsync(host);
                        break;

                    case '4':
                        await ListBlobFilesAsync(host);
                        break;

                    case '5':
                        await ExecuteUsuarioPerfilImportAsync(host);
                        break;

                    case '6':
                        await ExecuteUsuarioVinculoImportAsync(host);
                        break;

                    case '7':
                        await ExecuteUsuarioImportAsync(host);
                        break;

                    case '8':
                        await ExecuteLideresLototoImportAsync(host);
                        break;

                    case '0':
                        running = false;
                        Log.Information("Encerrando aplicação...");
                        break;

                    default:
                        Log.Warning("Opção inválida. Tente novamente.");
                        break;
                }

                if (running)
                {
                    Console.WriteLine("\nPressione qualquer tecla para continuar...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Erro fatal na aplicação");
            Console.WriteLine("\nPressione qualquer tecla para sair...");
            Console.ReadKey();
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    static void PrintHeader()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                                                              ║");
        Console.WriteLine("║              LOTOTO - Import Tool v1.0                       ║");
        Console.WriteLine("║              Importação de Equipamentos                      ║");
        Console.WriteLine("║                                                              ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
    }

    static void PrintMenu()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════════ MENU PRINCIPAL ═══════════════════════");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("  [1] Importar arquivos de diretório local");
        Console.WriteLine("  [2] Baixar arquivos do Blob Storage");
        Console.WriteLine("  [3] Baixar e importar do Blob Storage");
        Console.WriteLine("  [4] Listar arquivos disponíveis no Blob Storage");
        Console.WriteLine("  [5] Importar perfis de usuários (CSV)");
        Console.WriteLine("  [6] Importar vínculo Funcionário/Terceiro (CSV)");
        Console.WriteLine("  [7] Importar usuários da planilha revisada (xlsx genérico)");
        Console.WriteLine("  [8] Importar Líderes LOTOTO (planilha de Líderes LOTOTO)");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  [0] Sair");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("══════════════════════════════════════════════════════════════");
        Console.Write("\nEscolha uma opção: ");
    }

    static async Task ExecuteLocalImportAsync(IHost host)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var importService = scope.ServiceProvider.GetRequiredService<IBatchImportService>();

        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          IMPORTAÇÃO DE ARQUIVOS LOCAIS                       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            // Solicitar ID da Planta
            Console.Write("Digite o ID da Planta: ");
            if (!int.TryParse(Console.ReadLine(), out var plantaId) || plantaId <= 0)
            {
                Log.Error("ID da planta inválido.");
                return;
            }

            // Solicitar caminho do diretório
            Console.Write("Digite o caminho do diretório com os arquivos Excel: ");
            var directoryPath = Console.ReadLine()?.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            {
                Log.Error("Diretório inválido ou não existe: {Path}", directoryPath);
                return;
            }

            // Confirmar antes de importar
            var filesCount = Directory.GetFiles(directoryPath, "*.xlsx").Length;
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Encontrados {filesCount} arquivo(s) Excel no diretório.");
            Console.Write("Deseja continuar com a importação? (S/N): ");
            Console.ResetColor();

            var confirm = Console.ReadKey(intercept: true).KeyChar;
            Console.WriteLine();

            if (confirm != 'S' && confirm != 's')
            {
                Log.Information("Importação cancelada pelo usuário.");
                return;
            }

            // Executar importação
            Console.WriteLine();
            await importService.ImportBatchAsync(plantaId, directoryPath);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n✓ Importação concluída com sucesso!");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao executar importação local");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n✗ Erro ao executar importação. Verifique os logs para mais detalhes.");
            Console.ResetColor();
        }
    }

    static async Task ExecuteBlobDownloadAsync(IHost host)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var downloadService = scope.ServiceProvider.GetRequiredService<IBlobDownloadService>();

        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          DOWNLOAD DE ARQUIVOS DO BLOB STORAGE                ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            // Solicitar nome do container
            Console.Write("Digite o nome do container (ex: excel): ");
            var containerName = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(containerName))
            {
                Log.Error("Nome do container inválido.");
                return;
            }

            // Solicitar filtro (opcional)
            Console.Write("Digite o prefixo para filtrar (Enter para todos): ");
            var prefixFilter = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(prefixFilter))
                prefixFilter = null;

            // Solicitar diretório de destino
            Console.Write("Digite o caminho do diretório de destino: ");
            var outputDirectory = Console.ReadLine()?.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                Log.Error("Diretório de destino inválido.");
                return;
            }

            // Listar arquivos disponíveis
            Console.WriteLine("\nListando arquivos disponíveis...");
            var blobs = await downloadService.ListBlobsAsync(containerName, prefixFilter);

            if (!blobs.Any())
            {
                Log.Warning("Nenhum arquivo encontrado no container com o prefixo especificado.");
                return;
            }

            Console.WriteLine($"\nEncontrados {blobs.Count} arquivo(s):");
            foreach (var blob in blobs)
            {
                Console.WriteLine($"  • {blob.Name} ({FormatFileSize(blob.SizeInBytes)})");
            }

            // Confirmar download
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Deseja baixar estes arquivos? (S/N): ");
            Console.ResetColor();

            var confirm = Console.ReadKey(intercept: true).KeyChar;
            Console.WriteLine();

            if (confirm != 'S' && confirm != 's')
            {
                Log.Information("Download cancelado pelo usuário.");
                return;
            }

            // Executar download
            Console.WriteLine("\nIniciando download...");
            var results = await downloadService.DownloadFilesAsync(containerName, outputDirectory, prefixFilter);

            // Mostrar resultado
            var successCount = results.Count(r => r.Success);
            var errorCount = results.Count(r => !r.Success);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Download concluído:");
            Console.WriteLine($"  • Sucesso: {successCount}");
            Console.ResetColor();

            if (errorCount > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  • Erros: {errorCount}");
                Console.ResetColor();
            }

            Console.WriteLine($"\nArquivos salvos em: {outputDirectory}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao executar download");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n✗ Erro ao executar download. Verifique os logs para mais detalhes.");
            Console.ResetColor();
        }
    }

    static async Task ExecuteBlobDownloadAndImportAsync(IHost host)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var downloadService = scope.ServiceProvider.GetRequiredService<IBlobDownloadService>();
        var importService = scope.ServiceProvider.GetRequiredService<IBatchImportService>();

        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║       DOWNLOAD E IMPORTAÇÃO DO BLOB STORAGE                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            // Solicitar ID da Planta
            Console.Write("Digite o ID da Planta: ");
            if (!int.TryParse(Console.ReadLine(), out var plantaId) || plantaId <= 0)
            {
                Log.Error("ID da planta inválido.");
                return;
            }

            // Solicitar nome do container
            Console.Write("Digite o nome do container (ex: excel): ");
            var containerName = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(containerName))
            {
                Log.Error("Nome do container inválido.");
                return;
            }

            // Solicitar filtro (opcional)
            Console.Write("Digite o prefixo para filtrar (Enter para todos): ");
            var prefixFilter = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(prefixFilter))
                prefixFilter = null;

            // Criar diretório temporário
            var tempDirectory = Path.Combine(Path.GetTempPath(), "lototo_import", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDirectory);

            try
            {
                // Listar arquivos
                Console.WriteLine("\nListando arquivos disponíveis...");
                var blobs = await downloadService.ListBlobsAsync(containerName, prefixFilter);

                if (!blobs.Any())
                {
                    Log.Warning("Nenhum arquivo encontrado no container.");
                    return;
                }

                Console.WriteLine($"\nEncontrados {blobs.Count} arquivo(s):");
                foreach (var blob in blobs)
                {
                    Console.WriteLine($"  • {blob.Name} ({FormatFileSize(blob.SizeInBytes)})");
                }

                // Confirmar
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Deseja baixar e importar estes arquivos? (S/N): ");
                Console.ResetColor();

                var confirm = Console.ReadKey(intercept: true).KeyChar;
                Console.WriteLine();

                if (confirm != 'S' && confirm != 's')
                {
                    Log.Information("Operação cancelada pelo usuário.");
                    return;
                }

                // Download
                Console.WriteLine("\nBaixando arquivos...");
                var downloadResults = await downloadService.DownloadFilesAsync(
                    containerName,
                    tempDirectory,
                    prefixFilter);

                var downloadSuccess = downloadResults.Count(r => r.Success);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ {downloadSuccess} arquivo(s) baixado(s) com sucesso");
                Console.ResetColor();

                // Importação
                Console.WriteLine("\nIniciando importação...");
                await importService.ImportBatchAsync(plantaId, tempDirectory);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n✓ Download e importação concluídos com sucesso!");
                Console.ResetColor();
            }
            finally
            {
                // Limpar diretório temporário
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, recursive: true);
                    Log.Debug("Diretório temporário removido: {Path}", tempDirectory);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao executar download e importação");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n✗ Erro ao executar operação. Verifique os logs para mais detalhes.");
            Console.ResetColor();
        }
    }

    static async Task ExecuteUsuarioPerfilImportAsync(IHost host)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var importService = scope.ServiceProvider.GetRequiredService<IUsuarioPerfilImportService>();

        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          IMPORTAÇÃO DE PERFIS DE USUÁRIOS (CSV)              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Formato esperado do CSV (UTF-8):");
            Console.WriteLine("  Login;Perfis");
            Console.WriteLine("  joao.silva;Administrador,UsuarioFinal");
            Console.WriteLine("  maria.silva;SuperGestor");
            Console.WriteLine();
            Console.WriteLine("Perfis válidos: Usuario, Administrador, SuperGestor, UsuarioFinal, ComandoCentral.");
            Console.WriteLine("Separadores aceitos entre perfis: vírgula, pipe (|) ou barra (/).");
            Console.WriteLine();

            Console.Write("Digite o caminho do arquivo CSV: ");
            var csvPath = Console.ReadLine()?.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
            {
                Log.Error("Arquivo CSV inválido ou não existe: {Path}", csvPath);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Deseja prosseguir com a importação? (S/N): ");
            Console.ResetColor();

            var confirm = Console.ReadKey(intercept: true).KeyChar;
            Console.WriteLine();

            if (confirm != 'S' && confirm != 's')
            {
                Log.Information("Importação cancelada pelo usuário.");
                return;
            }

            var result = await importService.ImportFromCsvAsync(csvPath);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nLinhas processadas:      {result.TotalLinhas}");
            Console.WriteLine($"Usuários atualizados:    {result.UsuariosAtualizados}");
            Console.WriteLine($"Usuários não encontrados:{result.UsuariosNaoEncontrados}");
            Console.WriteLine($"Linhas inválidas:        {result.LinhasInvalidas}");
            Console.ResetColor();

            if (result.Erros.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nAvisos/erros:");
                foreach (var erro in result.Erros.Take(20))
                    Console.WriteLine($"  • {erro}");
                if (result.Erros.Count > 20)
                    Console.WriteLine($"  ... e mais {result.Erros.Count - 20} (ver log).");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao importar perfis de usuários");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n✗ Erro ao importar perfis. Verifique os logs para mais detalhes.");
            Console.ResetColor();
        }
    }

    static async Task ExecuteUsuarioVinculoImportAsync(IHost host)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var importService = scope.ServiceProvider.GetRequiredService<IUsuarioVinculoImportService>();

        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║      IMPORTAÇÃO DE VÍNCULO FUNCIONÁRIO/TERCEIRO (CSV)        ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Formato esperado do CSV (UTF-8):");
            Console.WriteLine("  Login;TipoVinculo;NomeEmpresa;DataValidadeAcesso");
            Console.WriteLine("  joao.silva;Funcionario;;");
            Console.WriteLine("  maria.terceira;Terceiro;Empresa XYZ;2026-12-31");
            Console.WriteLine();
            Console.WriteLine("TipoVinculo: Funcionario | Terceiro.");
            Console.WriteLine("Para Terceiro, NomeEmpresa e DataValidadeAcesso (YYYY-MM-DD ou DD/MM/YYYY) são obrigatórios.");
            Console.WriteLine();

            Console.Write("Digite o caminho do arquivo CSV: ");
            var csvPath = Console.ReadLine()?.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
            {
                Log.Error("Arquivo CSV inválido ou não existe: {Path}", csvPath);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Deseja prosseguir com a importação? (S/N): ");
            Console.ResetColor();

            var confirm = Console.ReadKey(intercept: true).KeyChar;
            Console.WriteLine();

            if (confirm != 'S' && confirm != 's')
            {
                Log.Information("Importação cancelada pelo usuário.");
                return;
            }

            var result = await importService.ImportFromCsvAsync(csvPath);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nLinhas processadas:      {result.TotalLinhas}");
            Console.WriteLine($"Usuários atualizados:    {result.UsuariosAtualizados}");
            Console.WriteLine($"Usuários não encontrados:{result.UsuariosNaoEncontrados}");
            Console.WriteLine($"Linhas inválidas:        {result.LinhasInvalidas}");
            Console.ResetColor();

            if (result.Erros.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nAvisos/erros:");
                foreach (var erro in result.Erros.Take(20))
                    Console.WriteLine($"  • {erro}");
                if (result.Erros.Count > 20)
                    Console.WriteLine($"  ... e mais {result.Erros.Count - 20} (ver log).");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao importar vínculo de usuários");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n✗ Erro ao importar vínculo. Verifique os logs para mais detalhes.");
            Console.ResetColor();
        }
    }

    static async Task ExecuteUsuarioImportAsync(IHost host)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var svc = scope.ServiceProvider.GetRequiredService<IUsuarioImportService>();

        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   IMPORTAR USUÁRIOS DA PLANILHA REVISADA (xlsx) — #7         ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Formato esperado:");
            Console.WriteLine("  • Cada ABA representa 'CodigoPlanta-Funcionarios' ou 'CodigoPlanta-Terceiros'.");
            Console.WriteLine("    Ex.: 'PLA-Funcionarios', 'PLA-Terceiros', 'PLB-Funcionarios'...");
            Console.WriteLine("  • Cabeçalho (linha 1) com colunas (mínimo Login e NomeCompleto):");
            Console.WriteLine("    Login | NomeCompleto | Perfil | NomeEmpresa | DataValidadeAcesso | DataValidadeTreinamento");
            Console.WriteLine("  • Datas: YYYY-MM-DD ou DD/MM/YYYY.");
            Console.WriteLine("  • Perfil: Usuario | Administrador | SuperGestor | UsuarioFinal | ComandoCentral.");
            Console.WriteLine();
            Console.WriteLine("Modo recomendado: rode primeiro em DRY-RUN para revisar antes de gravar.");
            Console.WriteLine();

            Console.Write("Digite o caminho do arquivo xlsx: ");
            var path = Console.ReadLine()?.Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                Log.Error("Arquivo inválido: {Path}", path);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Dry-run (S/N)? [S = simular sem gravar] ");
            Console.ResetColor();
            var dryRunKey = Console.ReadKey(intercept: true).KeyChar;
            Console.WriteLine();
            var dryRun = dryRunKey is not 'N' and not 'n';

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"Iniciar importação ({(dryRun ? "DRY-RUN" : "GRAVANDO")})? (S/N) ");
            Console.ResetColor();
            var confirm = Console.ReadKey(intercept: true).KeyChar;
            Console.WriteLine();
            if (confirm is not 'S' and not 's')
            {
                Log.Information("Importação cancelada pelo usuário.");
                return;
            }

            var r = await svc.ImportFromXlsxAsync(path, dryRun);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nModo:                        {(r.DryRun ? "DRY-RUN (nada foi gravado)" : "GRAVAÇÃO")}");
            Console.WriteLine($"Linhas lidas:                 {r.LinhasLidas}");
            Console.WriteLine($"Usuários a criar/criados:     {r.UsuariosCriados}");
            Console.WriteLine($"Usuários a atualizar/atual.:  {r.UsuariosAtualizados}");
            Console.WriteLine($"Usuários sem alteração:       {r.UsuariosSemAlteracao}");
            Console.WriteLine($"Duplicidades entre abas:      {r.DuplicidadesEntreAbas}");
            Console.WriteLine($"Linhas inválidas:             {r.LinhasInvalidas}");
            Console.ResetColor();

            if (r.Avisos.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nAvisos:");
                foreach (var a in r.Avisos.Take(20)) Console.WriteLine($"  • {a}");
                if (r.Avisos.Count > 20) Console.WriteLine($"  ... e mais {r.Avisos.Count - 20}");
                Console.ResetColor();
            }
            if (r.Erros.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nErros:");
                foreach (var e in r.Erros.Take(20)) Console.WriteLine($"  • {e}");
                if (r.Erros.Count > 20) Console.WriteLine($"  ... e mais {r.Erros.Count - 20}");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao importar usuários");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n✗ Erro ao importar. Verifique os logs.");
            Console.ResetColor();
        }
    }

    static async Task ExecuteLideresLototoImportAsync(IHost host)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var svc = scope.ServiceProvider.GetRequiredService<ILideresLototoImportService>();
        var usuarios = scope.ServiceProvider.GetRequiredService<EToto.Domain.Interfaces.IUsuarioRepository>();
        var executor = (MutableExecutorContext)scope.ServiceProvider
            .GetRequiredService<EToto.Domain.Interfaces.IExecutorContext>();

        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   IMPORTAR LÍDERES LOTOTO (planilha de Líderes LOTOTO)     ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Formato esperado:");
            Console.WriteLine("  • Header na LINHA 2 (linha 1 = título 'LÍDERES LOTOTO').");
            Console.WriteLine("  • Cabeçalho fixo:");
            Console.WriteLine("    A=NOME DO COLABORADOR | B=ÁREA | C=EMAIL (ou Empresa) | D=DATA DO TREINAMENTO | E=Perfil de acesso");
            Console.WriteLine("  • Abas:");
            Console.WriteLine("    - FARC, FPIT, FMTZ, FSET, FCTG  → planta de mesmo código, Funcionário.");
            Console.WriteLine("    - 'Parceiros <COD>'             → planta <COD>, Terceiro.");
            Console.WriteLine("  • Perfil: 'Lider' → UsuarioFinal; 'Comando Central e Lider' → UsuarioFinal + ComandoCentral.");
            Console.WriteLine();
            Console.WriteLine("Login derivado: funcionário = parte antes do '@' do email;");
            Console.WriteLine("                terceiro/sem email = primeiro.segundo.ultimo (sem acentos).");
            Console.WriteLine("Senha (col F): se preenchida, gravada com hash SHA-256; se vazia, fica em branco.");
            Console.WriteLine("Validade: DataValidadeTreinamento = DataTreinamento + 12 meses.");
            Console.WriteLine("          DataValidadeAcesso (Terceiro) = DataTreinamento + 12 meses.");
            Console.WriteLine();
            Console.WriteLine("Modo recomendado: rode primeiro em DRY-RUN para revisar antes de gravar.");
            Console.WriteLine();

            Console.Write("Digite o caminho do arquivo xlsx: ");
            var path = Console.ReadLine()?.Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                Log.Error("Arquivo inválido: {Path}", path);
                return;
            }

            // Operador que esta rodando o import — vai para CriadoPorId/AlteradoPorId.
            int? criadoPorId = null;
            Console.Write("Login do OPERADOR rodando o import (Enter = anonimo): ");
            var loginOperador = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(loginOperador))
            {
                var op = await usuarios.ObterPorLoginAsync(loginOperador);
                if (op is null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Operador '{loginOperador}' nao encontrado. Abortando.");
                    Console.ResetColor();
                    return;
                }
                criadoPorId = op.Id;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Operador: {op.NomeCompleto} (Id={op.Id})");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Aviso: import anonimo — CriadoPorId/AlteradoPorId ficarao NULL.");
                Console.ResetColor();
            }

            // Importante: setar o executor ANTES de chamar o service. O interceptor de
            // auditoria do LototoContext usa esse valor no SaveChangesAsync para gravar
            // AuditoriaEntradas.UsuarioId (= quem viu na tela /auditoria como "Usuário").
            executor.UsuarioIdAtual = criadoPorId;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Dry-run (S/N)? [S = simular sem gravar] ");
            Console.ResetColor();
            var dryRunKey = Console.ReadKey(intercept: true).KeyChar;
            Console.WriteLine();
            var dryRun = dryRunKey is not 'N' and not 'n';

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"Iniciar importação ({(dryRun ? "DRY-RUN" : "GRAVANDO")})? (S/N) ");
            Console.ResetColor();
            var confirm = Console.ReadKey(intercept: true).KeyChar;
            Console.WriteLine();
            if (confirm is not 'S' and not 's')
            {
                Log.Information("Importação cancelada pelo usuário.");
                return;
            }

            var r = await svc.ImportFromXlsxAsync(path, dryRun, criadoPorId);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nModo:                        {(r.DryRun ? "DRY-RUN (nada foi gravado)" : "GRAVAÇÃO")}");
            Console.WriteLine($"Linhas lidas:                 {r.LinhasLidas}");
            Console.WriteLine($"Usuários a criar/criados:     {r.UsuariosCriados}");
            Console.WriteLine($"Usuários a atualizar/atual.:  {r.UsuariosAtualizados}");
            Console.WriteLine($"Usuários sem alteração:       {r.UsuariosSemAlteracao}");
            Console.WriteLine($"Duplicidades entre abas:      {r.DuplicidadesEntreAbas}");
            Console.WriteLine($"Linhas inválidas:             {r.LinhasInvalidas}");
            Console.ResetColor();

            if (r.Avisos.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nAvisos:");
                foreach (var a in r.Avisos.Take(30)) Console.WriteLine($"  • {a}");
                if (r.Avisos.Count > 30) Console.WriteLine($"  ... e mais {r.Avisos.Count - 30}");
                Console.ResetColor();
            }
            if (r.Erros.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nErros:");
                foreach (var e in r.Erros.Take(30)) Console.WriteLine($"  • {e}");
                if (r.Erros.Count > 30) Console.WriteLine($"  ... e mais {r.Erros.Count - 30}");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao importar Líderes LOTOTO");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n✗ Erro ao importar. Verifique os logs.");
            Console.ResetColor();
        }
    }

    static async Task ListBlobFilesAsync(IHost host)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var downloadService = scope.ServiceProvider.GetRequiredService<IBlobDownloadService>();

        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          LISTAR ARQUIVOS DO BLOB STORAGE                     ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            // Solicitar nome do container
            Console.Write("Digite o nome do container (ex: excel): ");
            var containerName = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(containerName))
            {
                Log.Error("Nome do container inválido.");
                return;
            }

            // Solicitar filtro (opcional)
            Console.Write("Digite o prefixo para filtrar (Enter para todos): ");
            var prefixFilter = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(prefixFilter))
                prefixFilter = null;

            // Listar arquivos
            Console.WriteLine("\nListando arquivos...");
            var blobs = await downloadService.ListBlobsAsync(containerName, prefixFilter);

            if (!blobs.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nNenhum arquivo encontrado.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine($"\n═══════════════════════════════════════════════════════════════");
            Console.WriteLine($"Encontrados {blobs.Count} arquivo(s):\n");

            foreach (var blob in blobs.OrderBy(b => b.Name))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"  • {blob.Name}");
                Console.ResetColor();
                Console.WriteLine($" ({FormatFileSize(blob.SizeInBytes)})");

                if (blob.LastModified.HasValue)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"    Modificado: {blob.LastModified.Value:yyyy-MM-dd HH:mm:ss}");
                    Console.ResetColor();
                }

                Console.WriteLine();
            }

            Console.WriteLine($"═══════════════════════════════════════════════════════════════");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao listar arquivos");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n✗ Erro ao listar arquivos. Verifique os logs para mais detalhes.");
            Console.ResetColor();
        }
    }

    static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}