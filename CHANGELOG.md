# Changelog

Todas as mudanças notáveis deste projeto serão documentadas aqui.

O formato segue [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/) e versionamento
[SemVer](https://semver.org/lang/pt-BR/).

## [Unreleased]

### Adicionado
- **Espanhol (es-ES) como terceiro idioma.** As 663 chaves de interface ganham o
  dicionário `_es` no `LanguageService`, com paridade total: mesmo conjunto e mesma
  ordem do `_pt`. O seletor do topo passa a mostrar três bandeiras (BR / US / ES) e a
  lista de idiomas vem de `LanguageService.SupportedLanguages` — fonte única, a
  marcação não repete a lista.
- Chave sem tradução no idioma escolhido passa a cair no **português**, não na chave
  crua: uma tela meio traduzida é ruim, mas `ple.action.start` na cara do operador é pior.
- Idioma salvo no navegador só é aceito se ainda existir na lista — um valor antigo em
  `localStorage` não pode prender a interface numa língua que o sistema não tem mais.

### Corrigido
- Seletor de idioma legível na barra branca. As bordas eram `rgba(243,245,242,…)`,
  herança da navbar escura do tema anterior, e sumiam no topo claro: qual idioma estava
  ativo virava adivinhação. Agora o ativo leva a cor de ação e um anel, e há foco visível.

### Nota
- Os PDFs e o Excel (PLE, avaliação de risco, relatório de usuários) seguem em português
  nas três línguas, como já acontecia com o inglês. Decisão explícita: o PLE impresso
  circula no campo em português.

### Alterado
- **Rodapé vira o par do topo.** Estava transparente sobre o fundo, com filete quase
  invisível, e sumia. Agora é a mesma faixa recuada do trilho de navegação, com a marca
  Power Wave dentro e o mesmo filete de marca de 3px — o do topo abre a aplicação, o do
  rodapé fecha. Abaixo de 768px o filete some, porque a barra inferior fixa cobre essa
  borda. O alvo do link levanta em branco, como as pílulas do trilho.

### Alterado
- **Topo deixa de ser um bloco branco só.** Barra de marca, trilho de navegação e
  cartões usavam o mesmo branco e o topo lia como continuação da página. Agora são
  três planos: barra de marca branca flutuando sobre sombra, com filete de marca de
  3px na borda absoluta (azul de interface `#1570DB` — o institucional segue reservado
  ao logo) e divisória fina depois do logo; trilho de navegação recuado no cinza-azulado,
  com as pílulas levantando em branco no hover; conteúdo depois. No celular o trilho
  volta a ser folha branca ao virar barra inferior.

### Corrigido
- **Tinta de estado da linha saía berrante.** O Bootstrap pinta `.table-success > td`
  com verde cheio, e a regra do sistema visual mirava só o `<tr>` — a célula ganhava a
  cascata. Com `td`/`th` no seletor, a linha volta à tinta suave prevista no DESIGN.md.
  Vale para `/ple` e `/avaliacao-risco`.
- Listagem de avaliações passa a tingir a linha inteira nos dois estados (Ativa em verde
  lavado, Inativa em cinza anulado), como a tabela do bloqueio.

### Alterado
- **Avaliação de risco no mesmo vocabulário visual do bloqueio.** A listagem passa a
  usar a tabela do `/ple`: pílula sólida de estado (Ativa = seguro, Inativa = anulada),
  tinta suave na linha inteira, legenda de cores acima e colapso em ficha no celular.
  O nível de risco da matriz deixa os hex soltos (`#dc3545`, `#fd7e14`, `#ffc107`,
  `#28a745`) e passa aos tokens de estado — Alto = perigo · Médio = liberação ·
  Baixo = atenção · Insignificante = seguro. Cor sozinha não é dado: o rótulo escrito
  continua na célula.
- **Cabeçalho do detalhe do bloqueio reorganizado no padrão da avaliação de risco.**
  Nasce o componente `.pw-kv`: rótulo em coluna estreita tingida, valor ao lado, dois
  pares por linha no monitor e ficha empilhada no celular. Usado nas duas telas, com o
  número do documento como âncora e o estado do PLE junto dos demais campos.
- i18n: `ar.none_found` em PT/EN — o "Nenhuma avaliação encontrada." estava fixo em
  português no código.

### Corrigido
- **Botão de fechar saindo do cartão em todos os modais.** O Bootstrap posiciona o `X`
  com `float:right` e `margin:-1rem -1rem -1rem auto`, contando com o padding de 1rem
  que ele mesmo aplica no cabeçalho; como o sistema visual usa outro padding, a margem
  negativa empurrava o botão para fora. Float e margem zerados, alvo de 32px no
  monitor e `--pw-tap` no celular, com foco visível. Vale para os 11 modais e também
  para `.alert` e `.card-header`.
- O "fechar" do detalhe (bloqueio e avaliação de risco) vira `.pw-detail-close` no
  sistema visual, em vez de regra local de uma página só.

### Adicionado
- **Painel de bloqueio** portado do `cn-lototo/stage`, já no layout novo:
  - `/painel` (dentro do app, com seletor de planta e detalhe de requisições/líderes
    para Comando Central e SuperGestor) e `/mural/{codigo}` (tela cheia para TV fixa,
    **sem autenticação**, mesmo comportamento do repositório de origem).
  - `PainelAlertaDto`, `PainelAlertasService`, `MuralLayout`, `MuralGrid`.
  - `IPleRepository.GetPlesAtivosComDetalheAsync` e deep-link `/ple?abrir={id}`
    (clicar num card abre o PLE que originou o bloqueio).
  - Escala tipográfica dedicada à TV (`--pw-t-tv-*`): quem lê está a metros de distância.
  - Cores de estado alinhadas ao `/ple` — no repositório de origem "Em andamento" era
    azul; aqui é **perigo**.
- **Sistema visual próprio** em `wwwroot/css/etoto.css`: tokens de superfície, texto,
  borda, estado, tipografia, raio, elevação e alvo de toque. Cartão branco sobre fundo
  cinza-azulado, cantos de 6 a 20px, sombras difusas, fonte Figtree.
- `PRODUCT.md`, `DESIGN.md` e `.impeccable/design.json` documentando produto e sistema.

### Alterado
- **Layout substituído.** Sai o tema AdminLTE, entra o registro SaaS moderno
  (régua de acabamento: Monday, Asana, ClickUp — preferência registrada em `PRODUCT.md`).
  - Estado do PLE vira **pílula sólida**; "Em andamento" passa a ser PERIGO, não cor
    primária — há energia isolada com gente trabalhando.
  - Azul institucional `#006CB5` reservado ao logo; a interface usa `#1570DB`.
  - Um único mecanismo de seleção (input nativo + `accent-color`); o desenho de caixa
    do `icheck-bootstrap` e do `custom-control` é desligado, o que corrige checkbox e
    radio sobrepondo o texto em Usuários, Plantas e Equipamentos.
  - Tabelas colapsam em ficha no celular (11 de 13; duas têm cabeçalho agrupado e
    mantêm rolagem horizontal). O trilho de navegação vira barra inferior abaixo de 768px.
  - Histórico do PLE e da Avaliação de Risco passam a usar a mesma linha do tempo.
- **Regras de permissão do PLE** trazidas do `cn-lototo/stage`: cancelar apenas no
  status `Criado`, SuperGestor também pode finalizar, e excluir restrito a
  Administrador/SuperGestor.

### Corrigido
- **Três contrastes abaixo de 4.5:1** que estavam anotados no código como aprovados:
  `--pw-warn` (4.37), `--pw-idle` (4.41) e `--pw-ink-3` (4.41 na folha / 4.11 no fundo),
  este último usado em rótulos de campo e cabeçalhos de tabela por todo o sistema.
- Logo da Power Wave invisível no rodapé: o `filter: brightness(0) invert(1)` existia
  para o rodapé escuro antigo e deixava a marca branca sobre branco. No Login o escrito
  (`logo3.png`) nunca chegou a ser incluído.

### Removido
- `wwwroot/css/custom.css`: tokens `--seugarcom` de outro produto, ~60 linhas governando
  uma sidebar que não existe, o bloco `.lototo-layout` sem uso e encoding corrompido.

### Alterado
- Importador Líderes LOTOTO (opção `[8]`) ganha:
  - **Coluna F = Senha**: se preenchida, gera hash SHA-256 hex (mesmo algoritmo do
    `AuthService.GerarHash`) e grava em `SenhaHash`. Vazia → `SenhaHash = ''` (cara
    define no primeiro login).
  - **Operador identificado**: o menu agora pergunta o login do operador que está
    rodando o import. Resolve via `IUsuarioRepository.ObterPorLoginAsync` e passa o `Id`
    pro service, que grava em **`CriadoPorId`** (novos) e **`AlteradoPorId`**
    (atualizações). Login vazio = import anônimo (NULL), com aviso explícito.
  - Cabeçalho de instruções no menu atualizado para mencionar Senha e a derivação de
    login para terceiros/sem email (`primeiro.segundo.ultimo`).
  - Em atualização: hash da senha só é re-gravado se a coluna F vier preenchida E o hash
    bater diferente do atual (não apaga senha existente).
  - Testes: 3 novos casos (`SenhaColF_VaiComoHashSha256`, `SenhaVazia_MantemHashVazio`,
    `CriadoPorId_EhGravado_QuandoInformado`). Suite total: **119 testes**.

- Datagrid com seleção de linha em todas as tabelas de gestão: classe CSS reutilizável
  `.selectable-table` (em `wwwroot/css/custom.css`) define cursor pointer + hover sutil e
  `tr.row-selected` ganha highlight azul com barra lateral. Aplicado em `/usuarios`,
  `/plantas`, `/auditoria`, `/relatorio-usuarios` e `/campanhas` — clicar numa linha a
  marca como selecionada; clicar de novo deseleciona. O destaque convive com as classes
  de status (`table-danger`, `table-warning`) já existentes.
- Busca da tela `/usuarios` passa a procurar também por **vínculo** (Funcionário/Terceiro)
  e **empresa terceirizada**. Placeholder atualizado em PT/EN: "Login, nome, perfil,
  vínculo, empresa ou planta..." / "Login, name, profile, bond, company or plant...".

### Adicionado
- Importador dedicado da planilha **"LÍDERES LOTOTO Versão Final"** (ImportTool opção **[8]**):
  - Lê o formato específico da planilha sem exigir renomeação de abas/colunas:
    cabeçalho na linha 2, abas `FARC/FPIT/FMTZ/FSET/FCTG` (Funcionários) +
    `Parceiros <COD>` (Terceiros).
  - Login derivado: funcionário = parte antes do `@` do email; terceiro =
    `primeiro_nome.último_sobrenome` em minúsculas, sem acentos, ignorando
    `de/da/do/das/dos/e`.
  - Perfil: `"Lider"` → `UsuarioFinal`; `"Comando Central e Lider"` →
    `UsuarioFinal + ComandoCentral` (multi-perfil).
  - Data: serial Excel convertido para `DateTime`.
    `DataValidadeTreinamento = DataTreinamento + 12 meses`. Para Terceiro,
    `DataValidadeAcesso = DataTreinamento + 12 meses` (atende a regra de domínio).
  - Comportamento: dedup do mesmo login entre abas, idempotente (compara antes
    de gravar), modo DRY-RUN.
  - Testes: `LideresLototoImportServiceTests` (8 casos cobrindo mapeamento aba,
    perfil multi, login terceiro, serial Excel, dedup, idempotência, planta
    inexistente). Suite total: **108 testes**.

### Alterado
- Indicador de carregamento no Login (#10): ao submeter o formulário, o botão "Entrar"
  é desabilitado e mostra spinner + `login.signing_in` ("Entrando..." / "Signing in...")
  até a autenticação responder. Em caso de erro/credenciais inválidas ou banner de
  aviso de validade, o botão volta ao estado normal automaticamente. Em sucesso, o
  spinner permanece visível durante o `NavigateTo(forceLoad)` para evitar duplo clique
  durante a transição. Bloqueio defensivo contra duplo clique adicionado.

- Indicadores de carregamento (#9): telas `/usuarios`, `/plantas`, `/auditoria`,
  `/relatorio-usuarios`, `/campanhas` e `/campanhas/{id}` ganham feedback visual durante
  carregamento inicial e ações que disparam requisição.
  - Linha-spinner (`fa-sync-alt fa-spin` + `common.loading`) na tbody enquanto a tela
    está carregando dados — replica o padrão já usado em `Home.razor`.
  - Botões "Salvar"/"Excluir"/"Filtrar"/"Concluir"/"Salvar decisão" trocam o conteúdo
    para spinner + `common.processing` durante a ação e ficam `disabled` para evitar
    duplo clique. Modal de exclusão em `/usuarios` e `/plantas` também desabilita
    "Sim, excluir" enquanto a operação está em andamento.
  - `CampanhaDetalhe`: spinner por linha pendente (cada decisão tem botão Salvar
    independente). Botão "Concluir campanha" desabilita até resposta.
  - Novas chaves PT/EN: `common.loading`, `common.processing`.
- Layout da tela `/usuarios` (#8): cards passam a **empilhar verticalmente** (cada um em
  `col-12`) em vez de ficarem lado a lado (era `col-lg-4` + `col-lg-8`, deixando o
  formulário apertado). O formulário em si ganhou **grid interno em 2 colunas**
  (`col-md-6`) que continua empilhando em telas pequenas — coluna esquerda concentra
  identidade/perfis/treinamento/vínculo, coluna direita concentra plantas/status. Botões
  de Salvar/Excluir e o painel de auditoria seguem em largura total abaixo das colunas.
  Sem mudança de lógica/serviços.

### Adicionado
- Carga e migração de dados — semi-manual (#7):
  - `IUsuarioImportService` + `UsuarioImportService` (EPPlus 8) lê planilha xlsx revisada:
    cada aba é "CodigoPlanta-Funcionarios|Terceiros"; cabeçalho mínimo `Login`+`NomeCompleto`
    (opcionais: Perfil, NomeEmpresa, DataValidadeAcesso, DataValidadeTreinamento).
    Detecção de duplicidades entre abas (mantém o primeiro, reporta os demais); regra de
    Terceiro (empresa+validade obrigatórios) aplicada via `VinculoValidacao.ValidarTerceiro`;
    idempotente (compara antes de gravar); modo **DRY-RUN** que conta o que faria sem
    persistir nada. Usuários sem e-mail não são bloqueados — a entidade `Usuarios` não tem
    campo de e-mail por desenho.
  - `EToto.ImportTool`: nova opção **[7]** no menu, com prompt explícito de dry-run vs
    gravação, exibição do resumo (criados, atualizados, sem alteração, duplicidades, linhas
    inválidas) + lista paginada de avisos/erros.
  - `EToto.Infrastructure.DependencyInjection` registra `IUsuarioRepository`,
    `IPlantaRepository` e `IUnitOfWork` via `TryAddScoped`, permitindo que tanto Web quanto
    ImportTool reusem a mesma configuração sem duplicação.
  - **PLAYBOOK-MIGRACAO-DADOS.md** (novo, raiz do repo): roteiro completo BACPAC → scripts
    EF em ordem → opção [7] do ImportTool → queries de validação → plano de produção e
    rollback. Tabela "o que já está pronto vs semi-manual" deixa claro o que é
    automatizado por #1a/#2 (seed UsuarioPerfis + default Funcionario) e o que depende do
    operador. **Sem execução em produção** — apenas preparar/validar em HML.
  - Testes: `UsuarioImportServiceTests` (8 casos com xlsx gerado em runtime via EPPlus —
    dry-run, gravação, Terceiro com/sem dados, duplicidades entre abas, idempotência,
    planta inexistente, nome de aba inválido). Suite total: **100 testes**.

- Campanhas de revalidação (#6b):
  - Domain: enums `Periodicidade` (Mensal=1, Trimestral=3, Semestral=6, Anual=12),
    `StatusCampanha` (Planejada, EmAndamento, Concluida, Cancelada), `DecisaoRevisao`
    (Manter, Ajustar, Revogar). Entidades `CampanhaRevalidacao` e `ItemCampanhaRevalidacao`
    (snapshot JSON do usuário no início, decisão, decidido por, data, observação).
  - Infra: configurations + migration `AdicionarCampanhaRevalidacao` (2 tabelas, 6 índices,
    FKs com NoAction e índice único `(CampanhaId, UsuarioId)`). Campanhas adicionadas ao
    conjunto auditado de #5a (`AuditoriaCapture`) — toda decisão fica rastreada.
  - Application: `CampanhaRevalidacaoService.CriarCampanhaAsync` popula itens com usuários
    ativos (snapshot JSON), `DecidirItemAsync` aplica regra (Revogar → `Usuarios.Ativa=false`),
    `ConcluirAsync` fecha. `ListarAsync`/`ObterDetalheAsync` para a UI.
  - Email: nova abstração `IEmailService` + implementação default `LoggerEmailService`
    (registra a mensagem no log até SMTP/SendGrid). Notificação automática para
    Administradores/SuperGestores ao criar campanha.
  - Web: telas `/campanhas` (lista com botão "Nova campanha" + modal) e `/campanhas/{id}`
    (revisão item a item — dropdown Manter/Ajustar/Revogar + observação, indicador de
    progresso e botão "Concluir campanha"). Guarda Admin/SuperGestor.
  - DI: `ICampanhaRepository` + `CampanhaRevalidacaoService` + `IEmailService` registrados.
  - MainLayout: novo item **Campanhas** (`fa-bullhorn`) visível só para Admin/SuperGestor.
  - Traduções PT/EN: `nav.campaigns`, `campaign.*` (35 chaves).
  - Testes: `CampanhaRevalidacaoServiceTests` (6 casos — criar popula só ativos e notifica
    gestores; DataFimPrevista respeita periodicidade; revogar inativa; manter preserva;
    concluir muda status + DataFimReal; nome vazio lança). Suite total: **92 testes**.

- Extração de acessos — relatório (#6a):
  - Domain: novo campo `DataUltimoLogin` em `Usuarios` (atualizado por `AuthService.LoginAsync`
    em cada login bem-sucedido). Migration `AdicionarDataUltimoLogin` (1 ALTER TABLE).
  - Application: `RelatorioUsuariosService.GerarAsync(filtro)` consolida usuários ativos com
    perfis, plantas, vínculo, validade de acesso/treinamento, último login e responsáveis.
    Filtros: planta, perfil, tipo de vínculo, status de validade (Vigente/Vencendo/Vencido —
    aplicado ao acesso ou ao treinamento se exigido pelo perfil). Ordena por nome.
  - Web exportações:
    - `RelatorioUsuariosExcelService` (ClosedXML, MIME xlsx) gera planilha `Usuarios` com
      16 colunas formatadas (datas pt-BR, freeze do header).
    - `RelatorioUsuariosPdfService` (QuestPDF, padrão do `PlePdfService`) gera PDF A4 paisagem
      com cabeçalho, resumo dos filtros, tabela de 8 colunas e paginação.
  - Tela `/relatorio-usuarios` (`RelatorioUsuarios.razor`) com filtros, tabela paginada
    (linhas destacadas em vermelho/amarelo conforme pior status) e botões "Excel"/"PDF"
    que chamam os novos endpoints `/api/relatorio-usuarios/excel|pdf?plantaId=…&perfil=…&…`.
    Guarda: SuperGestor + Administrador.
  - MainLayout: novo item **Relatórios** (`fa-file-export`).
  - Traduções PT/EN: `nav.reports`, `report.users.*` (título, filtros, colunas, status,
    exportação, etc.).
  - Testes: `RelatorioUsuariosServiceTests` (6 casos — ativos, filtros perfil/vínculo/status,
    ordenação, ExigeTreinamento por perfil). Suite total: **86 testes**.


  - Application: `AuditoriaService.ConsultarAsync(filtro)` + DTOs `AuditoriaEntradaDto`,
    `AuditoriaConsultaFiltro`, `AuditoriaConsultaResultadoDto`.
  - Domain/Infra: `IAuditoriaRepository` com `AuditoriaConsultaCriterio`; `AuditoriaRepository`
    aplica os filtros (período inclusivo, usuário, entidade, ação), inclui `Usuario`,
    ordena por `ExecutadoEm DESC` e pagina (tamanho clamp 5–200).
  - Web: nova tela `/auditoria` (`Components/Pages/Auditoria.razor`) com filtros (período,
    usuário, entidade, ação), tabela paginada e **modal "antes/depois" campo a campo**
    (diff renderizado a partir dos JSONs gravados em #5a, com destaque amarelo nas linhas
    que mudaram). Guarda no `OnInitializedAsync`: redireciona para `/restricted` se não
    for SuperGestor ou Administrador.
  - DI: `AuditoriaService` + `IAuditoriaRepository` registrados no `Program.cs`.
  - MainLayout: novo item **Auditoria** (ícone `fa-history`) visível apenas para
    Administrador/SuperGestor.
  - Traduções PT/EN: `nav.audit`, `audit.title`, `audit.subtitle`, `audit.filter.*`,
    `audit.col_*`, `audit.action_*`, `audit.diff_*`, `audit.no_results`, `audit.unknown_user`.
  - Testes: `AuditoriaServiceTests` (5 casos) cobrindo filtro por período, combinação
    usuário+entidade+ação, ordenação DESC, paginação e mapeamento `UsuarioNome` via include.
    Suite total: **80 testes**.

- Módulo de Auditoria — captura automática (#5a):
  - Domain: entidade `AuditoriaEntrada` (Id Guid, NomeTabela, ChaveRegistro, Acao, UsuarioId?,
    ExecutadoEm, ValoresAntes/ValoresDepois em JSON). Enum `AcaoAuditoria`
    (Criar=1, Atualizar=2, Excluir=3). Interface `IExecutorContext` para abstrair quem
    está executando a operação.
  - Infrastructure: `AuditoriaCapture` (helper estático) coleta snapshot do `ChangeTracker`
    antes do `SaveChanges`, identificando entidades alvo (Usuarios, Plantas, Equipamento,
    Ple, PleEquipamento, PleHistorico, AvaliacaoRisco, AvaliacaoRiscoItem,
    AvaliacaoRiscoHistorico) e serializando antes/depois em JSON.
    `LototoContext` agora sobrescreve `SaveChanges`/`SaveChangesAsync` para gravar as
    entradas em uma 2ª chamada — a primeira persiste os dados (PKs geradas), a segunda
    grava a trilha. `AuditoriaEntradaConfiguration` define índices `(ExecutadoEm DESC)`,
    `UsuarioId` e `NomeTabela` para consulta (preparando #5b).
  - `BlazorExecutorContext` lê `UserId` do `ClaimsPrincipal` do AuthState; default da
    Infrastructure é `AnonymousExecutorContext` (ImportTool/jobs).
  - Migration `AdicionarTabelaAuditoria` cria a tabela com FK para `Usuarios` (NoAction).
  - Retenção permanente, sem expiração.
  - Testes: `AuditoriaCaptureTests` (6 casos com EF Core InMemory) cobrindo Criar/Atualizar/
    Excluir, executor anônimo, não-recursividade (AuditoriaEntrada não se audita) e
    entidades fora do conjunto auditado. Suite total: **75 testes**.

- Validade do treinamento bloqueia acesso (#3):
  - Domain: novo campo `DataValidadeTreinamento` em `Usuarios` (distinto de `DataTreinamento`,
    que é quando treinou); helper `TreinamentoValidacao` define quais perfis exigem treinamento
    (Usuario, UsuarioFinal, ComandoCentral) e o método `Usuarios.ExigeTreinamentoValido()`
    consulta a coleção `Perfis` (com fallback ao campo legado). `StatusValidadeTreinamento()`
    reusa `VinculoValidacao.AvaliarStatus` — mesma janela de 30 dias.
  - Infra: migration `AdicionarValidadeTreinamento` (1 coluna `datetime2 NULL` em `Usuarios`).
  - Application: `UsuariosDto` ganha `DataValidadeTreinamento`; serviço propaga em Criar/Atualizar.
  - AuthService: quando algum perfil exige treinamento, `LoginAsync` bloqueia com
    `LoginResult.TreinamentoExpirado()` se vencido e devolve `DiasParaVencerTreinamento`
    quando ≤30 dias. `LoginResult` ganha `TreinamentoVencido` e `DiasParaVencerTreinamento`.
  - Login.razor: mensagem específica orientando contato com SST quando treinamento vencido;
    banner agrupa **acesso + treinamento** se ambos estiverem na janela de aviso, com
    botão **Continuar**.
  - Usuarios.razor: campo "Validade do treinamento" condicional (aparece junto da data de
    treinamento quando `TreinamentoConcluido`); coluna **Treinamento** na listagem com
    badge colorida (Vencido/Vencendo/Vigente) ou rótulo "Não exigido para este perfil";
    destaque de linha agora reflete o pior status entre acesso e treinamento.
  - Traduções PT/EN: `login.training_expired`, `login.training_expiring_today`,
    `login.training_expiring_in_days`, `users.training_validity`, `users.col_training`,
    `users.training_not_required`.
  - Testes: `TreinamentoValidacaoTests` (8 casos cobrindo perfis exigidos, fallback ao campo
    legado e janela 0/30/31 dias) + extensões em `UsuariosServiceTests`. Suite total: **69 testes**.

- Vínculo Funcionário/Terceiro com validade de acesso (#2):
  - Domain: enum `TipoVinculo` (Funcionario=1, Terceiro=2), novos campos `TipoVinculo`,
    `NomeEmpresa`, `DataValidadeAcesso` em `Usuarios`. Helper `EhTerceiro` e
    `Usuarios.StatusValidade()`. Classe `VinculoValidacao` com `ValidarTerceiro(...)`
    (Terceiro exige empresa + validade) e `AvaliarStatus(...)` (janela de aviso = 30 dias).
  - Infra: nova `UsuarioVinculoConfiguration`. Migration `AdicionarVinculoFuncionarioTerceiro`
    adiciona as 3 colunas em `Usuarios`, com `defaultValue: 1` (Funcionario) para backfill
    automático de registros existentes.
  - Application: `UsuariosDto` ganha `TipoVinculo`, `NomeEmpresa`, `DataValidadeAcesso`.
    `UsuariosService.Criar`/`Atualizar` chama `VinculoValidacao.ValidarTerceiro` e limpa
    `NomeEmpresa`/`DataValidadeAcesso` quando o tipo voltar a ser Funcionário.
  - AuthService: `LoginAsync` bloqueia login com `LoginResult.Vencido()` quando o acesso está
    vencido e retorna `DiasParaVencer` quando faltam ≤30 dias. `LoginResult` ganha
    `AcessoVencido` e `DiasParaVencer`.
  - Login.razor: mensagem específica (i18n) "Procure a Segurança do Trabalho da planta
    responsável" no bloqueio; banner azul "Seu acesso vence em N dia(s)" com botão
    **Continuar** quando estiver na janela de aviso.
  - Usuarios.razor: combo `Tipo de vínculo`; campos `Empresa terceirizada` e `Validade do
    acesso` aparecem só quando Terceiro. Listagem ganha coluna **Vínculo** com badge
    colorida (`Vencido`/`Vencendo ≤30d`/`Vigente`) e a linha inteira é destacada
    (`table-danger`/`table-warning`).
  - ImportTool: novo `IUsuarioVinculoImportService` + opção **`[6]`** no menu — importa
    CSV `Login;TipoVinculo;NomeEmpresa;DataValidadeAcesso` (data em ISO ou DD/MM/YYYY),
    idempotente, aplicando `VinculoValidacao.ValidarTerceiro` por linha.
  - Traduções PT/EN: `login.access_expired`, `login.access_expiring_today`,
    `login.access_expiring_in_days`, `login.continue`, `users.bond_type`,
    `users.bond_employee`, `users.bond_contractor`, `users.company_name`,
    `users.access_validity`, `users.col_bond`, `users.col_validity`,
    `users.validity_expired`, `users.validity_expiring`, `users.validity_active`,
    `users.toast.company_required`, `users.toast.validity_required`.
  - Testes: 22 testes novos cobrindo a janela de validade (Domain),
    serviço (Terceiro exige empresa+validade, Funcionário limpa campos) e o import CSV
    (idempotência, tipo inválido, regra de Terceiro). Suite total: **53 testes**.

- UI multi-perfil em `Usuarios.razor` (#1c): formulário passa a usar **checkboxes** com regra
  "SuperGestor exclusivo" implementada client-side (marcar SuperGestor desmarca os demais e
  vice-versa). Coluna da listagem mostra todos os perfis. Painel de auditoria (em edição) exibe
  "Criado por X em DD/MM/YYYY HH:mm" e "Alterado por Y em ...".
- `UsuariosDto` ganha `Perfis: List<int>`, `CriadoPorId/CriadoPorNome/CriadoEm`,
  `AlteradoPorId/AlteradoPorNome/AlteradoEm` (saída) e `ExecutadoPorId` (entrada, usado pelo
  serviço para preencher CriadoPorId/AlteradoPorId).
- `UsuariosService` passa a chamar `Usuarios.DefinirPerfis(...)` (regra de domínio de #1a) e
  registra a auditoria de cadastro/alteração. Mapeamento de saída inclui perfis e auditoria.
- `UsuarioRepository.ObterComPlantasAsync` / `ListarComPlantasAsync` agora carregam `Perfis`,
  `CriadoPor` e `AlteradoPor`.
- ImportTool: novo `IUsuarioPerfilImportService` + opção `[5]` no menu — importa relação inicial
  Usuário↔Perfis a partir de um CSV simples (`Login;Perfis`, perfis separados por `,` `|` ou `/`),
  **idempotente** (não regrava se a coleção já está igual). Erros por linha são reportados sem
  interromper o lote; a regra "SuperGestor exclusivo" é validada por `DefinirPerfis`.
- Traduções PT/EN em `LanguageService`: `users.access_profiles`, `users.col_profiles`,
  `users.profile_super_exclusive`, `users.audit_title`, `users.created_by`, `users.updated_by`,
  `users.audit_at`, `users.audit_unknown`, `users.toast.profile_required`,
  `users.toast.profile_super_exclusive`.
- Projeto de testes `EToto.Application.Tests` cobrindo `UsuariosService` (multi-perfil + auditoria)
  e `UsuarioPerfilImportService` (csv, regras, idempotência, linhas inválidas) — 11 testes.

- Autenticação multi-perfil (#1b): `LototoAuthenticationStateProvider` emite **uma claim
  `ClaimTypes.Role` por perfil** do usuário via novo `LototoClaimsBuilder`. Telas existentes que
  já usam `user.IsInRole("...")` (MainLayout, AvaliacaoRisco, Ple, Home, Equipamentosrazor)
  passam a aceitar combinações automaticamente, sem alterações de UI.
- `SerializableUser` ganha `Perfis: List<int>` e `PerfisNomes: List<string>`; `Perfil`/`PerfilNome`
  passam a representar o perfil "primário" (SuperGestor quando presente, senão o primeiro) para
  retrocompat com `FindFirst("Perfil")`.
- `AuthService.LoginAsync` coleta os perfis de `Usuarios.Perfis` (com fallback ao campo legado).
- `ITokenService.GenerateToken(Guid, string, IEnumerable<string>)` — sobrecarga que emite uma
  `ClaimTypes.Role` por perfil no JWT. Sobrecarga de 1 perfil mantida.
- `UsuarioRepository.ObterPorLoginAsync` e `ListarComPlantasAsync` agora fazem `Include(u => u.Perfis)`.
- Projeto de testes `EToto.Web.Tests` cobrindo `LototoClaimsBuilder` (9 testes) — combinações
  com 1, 2 e 3 perfis; compat com `SerializableUser` legado; dedup de perfis repetidos.

- Entidade de junção `UsuarioPerfil` (UsuarioId, Perfil, DataAssociacao) espelhando o padrão de
  `UsuarioPlanta`, e coleção `Perfis` em `Usuarios` para suportar múltiplos perfis por usuário
  (#1a do playbook).
- Campos de auditoria de cadastro em `Usuarios`: `CriadoPorId`, `CriadoEm`, `AlteradoPorId`,
  `AlteradoEm`, com FKs auto-referenciais para `Usuarios` (NoAction para evitar ciclos no SQL Server).
- Método `Usuarios.DefinirPerfis(...)` e classe estática `PerfilUsuarioValidacao` aplicando a regra
  de domínio "SuperGestor não coexiste com outros perfis".
- Migration EF `AdicionarMultiplosPerfisPorUsuario` criando a tabela `UsuarioPerfis`, as colunas de
  auditoria e fazendo seed de uma linha em `UsuarioPerfis` por usuário existente (espelhando o
  `Perfil` legado).
- Projeto de testes `EToto.Domain.Tests` (xUnit) cobrindo a regra do SuperGestor único e o
  comportamento de `DefinirPerfis` / helpers `EhSuperGestor`/`EhUsuarioFinal`.

### Alterado
- `EhSuperGestor` e `EhUsuarioFinal` em `Usuarios` agora olham primeiro a coleção `Perfis` e caem
  para o campo legado `Perfil` quando a coleção não foi carregada (compatibilidade durante a
  transição planejada em #1b/#1c).
