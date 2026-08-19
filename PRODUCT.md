# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Todos os perfis do sistema são usuários primários — não há um público secundário. O produto
atende duas cenas de uso igualmente reais, e trabalho futuro precisa servir as duas:

**Em campo, junto ao equipamento** — abrir, acompanhar, liberar e conferir o PLE no ponto de
bloqueio. Ambiente industrial, tempo curto, frequentemente em dispositivo móvel ou tablet.

**No escritório / sala de operações** — cadastro, avaliação de risco, auditoria, relatórios,
revalidação de acessos. Sessões longas, muitos registros, monitor grande.

Perfis (`EToto.Domain/Enums/PerfilUsuario.cs`), com o que cada um faz:

| Perfil | Job |
|---|---|
| `Usuario` (1) | Vê e pesquisa equipamentos da própria planta. |
| `Administrador` (2) | Gerencia usuários e equipamentos da planta. |
| `SuperGestor` (3) | Vê todas as plantas e cadastra administradores. Perfil único — não coexiste com outros. |
| `UsuarioFinal` (4) | Líder de Bloqueio. Acesso completo exceto criar usuário e planta. |
| `ComandoCentral` (5) | Sala de Operações. Finaliza bloqueios e imprime; avaliação de risco só consulta + impressão. |

Um usuário pode ter múltiplos perfis e estar associado a múltiplas plantas
(`UsuarioPerfil`, `UsuarioPlanta`).

## Product Purpose

E-toto é o sistema de gestão de **LOTOTO** (Lockout/Tagout — bloqueio e etiquetagem de energias
perigosas) da Power Wave Solutions. Ele conduz o ciclo de vida da autorização de isolamento de
energias perigosas — o **PLE** — do cadastro do equipamento até a liberação e a trilha de
auditoria, controlando quem tem permissão para executar bloqueio em qual planta e se essa
permissão ainda é válida.

Sucesso é operacional, não estético: nenhum bloqueio executado por quem não está habilitado,
nenhuma etapa do PLE perdida ou ambígua, e trilha suficiente para responder a uma auditoria de
segurança do trabalho.

## Positioning

O diferencial não é o formulário de bloqueio — é o acoplamento entre **habilitação da pessoa** e
**execução do bloqueio**. O E-toto trata treinamento, tipo de vínculo (funcionário/terceiro),
validade de acesso e campanha de revalidação como parte do fluxo de LOTOTO, não como cadastro
paralelo: o login bloqueia quem está com treinamento ou acesso vencido, avisa 30 dias antes, e a
revalidação periódica devolve ao gestor a lista viva de quem ainda deve ter acesso. Um concorrente
que só digitalize a etiqueta de bloqueio não consegue afirmar isso.

## Operating Context

- **Ciclo do PLE** (`StatusPle`): `Criado` → `EmAndamento` → `InicioDesbloqueio` → `Finalizado`,
  com `Cancelado` como saída. O PLE é **impresso em 2 vias** e o papel acompanha o trabalho no
  campo — o artefato físico é parte do processo, não um extra.
- **Multi-planta.** Plantas identificadas por código (ex.: FARC, FPIT, FMTZ, FSET, FCTG). Usuários
  são associados a uma ou mais; o `SuperGestor` transita por todas.
- **Habilitação com validade.** `TipoVinculo` (Funcionário | Terceiro; terceiro exige empresa e
  data de validade de acesso), `DataValidadeTreinamento`, e `StatusValidadeAcesso`
  (`SemValidade` | `Vigente` | `Vencendo` ≤30 dias | `Vencido`). Vencido bloqueia o login com
  orientação para procurar a SST da planta; vencendo mostra aviso em todo login até renovar.
- **Campanhas de revalidação** com periodicidade `Mensal | Trimestral | Semestral | Anual`;
  o gestor decide `Manter | Ajustar | Revogar` para cada acesso.
- **Auditoria permanente**, capturada automaticamente no `SaveChanges` do `LototoContext`
  (`Criar | Atualizar | Excluir`, com valores antes/depois em JSON). Sem expiração.
- **Carga inicial por planilha** via `EToto.ImportTool` — a relação de líderes LOTOTO chega em
  Excel, por aba de planta e de parceiros.

## Capabilities and Constraints

Superfícies existentes (`EToto.Web/Components/Pages/`, rotas reais):

`/` login · `/home` pesquisa · `/equipamentos` · `/ple` e `/ple/print/{id}` · `/avaliacao-risco` ·
`/usuarios` (com sub-abas de auditoria, relatório de usuários e revalidação) · `/plantas` ·
`/restricted` · `/not-found` · `/Error`

Restrições técnicas:

- .NET 10, Blazor Server interativo (`InteractiveServer`) no `EToto.Web`; `EToto.Client`
  (WebAssembly, net9.0) existe mas hoje carrega pouco.
- EF Core 10 + SQL Server (`LototoContext`), autenticação JWT, hospedagem Azure App Service.
- Clean Architecture: Domain ← Application ← Infrastructure/Web. Sem referências invertidas.
- Sem `.resx`: a tradução é o `EToto.Web/Services/LanguageService.cs`, com os dicionários `_pt`,
  `_en`, `_es` e `_it` e uso `Lang["chave"]` nas telas. Os quatro têm as mesmas 663 chaves;
  chave ausente cai no português, não na chave crua.
- A base visual atual é AdminLTE 3 / Bootstrap 4 com jQuery. **Não é uma restrição pinada** — o
  usuário deliberadamente não a marcou como intocável quando perguntado. Tratar como incumbente
  a ser avaliada, não como fundação obrigatória.

Fatos de produto ainda em aberto:

- O campo legado `Usuarios.Perfil` (singular) convive com a coleção `Perfis` durante a transição;
  o plano de remoção ainda não tem data.
- Existe um serviço de e-mail previsto para as campanhas de revalidação (#6b do playbook) que
  ainda não foi confirmado como existente.

## Brand Commitments

- Nome do produto: **E-toto**. Fabricante: **Power Wave Solutions** (powerwavesolutions.net),
  presente no rodapé de toda tela autenticada.
- Assets de marca em `EToto.Web/wwwroot/images/`: `etoto.png`, `etoto-branco.png` (versão para
  fundo escuro, usada na navbar), `logo.png`, `logo2.png`, `logo3.png` (Power Wave).
- Azul institucional em uso hoje: `#006CB5` (`.bg-pw-blue`) e `#1F86D1` (`.bg-pw-blue-light`,
  navbar e rodapé).
- **Multilíngue PT/EN/ES/IT é obrigatório.** Todo texto visível ao usuário passa por
  `LanguageService`, nas quatro línguas. Nada hard-coded na tela. Uma chave nova só está
  pronta quando existe nos quatro dicionários. *(Confirmado pelo usuário como intocável.)*
  Os PDFs e o Excel (PLE, avaliação de risco, relatório de usuários) permanecem em
  português nas quatro línguas — decisão explícita do usuário, alinhada ao fato de o PLE
  impresso circular no campo em português.
- **Guardas por perfil e multi-planta são verdade de produto.** Quem vê o quê não pode ser
  afrouxado por conveniência visual. *(Confirmado pelo usuário como intocável.)*
- **Direção visual fixada pelo usuário: SaaS moderno, "clean", cantos arredondados.**
  A régua de acabamento são **Monday, Asana e ClickUp**. Preferência declarada
  explicitamente depois de ver e rejeitar uma direção autoral de desenho técnico
  ("ficou muito tudo quadrado", "quero algo clean"). Isto é um compromisso
  permanente, não uma preferência de uma sessão: trabalho futuro não deve
  reintroduzir cantos retos, paleta de papel/grafite ou tipografia de prancha.
  Dentro disso: as cores de estado do PLE permanecem (em tons modernos), e o
  azul institucional é modernizado para a interface — o `#006CB5` exato fica
  reservado a logo e material institucional.
- **Impressão do PLE em 2 vias e os PDFs são artefatos operacionais.** `PlePdfService`,
  `AvaliacaoRiscoPdfService`, `RelatorioUsuariosPdfService`, `RelatorioUsuariosExcelService` e a
  tela `PlePrint.razor` (A4, `@page { size: A4; margin: 0 }`) não mudam de layout sem pedido
  explícito. *(Confirmado pelo usuário como intocável.)*

## Evidence on Hand

- Documentação real no repositório: `README.md`, `CHANGELOG.md` (Keep a Changelog + SemVer, com
  histórico detalhado das entregas), `PLAYBOOK-IMPLEMENTACAO.md` (backlog aprovado, itens #1–#10).
- Suíte de testes real: `EToto.Domain.Tests`, `EToto.Application.Tests`, `EToto.Web.Tests`
  (119 testes no último registro do CHANGELOG).
- Interface funcional e completa já implementada nas rotas listadas acima — é a fonte de verdade
  visual incumbente.
- **Não existe** e não deve ser inventado: cliente nomeado, depoimento, estudo de caso, benchmark,
  número de plantas ou usuários em produção, preço, SLA. Os dados deste ambiente são fictícios por
  determinação do projeto.

## Product Principles

1. **A habilitação da pessoa é parte do bloqueio.** Treinamento, vínculo e validade não são
   cadastro de RH — são pré-condição de segurança. Design que esconde ou suaviza um vencimento
   está errado, mesmo que fique mais bonito.
2. **O estado do PLE precisa ser inequívoco.** Em qualquer tela, em qualquer tamanho, quem olha
   tem que saber em que etapa o bloqueio está e o que falta. Ambiguidade aqui é risco físico.
3. **Duas cenas, um produto.** O que é crítico em campo (PLE, avaliação de risco) precisa
   funcionar em tela pequena e com pressa; o que é de escritório (usuários, auditoria,
   relatórios) precisa suportar volume e sessão longa. Nenhuma das duas é o caso secundário.
4. **Papel e tela são o mesmo processo.** O PLE impresso em 2 vias circula no campo. Mudanças na
   tela não podem quebrar a correspondência com o que está no papel.
5. **A trilha é permanente.** Auditoria não expira e não se apaga. Toda ação relevante deixa
   rastro de quem, quando e o que mudou.

## Accessibility & Inclusion

- Uso em campo pressupõe condições industriais: possivelmente de luva, sob luz variável, em
  dispositivo móvel. Alvos de toque e contraste precisam sobreviver a isso.
- Multilíngue PT/EN/ES/IT (ver Brand Commitments) — o produto tem usuários que não leem
  português.
- Nenhum padrão formal de acessibilidade (WCAG nível X) foi estabelecido pelo usuário até aqui.
