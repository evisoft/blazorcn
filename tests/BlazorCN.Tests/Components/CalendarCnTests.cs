using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class CalendarCnTests : BunitContext
{
    [Fact]
    public void Renders_Current_Month()
    {
        var date = new DateTime(2025, 6, 15);
        var cut = Render<CalendarCn>(p => p.Add(c => c.Value, date));
        var calendar = cut.Find("[data-slot='calendar']");
        cut.Markup.Should().Contain("June 2025");
    }

    [Fact]
    public void Has_Day_Headers()
    {
        var cut = Render<CalendarCn>(p => p.Add(c => c.Value, new DateTime(2025, 1, 1)));
        var headers = cut.FindAll("th");
        headers.Should().HaveCount(7);
        headers[0].TextContent.Should().Be("Su");
        headers[1].TextContent.Should().Be("Mo");
    }

    [Fact]
    public void Has_DataSlot_Calendar()
    {
        var cut = Render<CalendarCn>();
        var calendar = cut.Find("[data-slot='calendar']");
        calendar.GetAttribute("data-slot").Should().Be("calendar");
    }

    [Fact]
    public void Navigation_Buttons_Exist()
    {
        var cut = Render<CalendarCn>(p => p.Add(c => c.Value, new DateTime(2025, 1, 1)));
        var buttons = cut.FindAll("button[type='button']");
        // At least 2 nav buttons (prev/next) plus day buttons
        buttons.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void Custom_Class_Is_Passed_Through()
    {
        var cut = Render<CalendarCn>(p => p.Add(c => c.Class, "my-calendar"));
        var calendar = cut.Find("[data-slot='calendar']");
        calendar.ClassList.Should().Contain("my-calendar");
    }

    [Fact]
    public void Day_Buttons_Are_Rendered()
    {
        var cut = Render<CalendarCn>(p => p.Add(c => c.Value, new DateTime(2025, 1, 15)));
        var dayButtons = cut.FindAll("td button");
        dayButtons.Count.Should().BeGreaterThan(0);
    }
}
