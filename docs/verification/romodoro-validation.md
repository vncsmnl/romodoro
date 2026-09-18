# Romodoro — validação

## Ambiente

- .NET SDK: 9.0.318
- Avalonia: 12.1.2
- Branch: `feature/romodoro-implementation`
- Fonte visual: Figma nodes `2:8`, `2:64`, `2:32`

## Comandos executados

| Comando | Resultado |
| --- | --- |
| `dotnet build Romodoro.sln` | PASS — 3 projetos, 0 erros, 0 warnings |
| `dotnet test Romodoro.sln --no-restore` | PASS — 6 testes |
| `dotnet test Romodoro.sln -c Release --no-restore` | PASS — 6 testes |
| `dotnet run --project src/Romodoro/Romodoro.csproj --no-build` | PASS — processo iniciou e encerrou sem exceção |
| `detect.mjs --json src/Romodoro/Views src/Romodoro/App.axaml` | PASS — lista vazia |

## Publicação

Publicações Release concluídas para `win-x64`, `linux-x64`, `osx-x64` e `osx-arm64`, usando `--self-contained false`.

## Cobertura funcional automatizada

- Relógio: hora/data em português.
- Cronômetro: pausa/retomada e reset/voltas.
- Pomodoro: transição após quatro focos para pausa longa e reset de etapa.
- Notificação: falha nativa não impede áudio nem fallback.

## Observações

A validação de notificações nativas foi exercitada no ambiente Windows disponível; macOS e Linux foram compilados/publicados por RID, mas exigem execução em seus respectivos desktops para confirmar o backend de notificação do sistema.
