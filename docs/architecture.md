# Arquitetura

Romodoro é uma aplicação desktop Avalonia com um projeto executável e um projeto separado de testes. O ponto de entrada está em `src/Romodoro/Program.cs`; `App` monta os serviços, view models e a janela principal.

## Limites principais

- `Clock/` mantém a apresentação de hora e data localizadas.
- `Stopwatch/` contém o estado de tempo transcorrido e as voltas.
- `Pomodoro/` contém as etapas de foco e pausas.
- `Time/` fornece o relógio monotônico e os pulsos da UI.
- `Shell/` coordena seleção de modo, tamanho da janela e estado sempre no topo.
- `Views/` contém a composição Avalonia e os controles visuais.
- `Notifications/` isola notificações, som e fallback visual.
- `Infrastructure/` fornece binding e comandos pequenos para MVVM.

## Fluxo de tempo

Cada estado usa `SystemMonotonicClock` para calcular duração real. `DispatcherTickSource` atualiza a apresentação a cada 250 ms. O estado calcula a diferença entre o instante inicial e o relógio monotônico, evitando deriva quando a UI atrasa.

## Modos

`MainWindowViewModel` mantém os três view models e ativa o pulso do modo selecionado. A janela usa 330 × 410 para Relógio, 340 × 410 para Cronômetro e 400 × 660 para Pomodoro. O tema e as cores estão em `App.axaml`; os SVGs exportados do Figma ficam em `src/Romodoro/Assets/Icons`.

## Pomodoro

`PomodoroState` começa em foco de 25 minutos. Foco concluído leva a uma pausa curta de 5 minutos, exceto a cada quarto foco, quando leva a uma pausa longa de 15 minutos. A etapa seguinte inicia automaticamente. `PomodoroViewModel` dispara `ICompletionNotifier` na transição.

## Notificações

`PlatformCompletionNotifier` escolhe o mecanismo por sistema operacional: `msg` no Windows, `osascript` no macOS e `notify-send` no Linux. Uma falha gera `FallbackRequested`; a progressão do temporizador continua.

## Qualidade e entrega

`Directory.Build.props` habilita os analisadores .NET. `ci.yml` verifica formatação, compila e testa em pull requests e pushes para `main`. `release.yml` publica RIDs desktop quando uma tag semântica `vMAJOR.MINOR.PATCH` é enviada.

_Verified against `main`@`b529247` on 2026-09-18._
