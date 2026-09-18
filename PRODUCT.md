# Romodoro

<!-- impeccable:product-schema 1 -->

## Platform

adaptive

## Stack

Avalonia UI com .NET e C#, voltado a desktop Windows, macOS e Linux.

## Users

Pessoas que desejam consultar a hora, medir atividades e organizar sessões de foco em uma janela compacta que possa permanecer sobre outros aplicativos.

## Product Purpose

Oferecer relógio, cronômetro e temporizador Pomodoro em uma única aplicação desktop leve. O produto é bem-sucedido quando os três modos podem ser usados continuamente sem tirar o usuário do fluxo de trabalho.

## Operating Context

O aplicativo funciona como uma janela desktop flutuante. O usuário alterna entre os três modos, controla temporizadores e pode manter a janela sempre visível sobre outros aplicativos.

## Capabilities and Constraints

- Relógio e data atualizados em tempo real.
- Cronômetro com início, pausa, reinício e registro de voltas.
- Pomodoro com ciclos de 25 minutos de foco, 5 minutos de pausa curta e 15 minutos de pausa longa após quatro sessões de foco.
- Troca automática da etapa do Pomodoro.
- Notificação nativa e som ao término de cada etapa, com fallback quando um recurso do sistema não estiver disponível.
- Alternância de janela sempre no topo.
- Compatibilidade com Windows, macOS e Linux por meio do Avalonia UI.

## Brand Commitments

O arquivo Figma “Tempo — Relógio, Cronômetro e Pomodoro” é a fonte de verdade visual. A interface deve preservar sua composição compacta, tema escuro, destaque coral, tipografia Inter e textos em português.

## Evidence on Hand

Frames do Figma fornecidos para os três modos:

- Relógio: nó `2:8`.
- Pomodoro: nó `2:32`.
- Cronômetro: nó `2:64`.

Não há código, identidade adicional ou conteúdo anterior no diretório do projeto.

## Product Principles

- Permanecer compacto e legível enquanto outros aplicativos estão em uso.
- Dar acesso imediato às ações essenciais de cada modo.
- Preservar o estado do modo durante interações normais com a janela.
- Comportar-se de forma consistente nos três sistemas operacionais.
- Usar notificações e som para informar conclusões sem exigir atenção constante.

## Accessibility & Inclusion

Os controles devem ser acessíveis por teclado, possuir nomes acessíveis e manter contraste compatível com a linguagem visual aprovada.
