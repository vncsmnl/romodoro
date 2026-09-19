using FluentAssertions;
using Romodoro.Stopwatch;
using Romodoro.Tests.Fakes;

namespace Romodoro.Tests.Stopwatch;

public class StopwatchStateTests
{
    [Fact]
    public void Pause_and_resume_preserve_accumulated_time()
    {
        var clock = new FakeMonotonicClock(); var sut = new StopwatchState(clock);
        sut.Start(); clock.Advance(TimeSpan.FromSeconds(5)); sut.Pause(); clock.Advance(TimeSpan.FromSeconds(20)); sut.Start(); clock.Advance(TimeSpan.FromSeconds(3));
        sut.Elapsed.Should().Be(TimeSpan.FromSeconds(8));
    }

    [Fact]
    public void Reset_clears_elapsed_and_laps()
    {
        var clock = new FakeMonotonicClock(); var sut = new StopwatchState(clock);
        sut.Start(); clock.Advance(TimeSpan.FromSeconds(8)); sut.AddLap(); sut.Reset();
        sut.Elapsed.Should().Be(TimeSpan.Zero); sut.Laps.Should().BeEmpty(); sut.IsRunning.Should().BeFalse();
    }
}
