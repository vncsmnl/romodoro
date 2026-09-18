# Começar a usar

## Pré-requisitos

Instale o .NET SDK 9.0 ou posterior. O projeto declara `net9.0` nos projetos da aplicação e dos testes.

Confirme a instalação:

```bash
dotnet --version
```

O ambiente usado para verificar esta página retorna `9.0.318`.

## Restaurar dependências

Na raiz do repositório:

```bash
dotnet restore Romodoro.sln
```

As versões centrais ficam em `Directory.Packages.props`.

## Executar o aplicativo

```bash
dotnet run --project src/Romodoro/Romodoro.csproj
```

O aplicativo abre no modo Relógio. Use as abas para alternar para Cronômetro ou Pomodoro.

## Executar testes

```bash
dotnet test Romodoro.sln -c Release
```

Os testes cobrem relógio, cronômetro, voltas, transições Pomodoro e fallback de notificações.

## Verificar qualidade

Os comandos do CI podem ser executados localmente:

```bash
dotnet format whitespace Romodoro.sln --verify-no-changes
dotnet build Romodoro.sln -c Release
dotnet test Romodoro.sln -c Release --no-build
```

_Verified against `main`@`b529247` on 2026-09-18._
