using System.Collections.ObjectModel;
using System.Globalization;
using Romodoro.Infrastructure;
using Romodoro.Time;

namespace Romodoro.Stopwatch;

public sealed class StopwatchViewModel : BindableBase, IDisposable
{
    private readonly StopwatchState _state;
    private readonly ITickSource _ticks;
    private string _elapsedText = "00:00:00";
    private bool _isRunning;
    public string ElapsedText { get => _elapsedText; private set => SetProperty(ref _elapsedText, value); }
    public bool IsRunning { get => _isRunning; private set { if (SetProperty(ref _isRunning, value)) ToggleCommand.NotifyCanExecuteChanged(); } }
    public ObservableCollection<string> Laps { get; } = [];
    public RelayCommand ToggleCommand { get; }
    public RelayCommand ResetCommand { get; }
    public RelayCommand LapCommand { get; }
    public StopwatchViewModel(StopwatchState state, ITickSource ticks)
    {
        _state = state; _ticks = ticks; _ticks.Tick += OnTick;
        ToggleCommand = new(Toggle); ResetCommand = new(Reset); LapCommand = new(AddLap);
        Refresh();
    }
    public void Start() { _ticks.Start(); Refresh(); }
    public void Stop() => _ticks.StopTicking();
    private void Toggle() { if (_state.IsRunning) _state.Pause(); else _state.Start(); IsRunning = _state.IsRunning; Refresh(); }
    private void Reset() { _state.Reset(); Laps.Clear(); IsRunning = false; Refresh(); }
    private void AddLap() { _state.AddLap(); Laps.Clear(); foreach (var lap in _state.Laps) Laps.Add(lap.ToString("hh\\:mm\\:ss", CultureInfo.InvariantCulture)); }
    private void OnTick(object? sender, EventArgs e) => Refresh();
    private void Refresh() { ElapsedText = _state.Elapsed.ToString("hh\\:mm\\:ss", CultureInfo.InvariantCulture); IsRunning = _state.IsRunning; }
    public void Dispose() { _ticks.Tick -= OnTick; _ticks.StopTicking(); }
}
