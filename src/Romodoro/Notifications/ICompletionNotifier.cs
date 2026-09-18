using Romodoro.Pomodoro;

namespace Romodoro.Notifications;

public interface ICompletionNotifier
{
    Task NotifyAsync(PomodoroStage completed, PomodoroStage nextStage, CancellationToken cancellationToken = default);
}
