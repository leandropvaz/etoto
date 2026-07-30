# E-toto

**E-toto** é o sistema de demonstração da Power Wave Solutions para gestão de **LOTOTO**
(Lockout/Tagout — bloqueio e etiquetagem de energias perigosas), incluindo:

- Autorização de isolamento de energias perigosas (**PLE**) com impressão em 2 vias;
- Gestão de **equipamentos**, **plantas** e **usuários** (multi-planta, perfis de acesso);
- **Avaliação de risco**;
- **Auditoria** e relatórios em PDF;
- Importação em lote (ImportTool).

> Ambiente de demonstração — todos os dados são fictícios.

## Stack

.NET 10 (C#), Blazor (Web + WebAssembly), EF Core 10 + SQL Server, JWT, Azure App Service.

## Rodando localmente

```bash
dotnet restore EToto.Web/EToto.Web.csproj
dotnet run --project EToto.Web
```

Connection strings via `dotnet user-secrets` (dev) ou App Settings do Azure (`ConnectionStrings__Lototo`
e `ConnectionStrings__BlobStorage`).

## Deploy

Push na branch `main` dispara `.github/workflows/deploy.yml`, que publica no Azure App Service
da demo (`e-toto.powerwavesolutions.com.br`).
