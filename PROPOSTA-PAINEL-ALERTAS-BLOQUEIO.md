# Proposta — Painel de Alertas Visual de Bloqueio/Desbloqueio (LOTOTO)

| | |
|---|---|
| **Cliente** | _(nome do cliente)_ |
| **Projeto** | Painel de alertas visual de bloqueio/desbloqueio de equipamentos — Sistema LOTOTO |
| **Data** | 11/07/2026 |
| **Validade da proposta** | 30 dias |
| **Responsável** | _(seu nome / empresa)_ |

---

## 1. Sumário executivo

O sistema LOTOTO já controla todo o ciclo de **bloqueio e desbloqueio de equipamentos**
(Lockout/Tagout) com total rastreabilidade. Para levar essa informação ao **chão de fábrica**,
propomos um **painel de alertas visual** que mostra os equipamentos usando **os mesmos sinais já
conhecidos do sistema** (o indicador de bloqueio em vermelho, preparação em amarelo e livre),
de forma clara e legível à distância.

O painel é entregue de **duas formas**:

- **Em TVs/monitores nas áreas produtivas** — um mural em tela cheia, atualizado
  automaticamente, para toda a equipe enxergar o status **em tempo real**.
- **Dentro do sistema, em uma nova aba** — a mesma visão, com um recurso extra para o **Comando
  Central**: ao passar o mouse sobre um equipamento, vê **quem são os líderes de bloqueio** e
  **quais requisições** originaram aquele bloqueio.

O resultado é **mais segurança e visibilidade operacional**: qualquer pessoa na área identifica
imediatamente o que está bloqueado, e a Sala de Operações ganha um controle central ágil.

## 2. Entendimento do desafio

- A informação de bloqueio precisa estar **visível no local de trabalho**, não apenas dentro do
  sistema.
- A equipe de campo deve reconhecer o status **num relance**, com a mesma linguagem visual que
  já usa no sistema.
- O Comando Central precisa, rapidamente, **identificar responsáveis e requisições** de cada
  bloqueio para agilizar as liberações.

## 3. Solução proposta

Um painel visual, integrado ao LOTOTO atual, que exibe os equipamentos em **formato de mural**
com o estado de bloqueio destacado por cores/sinais idênticos aos do sistema, contemplando:

- **Mural em tela cheia** para TVs, atualizado automaticamente, com legenda dos estados.
- **Link dedicado** para as telas nas áreas produtivas, em **modo somente leitura** e **sem
  exibir dados sensíveis** (sem nomes ou números de requisição nas TVs).
- **Aba no sistema** com a mesma visão e, para o Comando Central, **detalhe ao passar o mouse**:
  líder(es) de bloqueio e requisição(ões) que originaram o bloqueio, com atalho para abrir a
  requisição.
- **Seleção por planta/área**, respeitando as permissões de cada usuário.

> A solução é construída **sobre o sistema existente**, reaproveitando os dados e a identidade
> visual já em uso — **sem impacto** nos cadastros e operações atuais.

## 4. Escopo

### Incluído
- Mural visual dos equipamentos com **os mesmos sinais/cores** do sistema.
- **Modo TV**: link dedicado, tela cheia, somente leitura, atualização automática, sem dados sensíveis.
- **Modo sistema (aba)**: mesma visão + detalhe (líderes e requisições) ao passar o mouse, para o Comando Central, com atalho para a requisição.
- Seleção por planta/área e controle de acesso por perfil.
- Textos em **português e inglês**; layout adequado a **telas grandes e tablet**.
- Testes e homologação da funcionalidade.

### Fora de escopo (nesta fase)
- Alarmes sonoros no mural.
- Alertas automáticos por e-mail/notificação (ex.: bloqueio parado há muito tempo).
- Exportação do painel (PDF/Excel).
- Personalização de layout pelo usuário.
- Indicadores gerenciais/gráficos estatísticos.

> Itens fora de escopo podem ser incorporados em fase futura, mediante nova estimativa.

## 5. Entregáveis

1. Painel de alertas (modos TV e aba) publicado em homologação para validação.
2. Ajustes decorrentes da homologação.
3. Publicação em produção.
4. Orientação de uso e de configuração das TVs por área.

## 6. Metodologia e cronograma

Trabalho conduzido em fases, com **validação em homologação antes de produção** (padrão já
adotado no projeto) e acompanhamento contínuo com o cliente.

| Fase | Descrição | Esforço (horas) |
|---|---|---:|
| 1. Alinhamento e prototipação | Definição do modo TV, layout do mural e aprovação | 5 |
| 2. Preparação dos dados | Informações que alimentam o mural e o detalhe de líderes/requisições | 8 |
| 3. Mural visual | Grade em tela cheia, sinais/cores do sistema, legenda e atualização automática | 16 |
| 4. Modo TV | Link dedicado, somente leitura e sem dados sensíveis | 6 |
| 5. Aba no sistema | Detalhe ao passar o mouse (líderes + requisições) e atalho para a requisição | 7 |
| 6. Bilíngue, menu e permissões | Português/inglês, integração ao menu e acessos | 4 |
| 7. Qualidade e homologação | Testes e validação com o cliente | 4 |
| 8. Documentação e entrega | Publicação e material de uso | 2 |
| **Total** | | **52 horas** |

**Prazo estimado:** cerca de **2 a 3 semanas**, conforme a disponibilidade de alocação e a
agilidade nas validações do cliente.

> Estimativa expressa **em horas de desenvolvimento**. Esta proposta não inclui valores
> financeiros; a conversão em investimento seguirá a condição comercial acordada entre as partes.

## 7. Time e governança

- Equipe especializada no sistema LOTOTO, com pleno conhecimento da base atual.
- Ponto focal único para alinhamento e status.
- Validação em homologação antes de qualquer publicação em produção.

## 8. Diferenciais

- **Mesma linguagem visual do sistema** — adoção imediata pela equipe, sem curva de aprendizado.
- **Conhecimento da base atual** — desenvolvimento sobre o sistema existente, sem retrabalho.
- **Processo seguro** — modo TV isolado e sem dados sensíveis; homologação antes de produção.

## 9. Próximos passos

1. Aprovação do escopo e da estimativa (52 horas).
2. Definição do modo de disponibilização do link para as TVs (alinhamento da Fase 1).
3. Início da Fase 1 e evolução até a publicação em produção.

## 10. Condições

- Estimativa de **52 horas de desenvolvimento**; valores financeiros a definir conforme condição
  comercial vigente.
- Escopo conforme seção 4; itens fora de escopo exigem nova estimativa.
- Proposta válida por **30 dias** a partir da data acima.
