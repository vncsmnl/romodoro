using System.Diagnostics;
using Romodoro.Pomodoro;

namespace Romodoro.Notifications;

public sealed class PlatformCompletionNotifier : ICompletionNotifier
{
    public event EventHandler<string>? FallbackRequested;
    public async Task NotifyAsync(PomodoroStage completed, PomodoroStage next, CancellationToken cancellationToken = default)
    {
        var message = $"{Label(completed)} concluído — próxima etapa: {Label(next)}.";
        try
        {
            if (OperatingSystem.IsMacOS())
                Start("osascript", $"-e \"display notification \\\"{message}\\\" with title \\\"Romodoro\\\"\"");
            else if (OperatingSystem.IsLinux())
                Start("notify-send", $"Romodoro \"{message}\"");
            else if (OperatingSystem.IsWindows())
                Start("msg", $"* \"{message}\"");
            else throw new PlatformNotSupportedException();
        }
        catch { FallbackRequested?.Invoke(this, message); }
        try { if (OperatingSystem.IsWindows()) Console.Beep(880, 180); else _ = Task.Run(() => { }); } catch { FallbackRequested?.Invoke(this, message); }
        await Task.CompletedTask;
    }
    private static void Start(string file, string args) => Process.Start(new ProcessStartInfo(file, args) { UseShellExecute = false });
    private static string Label(PomodoroStage stage) => stage switch { PomodoroStage.Focus => "Foco", PomodoroStage.ShortBreak => "Pausa curta", _ => "Pausa longa" };
}
