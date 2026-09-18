using Romodoro.Time;

namespace Romodoro.Pomodoro;

public sealed class PomodoroState
{
    private readonly IMonotonicClock _clock;
    private TimeSpan _startedAt;
    private bool _running;
    private TimeSpan _remaining = TimeSpan.FromMinutes(25);
    public PomodoroStage Stage { get; private set; } = PomodoroStage.Focus;
    public int CompletedFocusCount { get; private set; }
    public bool IsRunning => _running;
    public TimeSpan Remaining => _running ? _remaining - (_clock.Elapsed - _startedAt) : _remaining;
    public TimeSpan Duration => DurationOf(Stage);
    public double Progress => Math.Clamp(Remaining.TotalSeconds / Duration.TotalSeconds, 0, 1);
    public PomodoroState(IMonotonicClock clock) => _clock = clock;
    public void Start() { if (!_running) { _startedAt = _clock.Elapsed; _running = true; } }
    public void Pause() { if (_running) { _remaining = Remaining; _running = false; } }
    public void ResetStage() { _remaining = Duration; _running = false; }
    public void Skip() { _remaining = TimeSpan.Zero; AdvanceIfComplete(); }
    public bool AdvanceIfComplete()
    {
        if (Remaining > TimeSpan.Zero) return false;
        var completed = Stage;
        if (completed == PomodoroStage.Focus)
        {
            CompletedFocusCount++;
            Stage = CompletedFocusCount % 4 == 0 ? PomodoroStage.LongBreak : PomodoroStage.ShortBreak;
        }
        else Stage = PomodoroStage.Focus;
        _remaining = Duration;
        _startedAt = _clock.Elapsed;
        _running = true;
        return true;
    }
    public static TimeSpan DurationOf(PomodoroStage stage) => stage switch
    {
        PomodoroStage.Focus => TimeSpan.FromMinutes(25),
        PomodoroStage.ShortBreak => TimeSpan.FromMinutes(5),
        PomodoroStage.LongBreak => TimeSpan.FromMinutes(15),
        _ => throw new ArgumentOutOfRangeException(nameof(stage))
    };
}
