using FluentAssertions;
using Romodoro.Notifications;
using Romodoro.Pomodoro;

namespace Romodoro.Tests.Notifications;
public class CompletionNotifierTests
{
    [Fact]
    public async Task Native_failure_still_plays_audio_and_requests_fallback()
    {
        var audio = new RecordingAudio(); var fallback = string.Empty;
        var sut = new CompletionNotifier(new ThrowingNotification(), audio);
        sut.FallbackRequested += (_, message) => fallback = message;
        await sut.NotifyAsync(PomodoroStage.Focus, PomodoroStage.ShortBreak);
        audio.Count.Should().Be(1); fallback.Should().Contain("Foco concluído");
    }
    private sealed class ThrowingNotification : INotificationSink { public Task ShowAsync(string title, string message, CancellationToken token) => throw new InvalidOperationException(); }
    private sealed class RecordingAudio : IAudioSink { public int Count { get; private set; } public Task PlayAsync(CancellationToken token) { Count++; return Task.CompletedTask; } }
}
