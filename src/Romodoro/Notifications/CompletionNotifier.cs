using Romodoro.Pomodoro;

namespace Romodoro.Notifications;

public interface INotificationSink { Task ShowAsync(string title, string message, CancellationToken cancellationToken); }
public interface IAudioSink { Task PlayAsync(CancellationToken cancellationToken); }

public sealed class CompletionNotifier(INotificationSink notification, IAudioSink audio) : ICompletionNotifier
{
    public event EventHandler<string>? FallbackRequested;
    public async Task NotifyAsync(PomodoroStage completed, PomodoroStage next, CancellationToken cancellationToken = default)
    {
        var message = $"{Label(completed)} concluído. Hora d{(next == PomodoroStage.Focus ? 'o' : 'a')} {Label(next).ToLowerInvariant()}.";
        try { await notification.ShowAsync("Romodoro", message, cancellationToken); }
        catch { FallbackRequested?.Invoke(this, message); }
        try { await audio.PlayAsync(cancellationToken); }
        catch { FallbackRequested?.Invoke(this, message); }
    }
    private static string Label(PomodoroStage stage) => stage switch { PomodoroStage.Focus => "Foco", PomodoroStage.ShortBreak => "Pausa curta", _ => "Pausa longa" };
}

public sealed class NullCompletionNotifier : ICompletionNotifier
{
    public Task NotifyAsync(PomodoroStage completed, PomodoroStage next, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
