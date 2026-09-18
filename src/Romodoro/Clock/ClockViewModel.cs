using System.Globalization;
using Romodoro.Infrastructure;
using Romodoro.Time;

namespace Romodoro.Clock;

public sealed class ClockViewModel : BindableBase, IDisposable
{
    private readonly ITickSource _ticks;
    private readonly Func<DateTimeOffset> _now;
    private readonly CultureInfo _culture;
    private string _timeText = "00:00";
    private string _dateText = string.Empty;
    public string TimeText { get => _timeText; private set => SetProperty(ref _timeText, value); }
    public string DateText { get => _dateText; private set => SetProperty(ref _dateText, value); }
    public ClockViewModel(ITickSource ticks, Func<DateTimeOffset>? now = null, CultureInfo? culture = null)
    {
        _ticks = ticks; _now = now ?? (() => DateTimeOffset.Now); _culture = culture ?? CultureInfo.GetCultureInfo("pt-BR");
        _ticks.Tick += OnTick;
        Refresh();
    }
    public void Start() { Refresh(); _ticks.Start(); }
    public void Stop() => _ticks.Stop();
    private void OnTick(object? sender, EventArgs e) => Refresh();
    private void Refresh()
    {
        var now = _now().ToLocalTime();
        TimeText = now.ToString("HH:mm", _culture);
        var date = now.ToString("dddd, dd 'de' MMMM", _culture);
        DateText = char.ToLowerInvariant(date[0]) + date[1..];
    }
    public void Dispose() { _ticks.Tick -= OnTick; _ticks.Stop(); }
}
