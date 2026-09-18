using Romodoro.Time;

namespace Romodoro.Stopwatch;

public sealed class StopwatchState
{
    private readonly IMonotonicClock _clock;
    private TimeSpan _accumulated;
    private TimeSpan? _startedAt;
    private readonly List<TimeSpan> _laps = [];
    public StopwatchState(IMonotonicClock clock) => _clock = clock;
    public bool IsRunning => _startedAt.HasValue;
    public TimeSpan Elapsed => _startedAt is { } started ? _accumulated + (_clock.Elapsed - started) : _accumulated;
    public IReadOnlyList<TimeSpan> Laps => _laps;
    public void Start() { if (!IsRunning) _startedAt = _clock.Elapsed; }
    public void Pause() { if (_startedAt is { } started) { _accumulated += _clock.Elapsed - started; _startedAt = null; } }
    public void Reset() { _startedAt = null; _accumulated = TimeSpan.Zero; _laps.Clear(); }
    public void AddLap() { if (IsRunning || Elapsed > TimeSpan.Zero) _laps.Add(Elapsed); }
}
