using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Romodoro.Clock;
using Romodoro.Notifications;
using Romodoro.Pomodoro;
using Romodoro.Shell;
using Romodoro.Stopwatch;
using Romodoro.Time;

namespace Romodoro;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var clock = new SystemMonotonicClock();
            var notifier = new PlatformCompletionNotifier();
            var clockVm = new ClockViewModel(new DispatcherTickSource());
            var stopwatchVm = new StopwatchViewModel(new StopwatchState(clock), new DispatcherTickSource());
            var pomodoroVm = new PomodoroViewModel(new PomodoroState(clock), new DispatcherTickSource(), notifier);
            var shell = new MainWindowViewModel(clockVm, stopwatchVm, pomodoroVm);
            var window = new MainWindow { DataContext = shell, Width = shell.WindowWidth, Height = shell.WindowHeight };
            notifier.FallbackRequested += (_, message) => shell.ShowFallback(message);
            desktop.MainWindow = window;
            desktop.Exit += (_, _) => shell.Dispose();
        }
        base.OnFrameworkInitializationCompleted();
    }
}
