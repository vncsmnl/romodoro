using FluentAssertions;
using Romodoro.Pomodoro;
using Romodoro.Tests.Fakes;

namespace Romodoro.Tests.Pomodoro;

public class PomodoroStateTests
{
    [Fact]
    public void Fourth_focus_selects_long_break()
    {
        var clock = new FakeMonotonicClock(); var sut = new PomodoroState(clock);
        for (var i = 0; i < 4; i++) { sut.Start(); clock.Advance(TimeSpan.FromMinutes(25)); sut.AdvanceIfComplete(); if (i < 3) { sut.Skip(); } }
        sut.CompletedFocusCount.Should().Be(4); sut.Stage.Should().Be(PomodoroStage.LongBreak);
    }

    [Fact]
    public void Reset_stage_keeps_completed_count()
    {
        var clock = new FakeMonotonicClock(); var sut = new PomodoroState(clock);
        sut.Start(); clock.Advance(TimeSpan.FromMinutes(25)); sut.AdvanceIfComplete(); sut.Start(); clock.Advance(TimeSpan.FromMinutes(2)); sut.ResetStage();
        sut.Remaining.Should().Be(TimeSpan.FromMinutes(5)); sut.CompletedFocusCount.Should().Be(1);
    }
}
