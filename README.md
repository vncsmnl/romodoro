# Romodoro

<!-- wikikit:front-door:start -->

Romodoro é um relógio, cronômetro e Pomodoro desktop construído com Avalonia para Windows, macOS e Linux.

## Quickstart

Requer .NET SDK 10.0 ou posterior.

```bash
dotnet restore Romodoro.sln
dotnet run --project src/Romodoro/Romodoro.csproj
```

Execute os testes com:

```bash
dotnet test Romodoro.sln -c Release
```

Leia a [documentação](docs/index.md) para o guia completo, arquitetura, uso dos modos e releases.

<!-- wikikit:front-door:end -->

## CI/CD

GitHub Actions verifica formatação, analisadores, build e testes. Tags `vMAJOR.MINOR.PATCH` geram pacotes desktop e uma GitHub Release com notas extraídas de `CHANGELOG.md`.

## Licença

Nenhum arquivo de licença foi encontrado neste repositório.

_Verified against `main`@`b529247` on 2026-09-18._
