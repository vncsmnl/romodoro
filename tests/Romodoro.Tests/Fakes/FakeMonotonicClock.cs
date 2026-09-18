using Romodoro.Time;

namespace Romodoro.Tests.Fakes;
public sealed class FakeMonotonicClock : IMonotonicClock
{
    public TimeSpan Elapsed { get; private set; }
    public void Advance(TimeSpan amount) => Elapsed += amount;
}

public sealed class FakeTickSource : ITickSource
{
    public event EventHandler? Tick;
    public void Start() { }
    public void Stop() { }
    public void Pulse() => Tick?.Invoke(this, EventArgs.Empty);
}
