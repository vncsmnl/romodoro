using Avalonia.Controls;
using Romodoro.Clock;
using Romodoro.Infrastructure;
using Romodoro.Pomodoro;
using Romodoro.Stopwatch;

namespace Romodoro.Shell;

public sealed class MainWindowViewModel : BindableBase, IDisposable
{
    private readonly ClockViewModel _clock;
    private readonly StopwatchViewModel _stopwatch;
    private readonly PomodoroViewModel _pomodoro;
    private TimerMode _selectedMode = TimerMode.Clock;
    private bool _isTopmost;
    private string _fallbackMessage = string.Empty;
    public TimerMode SelectedMode { get => _selectedMode; private set => SetProperty(ref _selectedMode, value); }
    public bool IsTopmost { get => _isTopmost; set => SetProperty(ref _isTopmost, value); }
    public string FallbackMessage { get => _fallbackMessage; private set => SetProperty(ref _fallbackMessage, value); }
    public double WindowWidth => SelectedMode == TimerMode.Pomodoro ? 400 : SelectedMode == TimerMode.Stopwatch ? 340 : 330;
    public double WindowHeight => SelectedMode == TimerMode.Pomodoro ? 660 : 410;
    public object CurrentViewModel => SelectedMode switch { TimerMode.Clock => _clock, TimerMode.Stopwatch => _stopwatch, _ => _pomodoro };
    public RelayCommand SelectClockCommand { get; }
    public RelayCommand SelectStopwatchCommand { get; }
    public RelayCommand SelectPomodoroCommand { get; }
    public RelayCommand DismissFallbackCommand { get; }
    public MainWindowViewModel(ClockViewModel clock, StopwatchViewModel stopwatch, PomodoroViewModel pomodoro)
    {
        _clock = clock; _stopwatch = stopwatch; _pomodoro = pomodoro;
        SelectClockCommand = new(() => SelectMode(TimerMode.Clock));
        SelectStopwatchCommand = new(() => SelectMode(TimerMode.Stopwatch));
        SelectPomodoroCommand = new(() => SelectMode(TimerMode.Pomodoro));
        DismissFallbackCommand = new(() => FallbackMessage = string.Empty);
        SelectMode(TimerMode.Clock);
    }
    public void SelectMode(TimerMode mode)
    {
        _clock.Stop(); _stopwatch.Stop(); _pomodoro.Stop();
        SelectedMode = mode;
        Raise(nameof(WindowWidth)); Raise(nameof(WindowHeight)); Raise(nameof(CurrentViewModel));
        if (mode == TimerMode.Clock) _clock.Start();
        if (mode == TimerMode.Stopwatch) _stopwatch.Start();
        if (mode == TimerMode.Pomodoro) _pomodoro.Start();
    }
    public void ShowFallback(string message) => FallbackMessage = message;
    public void Dispose() { _clock.Dispose(); _stopwatch.Dispose(); _pomodoro.Dispose(); }
}
