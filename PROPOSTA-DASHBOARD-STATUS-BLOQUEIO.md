# Proposta — Painel de Status de Bloqueio e Desbloqueio (LOTOTO)

| | |
|---|---|
| **Cliente** | _(nome do cliente)_ |
| **Projeto** | Painel visual de status de bloqueio/desbloqueio de equipamentos — Sistema LOTOTO |
| **Data** | 11/07/2026 |
| **Validade da proposta** | 30 dias |
| **Responsável** | _(seu nome / empresa)_ |

---

## 1. Sumário executivo

O sistema LOTOTO já controla todo o ciclo de **bloqueio e desbloqueio de equipamentos**
(Lockout/Tagout), garantindo segurança e rastreabilidade das operações. Hoje, porém, essa
informação está distribuída entre diferentes telas de consulta, o que dificulta uma **leitura
rápida e gerencial** da situação em campo.

Propomos a criação de um **painel visual único (Dashboard)** que mostra, em tempo real e em uma
só tela, quantos equipamentos estão bloqueados, quais aguardam a liberação do Comando Central,
quais foram concluídos e o histórico recente de ações — por planta.

O resultado é **mais agilidade na tomada de decisão**, **menos tempo procurando informação** e
uma **visão de controle imediata** para a Sala de Operações e a gestão.

## 2. Entendimento do desafio

- A operação precisa saber, **a qualquer momento**, o que está bloqueado e o que está pronto
  para ser liberado — sem navegar por várias telas.
- Bloqueios que ficam **parados aguardando finalização** podem passar despercebidos.
- A gestão não tem hoje uma **visão consolidada por planta** para acompanhar o volume e o
  andamento das operações de LOTOTO.

## 3. Solução proposta

Um painel visual, integrado ao sistema LOTOTO atual, com:

- **Indicadores de destaque** — total de equipamentos bloqueados, em processo de desbloqueio,
  em preparação e finalizados no período.
- **Gráficos** — distribuição por situação, volume por planta e evolução ao longo do tempo.
- **Lista em tempo real** dos equipamentos atualmente bloqueados, com responsável e **tempo
  decorrido**, destacando os que estão parados há mais tempo.
- **Histórico recente** das ações (bloqueio, início de desbloqueio, finalização, cancelamento).
- **Filtros por planta e período** e **atualização automática** da tela.

> A solução é construída **sobre o sistema existente**, aproveitando os dados já registrados —
> **sem retrabalho** e **sem impacto** nos cadastros e operações atuais.

## 4. Escopo

### Incluído
- Nova tela de Dashboard acessível pelo menu do sistema.
- Indicadores, gráficos, lista em tempo real e histórico recente descritos na seção 3.
- Filtros por planta e período, respeitando as permissões de cada usuário.
- Atualização automática e adaptação para uso em **desktop e tablet**.
- Textos em **português e inglês**.
- Testes e homologação da funcionalidade.

### Fora de escopo (nesta fase)
- Exportação do painel em PDF/Excel.
- Alertas/notificações automáticas (ex.: aviso de bloqueio parado).
- Personalização de widgets pelo próprio usuário.
- Indicadores de outros módulos (Avaliação de Risco, Treinamento) no mesmo painel.

> Itens fora de escopo podem ser incorporados em uma fase futura, mediante nova estimativa.

## 5. Entregáveis

1. Painel de status de bloqueio/desbloqueio publicado no ambiente de homologação para validação.
2. Ajustes decorrentes da homologação.
3. Publicação em produção.
4. Documentação de uso resumida.

## 6. Metodologia e cronograma

Trabalho conduzido em fases, com validação em **homologação antes de produção** (padrão já
adotado no projeto) e acompanhamento contínuo com o cliente.

| Fase | Descrição | Esforço (horas) |
|---|---|---:|
| 1. Alinhamento e prototipação | Refinamento do painel e aprovação do layout | 4 |
| 2. Camada de dados e indicadores | Preparação dos dados que alimentam o painel | 10 |
| 3. Construção do painel | Tela, gráficos, lista em tempo real, filtros e atualização | 30 |
| 4. Qualidade e homologação | Testes e validação com o cliente | 6 |
| 5. Documentação e entrega | Publicação e material de uso | 2 |
| **Subtotal** | | **52** |
| Contingência (~15%) | Margem para ajustes de homologação | 8 |
| **Total estimado** | | **≈ 60 horas** |

**Prazo estimado:** cerca de **2 a 3 semanas**, conforme a disponibilidade de alocação e a
agilidade nas validações do cliente.

### Opções de escopo

| Opção | O que entrega | Esforço estimado |
|---|---|---:|
| **Essencial (MVP)** | Indicadores + lista de bloqueados em tempo real + 1 gráfico + filtro por planta | **24 a 30 horas** |
| **Completo** | Todo o escopo desta proposta (gráficos, evolução no tempo, histórico e atualização automática) | **52 a 60 horas** |

> **Estimativas em horas de desenvolvimento.** Esta proposta não inclui valores financeiros;
> a conversão em investimento seguirá a condição comercial acordada entre as partes.

## 7. Time e governança

- Equipe especializada no sistema LOTOTO, com pleno conhecimento da base atual.
- Ponto focal único para alinhamento e status.
- Validação em homologação antes de qualquer publicação em produção.

## 8. Diferenciais

- **Conhecimento do sistema existente** — desenvolvimento sobre a base atual, sem retrabalho.
- **Entrega incremental** — opção de começar pelo Essencial e evoluir para o Completo.
- **Processo seguro** — homologação obrigatória antes de produção.

## 9. Próximos passos

1. Aprovação do escopo (Essencial ou Completo).
2. Alinhamento do cronograma e início da Fase 1.
3. Validações em homologação e publicação em produção.

## 10. Condições

- Estimativa expressa **em horas de desenvolvimento**; valores financeiros a definir conforme
  condição comercial vigente.
- Escopo conforme seção 4; itens fora de escopo exigem nova estimativa.
- Proposta válida por **30 dias** a partir da data acima.
