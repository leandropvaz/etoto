# EToto — Contexto do projeto

Sistema de demonstração **E-toto**: gestão de LOTOTO (bloqueio/etiquetagem — Lockout/Tagout)
e módulos relacionados (PLE, equipamentos, plantas, auditoria). Backend .NET + frontend Blazor,
hospedado no Azure. Este repositório é a versão de demonstração da Power Wave Solutions,
com dados fictícios.

## Stack
- **.NET 10** (C#). O projeto `EToto.Client` (Blazor WebAssembly) é **net9.0**; os demais são **net10.0**.
- **Blazor**: `EToto.Web` (host/Components) + `EToto.Client` (Blazor WebAssembly, `Pages/`, `.razor`).
- **EF Core 10** com **SQL Server**. DbContext: `LototoContext`.
- **Autenticação**: JWT Bearer.
- Solução: `EToto.sln`.

## Arquitetura (Clean Architecture)
- `EToto.Domain` — entidades, enums, interfaces de domínio. Sem dependências de infra.
- `EToto.Application` — casos de uso, Services, DTOs, interfaces. Orquestra o domínio.
- `EToto.Infrastructure` — EF Core (`Data/LototoContext.cs`, `Data/Configurations`, `Data/Mapping`,
  `Data/Repositories`, `Data/UnitOfWork`), `Migrations/`, `Storage`, `DependencyInjection.cs`.
- `EToto.Web` — host Blazor / API / `Program.cs` / `appsettings*.json`.
- `EToto.Client` — UI Blazor WebAssembly (`Pages/`, `Layout/`, `Services/`).
- `EToto.ImportTool` — utilitário de importação em lote.

Regra de dependência: Domain ← Application ← Infrastructure/Web. Não criar referências invertidas.

## Comandos
- Build da solução: `dotnet build EToto.sln -c Debug`
- Rodar o site: `dotnet run --project EToto.Web`
- Restore: `dotnet restore EToto.Web/EToto.Web.csproj`
- Testes: `dotnet test`

## Banco de dados (EF Core)
- Migrations ficam em `EToto.Infrastructure/Migrations`.
- Criar migration:
  `dotnet ef migrations add <Nome> --project EToto.Infrastructure --startup-project EToto.Web`
- Aplicar:
  `dotnet ef database update --project EToto.Infrastructure --startup-project EToto.Web`
- Nomes de migration descritivos e em português.

## Git / Deploy (importante)
- **`main` = DEMO.** Push em `main` dispara o GitHub Actions `deploy.yml` e publica no
  App Service de demonstração no Azure (domínio `e-toto.powerwavesolutions.com.br`).
- Commits no padrão Conventional Commits (`feat:`, `fix:`, `chore:`, `docs:`).
- Este ambiente usa **somente dados fictícios** — nunca importar dados reais de clientes.

## Convenções de código
- C# com nullable/ações assíncronas onde fizer sentido; siga o estilo já existente nos arquivos vizinhos.
- DTOs em `Dto/`; mapeamentos de entidade em `Infrastructure/Data/Mapping` e `Configurations`.
- Componentes Blazor em PascalCase (`.razor`); páginas em `EToto.Client/Pages`.
- Mudanças mínimas e focadas no que foi pedido; não refatorar o que não foi solicitado.

## Arquivos sensíveis (nunca ler/gravar)
`appsettings*.json` com segredos, `.env`, connection strings, chaves do Azure, `secrets/`.
