using Avalonia.Threading;

namespace Romodoro.Time;

public sealed class DispatcherTickSource : ITickSource, IDisposable
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(250) };
    public event EventHandler? Tick;
    public DispatcherTickSource()
    {
        _timer.Tick += (_, _) => Tick?.Invoke(this, EventArgs.Empty);
    }
    public void Start() => _timer.Start();
    public void StopTicking() => _timer.Stop();
    public void Dispose() => StopTicking();
}
