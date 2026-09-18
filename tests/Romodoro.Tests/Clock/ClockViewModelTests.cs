using System.Globalization;
using FluentAssertions;
using Romodoro.Clock;
using Romodoro.Tests.Fakes;

namespace Romodoro.Tests.Clock;
public class ClockViewModelTests
{
    [Fact]
    public void Formats_localized_time_and_date()
    {
        var ticks = new FakeTickSource();
        var now = new DateTimeOffset(2026, 9, 18, 14, 32, 0, TimeSpan.FromHours(-3));
        var sut = new ClockViewModel(ticks, () => now, CultureInfo.GetCultureInfo("pt-BR"));
        sut.TimeText.Should().Be("14:32");
        sut.DateText.Should().Be("sexta-feira, 18 de setembro");
    }
}
