using Romodoro.Infrastructure;
using Romodoro.Notifications;
using Romodoro.Time;

namespace Romodoro.Pomodoro;

public sealed class PomodoroViewModel : BindableBase, IDisposable
{
    private readonly PomodoroState _state;
    private readonly ITickSource _ticks;
    private readonly ICompletionNotifier _notifier;
    private string _timeText = "25:00";
    private string _stageText = "Foco";
    private double _progress = 1;
    private bool _isRunning;
    public string TimeText { get => _timeText; private set => SetProperty(ref _timeText, value); }
    public string StageText { get => _stageText; private set => SetProperty(ref _stageText, value); }
    public double Progress { get => _progress; private set => SetProperty(ref _progress, value); }
    public bool IsRunning { get => _isRunning; private set => SetProperty(ref _isRunning, value); }
    public RelayCommand ToggleCommand { get; }
    public RelayCommand ResetCommand { get; }
    public RelayCommand SkipCommand { get; }
    public PomodoroViewModel(PomodoroState state, ITickSource ticks, ICompletionNotifier notifier)
    {
        _state = state; _ticks = ticks; _notifier = notifier; _ticks.Tick += OnTick;
        ToggleCommand = new(Toggle); ResetCommand = new(Reset); SkipCommand = new(Skip); Refresh();
    }
    public void Start() { _ticks.Start(); Refresh(); }
    public void Stop() => _ticks.StopTicking();
    private void Toggle() { if (_state.IsRunning) _state.Pause(); else _state.Start(); Refresh(); }
    private void Reset() { _state.ResetStage(); Refresh(); }
    private void Skip() { var previous = _state.Stage; if (_state.AdvanceIfComplete()) _ = NotifyAsync(previous); Refresh(); }
    private async void OnTick(object? sender, EventArgs e)
    {
        var previous = _state.Stage;
        if (_state.AdvanceIfComplete()) await NotifyAsync(previous);
        Refresh();
    }
    private Task NotifyAsync(PomodoroStage completed) => _notifier.NotifyAsync(completed, _state.Stage);
    private void Refresh()
    {
        var remaining = _state.Remaining;
        TimeText = $"{Math.Max(0, (int)remaining.TotalMinutes):00}:{Math.Max(0, remaining.Seconds):00}";
        StageText = _state.Stage switch { PomodoroStage.Focus => "Foco", PomodoroStage.ShortBreak => "Pausa curta", _ => "Pausa longa" };
        Progress = _state.Progress; IsRunning = _state.IsRunning;
    }
    public void Dispose() { _ticks.Tick -= OnTick; _ticks.StopTicking(); }
}
