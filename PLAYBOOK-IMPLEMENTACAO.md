# Playbook de implementação — backlog aprovado (EToto)

Prompts calibrados com os nomes reais do código. Acione cada item com `/implementar <item>` (ou
`/feature implemente o item #<item> do PLAYBOOK-IMPLEMENTACAO.md`).

## Mapa do código (referência rápida — nomes reais)
- **Entidade usuário:** `EToto.Domain/Entities/Usuarios.cs`
  (campos: `Id, Login, NomeCompleto, SenhaHash, PerfilUsuario Perfil, Ativa, DataCriacao,
  DataAtualizacao, PlantaId/Planta, TreinamentoConcluido, DataTreinamento, PlantasAssociadas`;
  helpers `EhSuperGestor`, `EhUsuarioFinal`).
- **Enum de perfil:** `EToto.Domain/Enums/PerfilUsuario.cs`
  (`Usuario=1, Administrador=2, SuperGestor=3, UsuarioFinal=4, ComandoCentral=5`).
- **Padrão N:M já existente (copiar):** `EToto.Domain/Entities/UsuarioPlanta.cs` + coleção `PlantasAssociadas`.
- **DbContext:** `EToto.Infrastructure/Data/LototoContext.cs`; mapeamentos em
  `Infrastructure/Data/Configurations` e `Infrastructure/Data/Mapping`. Connection string: **`Lototo`**.
- **Login / validação de acesso:** `EToto.Web/Services/AuthService.cs` → `LoginAsync(login, senha, plantaId)`
  (hoje valida senha, planta e já carrega `TreinamentoConcluido`). É AQUI que entram os bloqueios de #2 e #3.
- **Claims/JWT:** `EToto.Application/Services/TokenService.cs` (hoje um único `ClaimTypes.Role`). Multi-perfil entra aqui (#1b).
- **CRUD de usuário:** `EToto.Application/Services/UsuariosService.cs`.
- **Telas (Blazor server) em `EToto.Web/Components/Pages/`:**
  - `Login.razor` (rota `/`)
  - `Usuarios.razor` (gestão de usuários)
  - `Equipamentosrazor.razor` (Pesquisa / Equipamentos)
  - `AvaliacaoRisco.razor` (Avaliação de Risco)
  - `Ple.razor` + `PlePrint.razor` (Bloqueio / PLE)
  - `Plantas.razor`
- **Traduções PT/EN:** NÃO usa `.resx`. É o serviço `EToto.Web/Services/LanguageService.cs`,
  com dois dicionários `_pt` e `_en`; uso nas telas via `Lang["chave"]`. Para traduzir = adicionar a
  chave nos dois dicionários.
- **PDF (referência p/ #6):** já existem `EToto.Web/Services/PlePdfService.cs` e
  `AvaliacaoRiscoPdfService.cs` — copie o padrão para exportar PDF.
- **Importação em lote (p/ #1c e #7):** `EToto.ImportTool` (`BatchImportService.cs`).
- **Auditoria já iniciada:** existem migrations antigas (`AdicionarFKsAuditoria`, `AdicionarAuditoriaUsers`).
  O agente DEVE checar o que já existe antes de criar a tabela de auditoria do #5.

## Princípios
1. **Uma feature por vez.** Termine, revise o diff, aplique a migration no banco de HML, e só então a próxima.
2. **Fatie as grandes** (#1, #5, #6) — já estão fatiadas abaixo.
3. **Trabalhe a partir de `stage`.** O fluxo commita em `stage` mas **não** dá push (você revisa e empurra).
4. **#7 é semi-manual** (BACPAC e execução em produção conduzidos por você).

## Ordem recomendada
`#1a → #1b → #1c → #2 → #3 → #5a → #5b → #6a → #6b → #7`

---

## #1 — Múltiplos perfis por usuário + auditoria de cadastro

### #1a — schema + migração de dados
```
[#1a] Múltiplos perfis — camada de dados.
- Criar entidade de junção `UsuarioPerfil` (UsuarioId, Usuario, Perfil:PerfilUsuario), espelhando o padrão de `UsuarioPlanta`, e adicionar coleção `Perfis` em `Usuarios`. Mapear no LototoContext (Configurations/Mapping).
- Regra: qualquer combinação de perfis permitida, EXCETO SuperGestor que continua ÚNICO (não coexiste com outros). Validar.
- Atualizar helpers `EhSuperGestor`/`EhUsuarioFinal` para olhar a coleção `Perfis`. Manter o campo `Perfil` antigo por compatibilidade durante a transição (documentar plano de remoção).
- Adicionar auditoria de cadastro em `Usuarios`: `CriadoPorId, CriadoEm, AlteradoPorId, AlteradoEm` (FK para Usuarios).
- Migration EF `AdicionarMultiplosPerfisPorUsuario` + migração de dados: cada usuário atual vira uma linha em `UsuarioPerfil` com seu `Perfil` atual.
- Testes da migração e da regra do SuperGestor único.
NÃO mexer em UI, claims ou permissões de tela ainda. Branch `stage`, sem push.
```

### #1b — autenticação/claims + permissões nas telas
```
[#1b] Múltiplos perfis — autenticação e permissões.
- Em `TokenService.cs`, gerar múltiplas claims `ClaimTypes.Role` (uma por perfil da coleção `Perfis`), em vez de uma só.
- Em `AuthService.LoginAsync`, montar a sessão com todos os perfis ativos.
- Revisar as verificações de perfil nas telas `Equipamentosrazor.razor` (Pesquisa), `AvaliacaoRisco.razor` e `Ple.razor` (Bloqueio) para aceitar QUALQUER um dos perfis do usuário (não só um). Conferir também o menu/layout (MainLayout) que hoje usa chaves tipo `nav.restricted_admin`.
- Testes de autorização cobrindo usuários com combinação de perfis.
Branch `stage`, sem push.
```

### #1c — UI de usuários + importação + i18n
```
[#1c] Múltiplos perfis — UI.
- Em `Usuarios.razor`: multi-seleção de perfis (substituir o seletor único). Exibir no detalhe quem criou e quem alterou, com data/hora (campos de #1a).
- Importação automatizada da relação inicial de perfis a partir da planilha revisada, estendendo `EToto.ImportTool/BatchImportService.cs`.
- Traduções: adicionar as chaves novas em `LanguageService.cs` (dicionários `_pt` e `_en`).
- Testes.
Branch `stage`, sem push.
```

---

## #2 — Vínculo Funcionário/Terceiro com validade de acesso
```
[#2] Vínculo Funcionário/Terceiro.
- Em `Usuarios.cs`: novos campos `TipoVinculo` (enum novo: Funcionario|Terceiro), `NomeEmpresa` (string?), `DataValidadeAcesso` (DateTime?). NomeEmpresa e DataValidadeAcesso obrigatórios quando Terceiro.
- Migration EF com default `Funcionario` para os existentes.
- Em `AuthService.LoginAsync`: (a) se `DataValidadeAcesso` vencida, bloquear login com mensagem orientando contato com a Segurança do Trabalho da planta responsável; (b) se faltam ≤30 dias, retornar um aviso a exibir em todo login até renovar.
- `Login.razor` deve exibir essas mensagens (bloqueio e aviso). Em `Usuarios.razor`, destaque visual na listagem para validade ≤30 dias.
- UI condicional em `Usuarios.razor`: campos de Terceiro aparecem só quando TipoVinculo=Terceiro.
- Identificação automática dos Terceiros na carga inicial a partir das abas dedicadas da planilha (ImportTool).
- Traduções PT/EN em `LanguageService.cs` e testes.
Branch `stage`, sem push.
```

## #3 — Validade do treinamento bloqueia acesso
```
[#3] Validade de treinamento.
- ATENÇÃO: `Usuarios.cs` já tem `DataTreinamento` (quando treinou) e `TreinamentoConcluido`. Criar campo NOVO `DataValidadeTreinamento` (DateTime?) — é a validade, diferente do DataTreinamento. Definido e mantido manualmente.
- Migration EF e tratamento na carga inicial.
- Em `AuthService.LoginAsync`, reaproveitando o padrão do #2: bloquear login quando o treinamento estiver vencido, para os perfis que o exigem, com mensagem orientando contato com a SST da planta responsável; aviso 30 dias antes em todo login até renovar.
- Destaque visual na listagem (`Usuarios.razor`).
- Traduções PT/EN e testes.
Branch `stage`, sem push.
```

---

## #5 — Módulo de Auditoria

### #5a — captura + tabela
```
[#5a] Módulo de Auditoria — captura.
- PRIMEIRO: inspecionar o que já existe (migrations `AdicionarFKsAuditoria`, `AdicionarAuditoriaUsers`) e reaproveitar/consolidar em vez de duplicar.
- Tabela de auditoria: tabela afetada, identificador do registro, ação (Criar|Atualizar|Excluir), usuário executor, data/hora, valores anteriores e novos (JSON).
- Captura automática interceptando `SaveChanges/SaveChangesAsync` do `LototoContext`, cobrindo Usuários, Plantas, Equipamentos, Bloqueios (PLE) e Avaliações de Risco.
- Retenção permanente, sem expiração. Índices para consulta (por data, usuário, entidade).
- Migration EF e testes.
Branch `stage`, sem push.
```

### #5b — tela de consulta
```
[#5b] Módulo de Auditoria — consulta.
- Nova tela em `EToto.Web/Components/Pages/` (ex.: `Auditoria.razor`) com filtros por período, usuário executor, tipo de entidade e ação.
- Detalhe da alteração "antes / depois" campo a campo.
- Acesso restrito a SuperGestor e Administrador (seguir o padrão de guarda das outras telas + item de menu no MainLayout).
- Paginação. Traduções PT/EN em `LanguageService.cs` e testes.
Branch `stage`, sem push.
```

---

## #6 — Extração da lista de acessos e revalidação

### #6a — relatório + exportação
```
[#6a] Extração de acessos — relatório.
- Relatório consolidado de usuários ativos: perfis, plantas associadas (PlantasAssociadas), tipo de vínculo, empresa (se Terceiro), validade do acesso, validade do treinamento, último login e responsáveis por cadastro/alteração.
- Filtros por planta, perfil, tipo de vínculo e status de validade (vigente | próximo do vencimento | vencido).
- Exportação Excel e PDF. Para PDF, copiar o padrão de `PlePdfService.cs`/`AvaliacaoRiscoPdfService.cs`.
- Nova tela + traduções PT/EN e testes.
Branch `stage`, sem push.
```

### #6b — campanhas de revalidação
```
[#6b] Campanha de revalidação.
- Conceito de campanha com periodicidade configurável (mensal, trimestral, semestral, anual) — nova entidade + migration.
- Lista de revisão para o gestor responsável com ações: manter, ajustar perfis/validades ou revogar (inativar = `Ativa=false`).
- Trilha completa integrada ao Módulo de Auditoria (#5).
- Notificação por e-mail aos gestores ao iniciar a campanha (verificar se já existe serviço de e-mail; se não, criar abstração).
- Histórico de campanhas anteriores consultável.
- Traduções PT/EN e testes.
Branch `stage`, sem push.
```

---

## #7 — Carga e migração de dados (semi-manual)
> ⚠️ Antes de produção: **BACPAC** de HML e de produção, rodar em HML, validar, e só então produção.
> Os agentes preparam scripts; backup e execução em prod são conduzidos por você.
```
[#7] Carga e migração de dados.
- Estender `EToto.ImportTool/BatchImportService.cs` para importar a planilha revisada: mapeamento por aba (planta + tipo de vínculo), detecção de duplicidades entre abas, tratamento de usuários sem e-mail.
- Script para popular `UsuarioPerfil` (#1) a partir dos usuários atuais.
- Definir `TipoVinculo` padrão `Funcionario` para os existentes.
- NÃO executar em produção — apenas preparar/validar em HML e documentar o passo a passo, incluindo onde fazer o BACPAC.
Branch `stage`, sem push.
```

---

## Fluxo de cada chamada
Cada `/implementar <item>` roda: **@dba → @dev-backend → @dev-frontend → @tester → @doc-writer**,
atualiza o `CHANGELOG.md` e commita em `stage`. **Não** dá `git push` — revise o diff e empurre você
mesmo (push em `stage` dispara o deploy de HML no Azure).
```

---

# Melhorias de UI (lote 2) — adicionadas depois do backlog principal

> Estes itens são só de front-end. O fluxo se resume a **@dev-frontend → @tester → @doc-writer**
> (não precisa de @dba nem @dev-backend). Mesma regra: trabalhar a partir de `stage`, sem `git push`.

## #8 — Melhorar o layout da tela de Usuários (está apertada)
```
[#8] Layout da tela de Usuários — UI.
- Arquivo: EToto.Web/Components/Pages/Usuarios.razor.
- Hoje a tela usa duas colunas lado a lado dentro de um `row`: o formulário em `col-12 col-lg-4` e a lista em `col-12 col-lg-8`. Em telas grandes isso deixa o formulário apertado.
- Mudar para layout EMPILHADO (uma seção embaixo da outra), cada uma em largura total (`col-12`): formulário de cadastro/edição em cima, lista de usuários embaixo (ou a ordem que fizer mais sentido de uso).
- Como o formulário passa a ocupar a largura toda, reorganizar os campos em 2–3 colunas internas (usar `row`/`col-md-*` dentro do card do formulário) para ele não ficar muito alto e aproveitar o espaço horizontal.
- Manter responsividade (em telas pequenas tudo volta a empilhar em 1 coluna). Não alterar a lógica/serviços, só o markup e classes.
- Traduções: provavelmente nenhuma chave nova; se criar texto, adicionar em LanguageService.cs (_pt/_en). Testes de fumaça/render.
Branch `stage`, sem push.
```

## #9 — Loading em todas as telas que aguardam requisições/dados
```
[#9] Indicador de carregamento nas telas — UI.
- Várias telas JÁ têm loading (AvaliacaoRisco, Ple, Equipamentosrazor, Home, Campanhas, CampanhaDetalhe, PlePrint). PRIMEIRO: olhar o padrão visual já usado nessas (spinner + flag de "carregando") e REUTILIZAR o mesmo padrão, para ficar consistente.
- Telas que ainda NÃO têm e precisam: `Usuarios.razor`, `Plantas.razor`, `Auditoria.razor`, `RelatorioUsuarios.razor`. Revisar também as demais e cobrir qualquer ponto que faça requisição/carregue dados sem feedback.
- Mostrar o indicador durante `OnInitializedAsync` e durante ações que chamam o servidor (salvar, filtrar, exportar, etc.), e escondê-lo ao terminar. Desabilitar os botões enquanto carrega para evitar duplo clique.
- Traduções de qualquer texto tipo "Carregando..." em LanguageService.cs (_pt/_en). Testes de fumaça.
Branch `stage`, sem push.
```

## #10 — Loading na tela de Login
```
[#10] Loading no Login — UI.
- Arquivo: EToto.Web/Components/Pages/Login.razor.
- Hoje, ao clicar em "Entrar" (LoginAsync chamando AuthService.LoginAsync), não há feedback visual e parece que nada acontece.
- Adicionar estado de carregamento: ao submeter, desabilitar o botão "Entrar" e mostrar um spinner/indicador (ex.: texto "Entrando..." + spinner) até a autenticação responder. Reabilitar em caso de erro.
- Adicionar a chave de tradução (ex.: "login.signing_in" = "Entrando..." / "Signing in...") em LanguageService.cs (_pt/_en) e usar via Lang["..."].
- Testes de fumaça do fluxo de login.
Branch `stage`, sem push.
```
