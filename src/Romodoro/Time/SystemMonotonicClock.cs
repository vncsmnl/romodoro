using System.Diagnostics;

namespace Romodoro.Time;

public sealed class SystemMonotonicClock : IMonotonicClock
{
    private readonly global::System.Diagnostics.Stopwatch _watch = global::System.Diagnostics.Stopwatch.StartNew();
    public TimeSpan Elapsed => _watch.Elapsed;
}
