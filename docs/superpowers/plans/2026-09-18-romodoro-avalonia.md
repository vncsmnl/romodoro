# Romodoro Avalonia Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a faithful, functional Avalonia desktop application for Windows, macOS, and Linux with Clock, Stopwatch, and standard Pomodoro modes from the supplied Figma frames.

**Architecture:** A single Avalonia desktop project contains focused feature folders for each mode plus shared window and notification services. Time-dependent models consume an injectable monotonic clock, while view models expose small bindable states and commands; the custom window adapts its size to the active mode.

**Tech Stack:** .NET 9, C# 13, Avalonia 12.1.2, Avalonia.Themes.Fluent 12.1.2, DesktopNotifications 1.3.1 platform packages, NetCoreAudio 2.0.1, xUnit, FluentAssertions.

**Spec:** `docs/superpowers/specs/2026-09-18-romodoro-avalonia-design.md`

## Global Constraints

- Support Windows, macOS, and Linux through one `net9.0` Avalonia desktop application.
- Use Figma nodes `2:8`, `2:64`, and `2:32` as the visual source of truth.
- Preserve exact core colors: `#131416`, `#1B1C1F`, `#383B40`, `#F0F0F2`, `#9699A3`, and `#FF6661`.
- Bundle Inter and every Figma SVG locally; no runtime asset may depend on a temporary Figma URL.
- Pomodoro durations are 25 minutes focus, 5 minutes short break, and 15 minutes long break after four focuses.
- Completion must trigger native notification and sound; failures degrade to an in-app accessible banner without stopping timer progression.
- Keep scope free of persistence, configurable durations, cloud synchronization, telemetry, and tray execution.
- Implement keyboard access and accessible names for every interactive control.

---

## Planned File Structure

```text
Romodoro.sln
Directory.Packages.props                 Central package versions
src/Romodoro/
  Romodoro.csproj                        Avalonia desktop application
  Program.cs                             Desktop entry point
  App.axaml                              Fonts, theme, and global resources
  App.axaml.cs                           Composition root
  Assets/Fonts/Inter-*.ttf               Bundled Inter faces
  Assets/Icons/*.svg                     Exact Figma exports
  Assets/Sounds/session-complete.wav     Embedded completion cue
  Infrastructure/BindableBase.cs         Property notification base
  Infrastructure/RelayCommand.cs         Synchronous ICommand
  Time/IMonotonicClock.cs                Injectable elapsed-time source
  Time/SystemMonotonicClock.cs           Stopwatch-backed clock
  Time/ITickSource.cs                    UI update abstraction
  Time/DispatcherTickSource.cs           Avalonia dispatcher timer
  Clock/ClockViewModel.cs                 Local time/date presentation
  Stopwatch/StopwatchState.cs            Pure stopwatch state machine
  Stopwatch/StopwatchViewModel.cs        Commands and lap presentation
  Pomodoro/PomodoroStage.cs               Stage enum
  Pomodoro/PomodoroState.cs               Pure cycle state machine
  Pomodoro/PomodoroViewModel.cs           Commands, progress, completion
  Notifications/ICompletionNotifier.cs    Notification/sound contract
  Notifications/CompletionNotifier.cs     Failure-isolated orchestration
  Notifications/DesktopNotificationSink.cs Native platform notifications
  Notifications/AudioSink.cs              Embedded WAV playback
  Shell/TimerMode.cs                       Mode enum
  Shell/MainWindowViewModel.cs             Mode/window state coordinator
  Views/MainWindow.axaml                   Shared custom window shell
  Views/MainWindow.axaml.cs                Window-only platform operations
  Views/ClockView.axaml                     Clock frame
  Views/StopwatchView.axaml                 Stopwatch frame and laps
  Views/PomodoroView.axaml                  Pomodoro frame and progress ring
  Views/Controls/ModeTabs.axaml             Reusable three-mode navigation
  Views/Controls/ActionButton.axaml         Reusable circular action button
  Views/Controls/AlwaysOnTopFooter.axaml    Reusable footer
tests/Romodoro.Tests/
  Romodoro.Tests.csproj
  Fakes/FakeMonotonicClock.cs
  Clock/ClockViewModelTests.cs
  Stopwatch/StopwatchStateTests.cs
  Pomodoro/PomodoroStateTests.cs
  Notifications/CompletionNotifierTests.cs
  Shell/MainWindowViewModelTests.cs
```

### Task 1: Scaffold the reproducible Avalonia solution

**Files:**
- Create: `Romodoro.sln`
- Create: `Directory.Packages.props`
- Create: `src/Romodoro/Romodoro.csproj`
- Create: `src/Romodoro/Program.cs`
- Create: `src/Romodoro/App.axaml`
- Create: `src/Romodoro/App.axaml.cs`
- Create: `tests/Romodoro.Tests/Romodoro.Tests.csproj`

**Interfaces:**
- Consumes: .NET 9 SDK.
- Produces: buildable `Romodoro.sln`, application assembly `Romodoro`, and test assembly `Romodoro.Tests`.

- [ ] **Step 1: Create solution and projects**

Run:

```powershell
dotnet new install Avalonia.Templates::12.1.2
dotnet new sln -n Romodoro
dotnet new avalonia.app -n Romodoro -o src/Romodoro -f net9.0
dotnet new xunit -n Romodoro.Tests -o tests/Romodoro.Tests -f net9.0
dotnet sln add src/Romodoro/Romodoro.csproj tests/Romodoro.Tests/Romodoro.Tests.csproj
dotnet add tests/Romodoro.Tests/Romodoro.Tests.csproj reference src/Romodoro/Romodoro.csproj
```

- [ ] **Step 2: Pin dependencies centrally**

Create `Directory.Packages.props`:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Avalonia" Version="12.1.2" />
    <PackageVersion Include="Avalonia.Desktop" Version="12.1.2" />
    <PackageVersion Include="Avalonia.Themes.Fluent" Version="12.1.2" />
    <PackageVersion Include="Avalonia.Fonts.Inter" Version="12.1.2" />
    <PackageVersion Include="DesktopNotifications" Version="1.3.1" />
    <PackageVersion Include="DesktopNotifications.Avalonia" Version="1.3.1" />
    <PackageVersion Include="DesktopNotifications.Windows" Version="1.3.1" />
    <PackageVersion Include="DesktopNotifications.Apple" Version="1.3.1" />
    <PackageVersion Include="DesktopNotifications.FreeDesktop" Version="1.3.1" />
    <PackageVersion Include="NetCoreAudio" Version="2.0.1" />
    <PackageVersion Include="FluentAssertions" Version="8.8.0" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageVersion Include="xunit" Version="2.9.3" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="3.1.4" />
  </ItemGroup>
</Project>
```

Remove explicit `Version` attributes from both project files and add the notification/audio packages to the app plus FluentAssertions to tests.

- [ ] **Step 3: Establish global resources**

In `App.axaml`, define `Color` and `SolidColorBrush` resources named `WindowBackgroundColor`, `SurfaceColor`, `BorderColor`, `PrimaryTextColor`, `SecondaryTextColor`, and `AccentColor`, then set the default font family:

```xml
<Application.Resources>
  <Color x:Key="WindowBackgroundColor">#131416</Color>
  <Color x:Key="SurfaceColor">#1B1C1F</Color>
  <Color x:Key="BorderColor">#383B40</Color>
  <Color x:Key="PrimaryTextColor">#F0F0F2</Color>
  <Color x:Key="SecondaryTextColor">#9699A3</Color>
  <Color x:Key="AccentColor">#FF6661</Color>
  <FontFamily x:Key="InterFont">avares://Avalonia.Fonts.Inter/Assets#Inter</FontFamily>
</Application.Resources>
```

- [ ] **Step 4: Verify clean build**

Run: `dotnet restore && dotnet build Romodoro.sln --no-restore`

Expected: build succeeds with zero errors.

- [ ] **Step 5: Commit**

```powershell
git add Romodoro.sln Directory.Packages.props src tests
git commit -m "build: scaffold Avalonia desktop solution"
```

### Task 2: Implement deterministic timing infrastructure

**Files:**
- Create: `src/Romodoro/Infrastructure/BindableBase.cs`
- Create: `src/Romodoro/Infrastructure/RelayCommand.cs`
- Create: `src/Romodoro/Time/IMonotonicClock.cs`
- Create: `src/Romodoro/Time/SystemMonotonicClock.cs`
- Create: `src/Romodoro/Time/ITickSource.cs`
- Create: `src/Romodoro/Time/DispatcherTickSource.cs`
- Create: `tests/Romodoro.Tests/Fakes/FakeMonotonicClock.cs`

**Interfaces:**
- Consumes: `System.Diagnostics.Stopwatch`, Avalonia `DispatcherTimer`.
- Produces: `IMonotonicClock.Elapsed`, `ITickSource.Tick`, `ITickSource.Start()`, `ITickSource.Stop()`, reusable `BindableBase` and `RelayCommand`.

- [ ] **Step 1: Add the fake clock used by later tests**

```csharp
public sealed class FakeMonotonicClock : IMonotonicClock
{
    public TimeSpan Elapsed { get; private set; }
    public void Advance(TimeSpan duration) => Elapsed += duration;
}
```

- [ ] **Step 2: Define the production contracts**

```csharp
public interface IMonotonicClock
{
    TimeSpan Elapsed { get; }
}

public interface ITickSource
{
    event EventHandler? Tick;
    void Start();
    void Stop();
}
```

Implement `SystemMonotonicClock` with one continuously running `Stopwatch`. Implement `DispatcherTickSource` with a 250 ms `DispatcherTimer`.

- [ ] **Step 3: Add binding primitives**

`BindableBase.SetProperty<T>` must compare with `EqualityComparer<T>.Default`, update the field, and raise `PropertyChanged`. `RelayCommand` must implement `ICommand`, accept `Action` and optional `Func<bool>`, and expose `NotifyCanExecuteChanged()`.

- [ ] **Step 4: Build infrastructure**

Run: `dotnet build Romodoro.sln`

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add src/Romodoro/Infrastructure src/Romodoro/Time tests/Romodoro.Tests/Fakes
git commit -m "feat: add deterministic timing infrastructure"
```

### Task 3: Build the clock mode test-first

**Files:**
- Create: `src/Romodoro/Clock/ClockViewModel.cs`
- Create: `tests/Romodoro.Tests/Clock/ClockViewModelTests.cs`

**Interfaces:**
- Consumes: `ITickSource`, `Func<DateTimeOffset> now`, `CultureInfo`.
- Produces: `ClockViewModel.TimeText`, `DateText`, `Start()`, and `Stop()`.

- [ ] **Step 1: Write failing formatting and refresh tests**

```csharp
[Fact]
public void Refresh_formats_time_and_date_in_portuguese()
{
    var ticks = new ManualTickSource();
    var now = new DateTimeOffset(2026, 9, 18, 14, 32, 0, TimeSpan.FromHours(-3));
    var sut = new ClockViewModel(ticks, () => now, CultureInfo.GetCultureInfo("pt-BR"));

    sut.Start();

    sut.TimeText.Should().Be("14:32");
    sut.DateText.Should().Be("sexta-feira, 18 de setembro");
}
```

Include a second test that changes `now`, raises `ManualTickSource.Tick`, and expects the new minute.

- [ ] **Step 2: Run the clock tests and verify RED**

Run: `dotnet test --filter FullyQualifiedName~ClockViewModelTests`

Expected: FAIL because `ClockViewModel` does not exist.

- [ ] **Step 3: Implement the clock view model**

Format with `now().ToLocalTime().ToString("HH:mm", culture)` and `ToString("dddd, dd 'de' MMMM", culture)`. Normalize the first date character to lowercase and refresh immediately in `Start()` before subscribing to ticks.

- [ ] **Step 4: Run clock tests**

Run: `dotnet test --filter FullyQualifiedName~ClockViewModelTests`

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add src/Romodoro/Clock tests/Romodoro.Tests/Clock
git commit -m "feat: add live localized clock"
```

### Task 4: Build the stopwatch state machine test-first

**Files:**
- Create: `src/Romodoro/Stopwatch/StopwatchState.cs`
- Create: `src/Romodoro/Stopwatch/StopwatchViewModel.cs`
- Create: `tests/Romodoro.Tests/Stopwatch/StopwatchStateTests.cs`

**Interfaces:**
- Consumes: `IMonotonicClock`, `ITickSource`.
- Produces: `Start()`, `Pause()`, `Reset()`, `AddLap()`, `Elapsed`, `IsRunning`, `IReadOnlyList<TimeSpan> Laps`; view-model commands and `ElapsedText`.

- [ ] **Step 1: Write failing state tests**

```csharp
[Fact]
public void Pause_and_resume_preserve_accumulated_time()
{
    var clock = new FakeMonotonicClock();
    var sut = new StopwatchState(clock);
    sut.Start();
    clock.Advance(TimeSpan.FromSeconds(5));
    sut.Pause();
    clock.Advance(TimeSpan.FromSeconds(20));
    sut.Start();
    clock.Advance(TimeSpan.FromSeconds(3));

    sut.Elapsed.Should().Be(TimeSpan.FromSeconds(8));
}

[Fact]
public void Reset_clears_elapsed_time_and_laps()
{
    var clock = new FakeMonotonicClock();
    var sut = new StopwatchState(clock);
    sut.Start();
    clock.Advance(TimeSpan.FromSeconds(8));
    sut.AddLap();
    sut.Reset();

    sut.Elapsed.Should().Be(TimeSpan.Zero);
    sut.Laps.Should().BeEmpty();
    sut.IsRunning.Should().BeFalse();
}
```

- [ ] **Step 2: Run stopwatch tests and verify RED**

Run: `dotnet test --filter FullyQualifiedName~StopwatchStateTests`

Expected: FAIL because `StopwatchState` does not exist.

- [ ] **Step 3: Implement state and view model**

Store `_startedAt` and `_accumulated`. While running, return `_accumulated + clock.Elapsed - _startedAt`. Format as `hh\:mm\:ss`. Expose reset, start/pause, and lap commands; update text on every tick and replace the observable lap list only when a lap changes.

- [ ] **Step 4: Run stopwatch tests**

Run: `dotnet test --filter FullyQualifiedName~StopwatchStateTests`

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add src/Romodoro/Stopwatch tests/Romodoro.Tests/Stopwatch
git commit -m "feat: add stopwatch and lap tracking"
```

### Task 5: Build the standard Pomodoro cycle test-first

**Files:**
- Create: `src/Romodoro/Pomodoro/PomodoroStage.cs`
- Create: `src/Romodoro/Pomodoro/PomodoroState.cs`
- Create: `src/Romodoro/Pomodoro/PomodoroViewModel.cs`
- Create: `src/Romodoro/Notifications/ICompletionNotifier.cs`
- Create: `tests/Romodoro.Tests/Pomodoro/PomodoroStateTests.cs`

**Interfaces:**
- Consumes: `IMonotonicClock`, `ITickSource`.
- Produces: `ICompletionNotifier.NotifyAsync(PomodoroStage, PomodoroStage, CancellationToken)`, `Stage`, `Remaining`, `Progress`, `CompletedFocusCount`, `IsRunning`, `Start()`, `Pause()`, `ResetStage()`, `AdvanceIfComplete()`, `Skip()`.

- [ ] **Step 1: Write failing cycle tests**

```csharp
[Fact]
public void Fourth_focus_is_followed_by_long_break()
{
    var clock = new FakeMonotonicClock();
    var sut = new PomodoroState(clock);

    for (var focus = 1; focus <= 4; focus++)
    {
        sut.Start();
        clock.Advance(TimeSpan.FromMinutes(25));
        sut.AdvanceIfComplete().Should().BeTrue();
        sut.Stage.Should().Be(focus == 4 ? PomodoroStage.LongBreak : PomodoroStage.ShortBreak);
        sut.Skip();
    }

    sut.CompletedFocusCount.Should().Be(4);
}

[Fact]
public void Reset_restarts_current_stage_without_erasing_cycle_count()
{
    var clock = new FakeMonotonicClock();
    var sut = new PomodoroState(clock);
    sut.Start();
    clock.Advance(TimeSpan.FromMinutes(25));
    sut.AdvanceIfComplete();
    sut.Start();
    clock.Advance(TimeSpan.FromMinutes(2));
    sut.ResetStage();

    sut.Remaining.Should().Be(TimeSpan.FromMinutes(5));
    sut.CompletedFocusCount.Should().Be(1);
}
```

Add tests for pause/resume, progress clamping between 0 and 1, short-break transition, and automatic start of the next stage.

- [ ] **Step 2: Run Pomodoro tests and verify RED**

Run: `dotnet test --filter FullyQualifiedName~PomodoroStateTests`

Expected: FAIL because Pomodoro types do not exist.

- [ ] **Step 3: Implement the state machine**

Use a duration switch:

```csharp
private static TimeSpan DurationOf(PomodoroStage stage) => stage switch
{
    PomodoroStage.Focus => TimeSpan.FromMinutes(25),
    PomodoroStage.ShortBreak => TimeSpan.FromMinutes(5),
    PomodoroStage.LongBreak => TimeSpan.FromMinutes(15),
    _ => throw new ArgumentOutOfRangeException(nameof(stage))
};
```

When a focus completes, increment the count and choose long break for counts divisible by four. When a break completes, return to focus. Every automatic completion sets the next stage running from the current monotonic timestamp.

- [ ] **Step 4: Implement Pomodoro presentation**

Create the notification contract:

```csharp
public interface ICompletionNotifier
{
    Task NotifyAsync(
        PomodoroStage completed,
        PomodoroStage next,
        CancellationToken cancellationToken = default);
}
```

Expose `TimeText` as `mm\:ss`, stage labels `Foco`, `Pausa curta`, and `Pausa longa`, plus `Progress = 1 - Remaining / Duration`. On a tick that completes a stage, call `ICompletionNotifier.NotifyAsync(previousStage, nextStage)` once and continue even if it throws.

- [ ] **Step 5: Run Pomodoro tests**

Run: `dotnet test --filter FullyQualifiedName~PomodoroStateTests`

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add src/Romodoro/Pomodoro src/Romodoro/Notifications/ICompletionNotifier.cs tests/Romodoro.Tests/Pomodoro
git commit -m "feat: add standard Pomodoro cycle"
```

### Task 6: Add failure-isolated native notifications and sound

**Files:**
- Modify: `src/Romodoro/Notifications/ICompletionNotifier.cs`
- Create: `src/Romodoro/Notifications/INotificationSink.cs`
- Create: `src/Romodoro/Notifications/IAudioSink.cs`
- Create: `src/Romodoro/Notifications/CompletionNotifier.cs`
- Create: `src/Romodoro/Notifications/DesktopNotificationSink.cs`
- Create: `src/Romodoro/Notifications/AudioSink.cs`
- Create: `src/Romodoro/Assets/Sounds/session-complete.wav`
- Create: `tests/Romodoro.Tests/Notifications/CompletionNotifierTests.cs`

**Interfaces:**
- Consumes: DesktopNotifications platform managers and `NetCoreAudio.Player`.
- Produces: `Task NotifyAsync(PomodoroStage completed, PomodoroStage next, CancellationToken cancellationToken = default)` and bindable fallback event `FallbackRequested`.

- [ ] **Step 1: Write failing isolation tests**

```csharp
[Fact]
public async Task Notification_failure_does_not_prevent_audio_or_fallback()
{
    var notifications = new ThrowingNotificationSink();
    var audio = new RecordingAudioSink();
    var sut = new CompletionNotifier(notifications, audio);
    string? fallback = null;
    sut.FallbackRequested += (_, message) => fallback = message;

    await sut.NotifyAsync(PomodoroStage.Focus, PomodoroStage.ShortBreak);

    audio.PlayCount.Should().Be(1);
    fallback.Should().Be("Foco concluído. Hora da pausa curta.");
}
```

Add the mirror case where audio throws but native notification is still attempted.

- [ ] **Step 2: Run notification tests and verify RED**

Run: `dotnet test --filter FullyQualifiedName~CompletionNotifierTests`

Expected: FAIL because notifier types do not exist.

- [ ] **Step 3: Implement orchestration and native sink**

`CompletionNotifier` must attempt notification and audio independently, collect failures, and raise `FallbackRequested` when either fails. `DesktopNotificationSink` selects the Windows, Apple, or FreeDesktop manager using `OperatingSystem` checks and sends title `Romodoro` with the stage-specific Portuguese message.

- [ ] **Step 4: Implement audio playback**

Copy the embedded WAV to `Path.Combine(Path.GetTempPath(), "Romodoro", "session-complete.wav")` once, then call `Player.Play(path)`. Mark the project asset as `AvaloniaResource` and ensure disposal does not cancel an in-flight cue.

- [ ] **Step 5: Run notification tests**

Run: `dotnet test --filter FullyQualifiedName~CompletionNotifierTests`

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add src/Romodoro/Notifications src/Romodoro/Assets/Sounds tests/Romodoro.Tests/Notifications
git commit -m "feat: add completion notifications and sound"
```

### Task 7: Build shell state and exact reusable visual controls

**Files:**
- Create: `src/Romodoro/Shell/TimerMode.cs`
- Create: `src/Romodoro/Shell/MainWindowViewModel.cs`
- Create: `tests/Romodoro.Tests/Shell/MainWindowViewModelTests.cs`
- Create: `src/Romodoro/Views/Controls/ModeTabs.axaml`
- Create: `src/Romodoro/Views/Controls/ModeTabs.axaml.cs`
- Create: `src/Romodoro/Views/Controls/ActionButton.axaml`
- Create: `src/Romodoro/Views/Controls/ActionButton.axaml.cs`
- Create: `src/Romodoro/Views/Controls/AlwaysOnTopFooter.axaml`
- Create: `src/Romodoro/Views/Controls/AlwaysOnTopFooter.axaml.cs`
- Create: `src/Romodoro/Assets/Icons/*.svg`

**Interfaces:**
- Consumes: the three feature view models.
- Produces: `SelectedMode`, `WindowWidth`, `WindowHeight`, `IsTopmost`, `FallbackMessage`, commands `SelectClock`, `SelectStopwatch`, `SelectPomodoro`, and reusable styled controls.

- [ ] **Step 1: Write failing shell-state tests**

```csharp
[Theory]
[InlineData(TimerMode.Clock, 330, 410)]
[InlineData(TimerMode.Stopwatch, 340, 410)]
[InlineData(TimerMode.Pomodoro, 400, 660)]
public void Selecting_mode_sets_figma_window_size(TimerMode mode, double width, double height)
{
    var sut = ShellFactory.Create();
    sut.SelectMode(mode);
    sut.WindowWidth.Should().Be(width);
    sut.WindowHeight.Should().Be(height);
}
```

Add tests that selection starts only the active view-model tick source and that a notifier fallback message becomes visible until `DismissFallbackCommand` executes.

- [ ] **Step 2: Run shell tests and verify RED**

Run: `dotnet test --filter FullyQualifiedName~MainWindowViewModelTests`

Expected: FAIL because shell types do not exist.

- [ ] **Step 3: Implement shell state**

Default to `TimerMode.Clock`. Map sizes exactly through a switch expression. Preserve stopwatch and Pomodoro state while switching views; only presentation ticks are stopped for inactive modes.

- [ ] **Step 4: Download and verify Figma assets**

Use the Figma asset downloader for nodes `2:8`, `2:64`, and `2:32`. Save returned vector bytes under `Assets/Icons` with semantic names and deduplicate byte-identical files. Verify each file begins with an SVG root and that no source URL remains in XAML or C#.

- [ ] **Step 5: Implement reusable controls**

`ModeTabs` uses a three-column `Grid`, fixed 24 px icon presenters, 12 px labels, and a 2 px active underline. `ActionButton` exposes `Icon`, `Command`, `Diameter`, `IsPrimary`, and `AutomationProperties.Name`. `AlwaysOnTopFooter` exposes a two-way `IsChecked` property and the exact Portuguese label.

- [ ] **Step 6: Run tests and compile XAML**

Run: `dotnet test && dotnet build src/Romodoro/Romodoro.csproj`

Expected: PASS with no XAML compiler errors.

- [ ] **Step 7: Commit**

```powershell
git add src/Romodoro/Shell src/Romodoro/Views/Controls src/Romodoro/Assets/Icons tests/Romodoro.Tests/Shell
git commit -m "feat: add timer shell and shared controls"
```

### Task 8: Implement the three Figma views and custom window

**Files:**
- Create: `src/Romodoro/Views/ClockView.axaml`
- Create: `src/Romodoro/Views/ClockView.axaml.cs`
- Create: `src/Romodoro/Views/StopwatchView.axaml`
- Create: `src/Romodoro/Views/StopwatchView.axaml.cs`
- Create: `src/Romodoro/Views/PomodoroView.axaml`
- Create: `src/Romodoro/Views/PomodoroView.axaml.cs`
- Modify: `src/Romodoro/Views/MainWindow.axaml`
- Modify: `src/Romodoro/Views/MainWindow.axaml.cs`
- Modify: `src/Romodoro/App.axaml.cs`

**Interfaces:**
- Consumes: all view models and controls from Tasks 3–7.
- Produces: runnable, interactive desktop UI matching all three Figma nodes.

- [ ] **Step 1: Load the UI craft requirements**

Read `C:/Users/vinic/.agents/skills/impeccable/reference/craft-floor.md` immediately before editing UI. Treat the Figma frames as established visual authority; do not invent a replacement direction.

- [ ] **Step 2: Implement the clock and stopwatch views**

Use shared `ModeTabs` at 15 px inset and 78 px height. Clock centers `TimeText` at 48 px with `DateText` at 13 px. Stopwatch centers `ElapsedText` at 48 px, `Em andamento` at 15 px, then places reset, pause/play, and lap action buttons at 54/58/54 px. The lap list is collapsed when empty and expands below actions without changing the initial 340 × 410 frame.

- [ ] **Step 3: Implement the Pomodoro progress ring and controls**

Create a 250 × 250 ring with a neutral 8 px track and coral progress arc driven by `Progress`. Center the 56 px countdown and 15 px stage label. Place reset, play/pause, and skip buttons at the Figma coordinates relative to the view. Do not create SVG path approximations for icons; use the downloaded exports.

- [ ] **Step 4: Implement the adaptive custom window**

Set `WindowDecorations="None"`, `TransparencyLevelHint="Transparent"`, `Background="Transparent"`, `CanResize="False"`, and bind width/height to shell state. The root `Border` uses radius 24, 1 px border, and `BoxShadow="0 18 21 0 #75000000"`. Mark the non-interactive header region with `WindowDecorationProperties.ElementRole="TitleBar"`; mark minimize and close buttons with their corresponding roles.

- [ ] **Step 5: Wire composition root**

In `App.axaml.cs`, instantiate one monotonic clock, separate tick sources, feature states/view models, notification services, and `MainWindowViewModel`; assign it to `MainWindow.DataContext`. Subscribe window `Topmost`, width, and height to bound state and dispose tick/audio services on desktop exit.

- [ ] **Step 6: Add accessible interaction checks**

Verify every button has `AutomationProperties.Name`, all tab items are keyboard focusable, Space/Enter invoke commands, focus indicators remain visible, and timer ticks do not change an `AutomationProperties.LiveSetting`; only completion banner uses polite live announcement.

- [ ] **Step 7: Build and smoke-run**

Run: `dotnet build Romodoro.sln && dotnet run --project src/Romodoro/Romodoro.csproj`

Expected: window opens in Clock mode, all three modes switch, custom minimize/close work, and the topmost toggle updates immediately.

- [ ] **Step 8: Commit**

```powershell
git add src/Romodoro/Views src/Romodoro/App.axaml.cs
git commit -m "feat: implement Figma timer interface"
```

### Task 9: Validate fidelity, behavior, and platform packaging

**Files:**
- Modify: UI or service files identified by validation.
- Create: `docs/verification/romodoro-validation.md`

**Interfaces:**
- Consumes: complete application and approved Figma frames.
- Produces: tested application, visual evidence, and recorded platform limitations.

- [ ] **Step 1: Run the complete automated suite**

Run:

```powershell
dotnet test Romodoro.sln --configuration Release
dotnet build Romodoro.sln --configuration Release --no-restore
```

Expected: all tests pass and build reports zero errors.

- [ ] **Step 2: Run the Impeccable mechanical detector once**

Run:

```powershell
node C:\Users\vinic\.agents\skills\impeccable\scripts\detect.mjs --json src/Romodoro/Views src/Romodoro/App.axaml
```

Resolve only findings that apply to native Avalonia UI and preserve the approved Figma design.

- [ ] **Step 3: Capture all three states**

Run the app and capture PNG screenshots at the exact frame sizes: Clock 330 × 410, Stopwatch 340 × 410, and Pomodoro 400 × 660. Capture a second pass at 150% Windows display scale. Store evidence outside source control unless the repository establishes a screenshot convention.

- [ ] **Step 4: Perform one batched fidelity correction**

Compare captures against Figma for bounding size, 15 px shell inset, 78 px tabs, text baselines, 24 px icons, 54/58 px actions, ring geometry, footer divider, colors, radius, and clipping. Correct every material mismatch in one patch, rebuild once, and recapture once.

- [ ] **Step 5: Exercise behavior manually**

Verify keyboard-only navigation, minimize/close, drag region, always-on-top, stopwatch pause/resume/lap/reset, Pomodoro pause/reset/skip, notification fallback, and sound. Use injected short test durations in a Debug-only composition path to verify automatic 4-focus sequence without waiting 115 minutes; production constants remain unchanged.

- [ ] **Step 6: Publish platform outputs**

Run:

```powershell
dotnet publish src/Romodoro/Romodoro.csproj -c Release -r win-x64 --self-contained false
dotnet publish src/Romodoro/Romodoro.csproj -c Release -r osx-x64 --self-contained false
dotnet publish src/Romodoro/Romodoro.csproj -c Release -r osx-arm64 --self-contained false
dotnet publish src/Romodoro/Romodoro.csproj -c Release -r linux-x64 --self-contained false
```

Expected: four publish directories are produced without compilation errors. Runtime notification behavior on macOS and Linux must be recorded as requiring confirmation on those operating systems if they are unavailable locally.

- [ ] **Step 7: Record validation results**

Create `docs/verification/romodoro-validation.md` containing exact commands, test totals, capture sizes, detector result, notification results per available OS, and any limitation that could not be exercised on Windows.

- [ ] **Step 8: Commit**

```powershell
git add src tests docs/verification/romodoro-validation.md
git commit -m "test: verify Romodoro desktop experience"
```

### Task 10: Final review and handoff

**Files:**
- Modify: files required by review findings only.
- Modify: `DESIGN.md`

**Interfaces:**
- Consumes: approved spec, implementation, test output, and screenshots.
- Produces: reviewer disposition, documented design system, and final clean worktree.

- [ ] **Step 1: Run a fresh implementation review**

Use the Impeccable finish-reviewer with the three Figma screenshots, corresponding application screenshots, spec, direction contract, and changed file list. A `fix` disposition receives one grouped correction round; `rebuild` replaces the named region before a fresh full review.

- [ ] **Step 2: Re-run affected verification**

Run the targeted tests for corrected code, then `dotnet test Romodoro.sln --configuration Release` and recapture only the affected modes.

- [ ] **Step 3: Document the shipped visual system**

Use the Impeccable documenter after the final correction to write `DESIGN.md` from the actual implementation, including tokens, typography, mode dimensions, reusable controls, responsive/DPI rules, and platform fallbacks.

- [ ] **Step 4: Confirm repository state**

Run:

```powershell
git status --short
git log --oneline -12
```

Expected: no uncommitted implementation files and a readable sequence of focused commits.

- [ ] **Step 5: Commit final documentation when changed**

```powershell
git add DESIGN.md src tests docs
git commit -m "docs: record Romodoro design system"
```
