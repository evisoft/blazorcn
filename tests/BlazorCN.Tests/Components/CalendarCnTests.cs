using System.Globalization;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
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

    [Fact]
    public void Single_Mode_Selects_Date()
    {
        DateTime? selected = null;
        var cut = Render<CalendarCn>(p => p
            .Add(c => c.Value, new DateTime(2025, 1, 15))
            .Add(c => c.ValueChanged, (DateTime? v) => selected = v));

        // Click day "10"
        var dayButton = cut.FindAll("td button").First(b => b.TextContent.Trim() == "10");
        dayButton.Click();

        selected.Should().NotBeNull();
        selected!.Value.Day.Should().Be(10);
    }

    [Fact]
    public void Multiple_Mode_Toggles_Dates()
    {
        List<DateTime>? result = null;
        var cut = Render<CalendarCn>(p => p
            .Add(c => c.Mode, CalendarSelectionMode.Multiple)
            .Add(c => c.SelectedDates, new List<DateTime>())
            .Add(c => c.SelectedDatesChanged, (List<DateTime> v) => result = v));

        // Click day "10" — wrap in InvokeAsync to avoid stale handler
        cut.InvokeAsync(() => cut.FindAll("td button").First(b => b.TextContent.Trim() == "10").Click());

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }

    [Fact]
    public void Multiple_Mode_Removes_On_Second_Click()
    {
        var today = DateTime.Today;
        var initialDates = new List<DateTime> { new DateTime(today.Year, today.Month, 10) };
        List<DateTime>? result = null;
        var cut = Render<CalendarCn>(p => p
            .Add(c => c.Mode, CalendarSelectionMode.Multiple)
            .Add(c => c.SelectedDates, initialDates)
            .Add(c => c.SelectedDatesChanged, (List<DateTime> v) => result = v));

        // Click day "10" to deselect
        var dayButton = cut.FindAll("td button").First(b => b.TextContent.Trim() == "10");
        dayButton.Click();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Range_Mode_Sets_Start_And_End()
    {
        DateTime? start = null;
        DateTime? end = null;
        var cut = Render<CalendarCn>(p => p
            .Add(c => c.Mode, CalendarSelectionMode.Range)
            .Add(c => c.Value, new DateTime(2025, 1, 1))
            .Add(c => c.RangeStartChanged, (DateTime? v) => start = v)
            .Add(c => c.RangeEndChanged, (DateTime? v) => end = v));

        // First click sets start — re-query after render
        cut.InvokeAsync(() => cut.FindAll("td button").First(b => b.TextContent.Trim() == "5").Click());
        start.Should().NotBeNull();
        start!.Value.Day.Should().Be(5);
        end.Should().BeNull();

        // Second click sets end — re-query again
        cut.InvokeAsync(() => cut.FindAll("td button").First(b => b.TextContent.Trim() == "15").Click());
        end.Should().NotBeNull();
        end!.Value.Day.Should().Be(15);
    }

    [Fact]
    public void Range_Mode_Swaps_If_End_Before_Start()
    {
        DateTime? start = null;
        DateTime? end = null;
        var cut = Render<CalendarCn>(p => p
            .Add(c => c.Mode, CalendarSelectionMode.Range)
            .Add(c => c.Value, new DateTime(2025, 1, 1))
            .Add(c => c.RangeStartChanged, (DateTime? v) => start = v)
            .Add(c => c.RangeEndChanged, (DateTime? v) => end = v));

        // Click 15 first, then 5 — re-query after each render
        cut.InvokeAsync(() => cut.FindAll("td button").First(b => b.TextContent.Trim() == "15").Click());
        cut.InvokeAsync(() => cut.FindAll("td button").First(b => b.TextContent.Trim() == "5").Click());

        start!.Value.Day.Should().Be(5);
        end!.Value.Day.Should().Be(15);
    }

    [Fact]
    public void Disabled_Dates_Are_Disabled()
    {
        var today = DateTime.Today;
        var disabledDate = new DateTime(today.Year, today.Month, 15);
        var cut = Render<CalendarCn>(p => p
            .Add(c => c.DisabledDates, new List<DateTime> { disabledDate }));

        var dayButton = cut.FindAll("td button").First(b => b.TextContent.Trim() == "15");
        dayButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Disabled_Dates_Get_LineThrough_Class()
    {
        var today = DateTime.Today;
        var disabledDate = new DateTime(today.Year, today.Month, 15);
        var cut = Render<CalendarCn>(p => p
            .Add(c => c.DisabledDates, new List<DateTime> { disabledDate }));

        var dayButton = cut.FindAll("td button").First(b => b.TextContent.Trim() == "15");
        dayButton.ClassList.Should().Contain("line-through");
    }

    [Fact]
    public void ShowWeekNumbers_Renders_Week_Column()
    {
        var cut = Render<CalendarCn>(p => p
            .Add(c => c.Value, new DateTime(2025, 1, 15))
            .Add(c => c.ShowWeekNumbers, true));

        // Header should now have 8 columns (1 empty + 7 day names)
        var headers = cut.FindAll("th");
        headers.Should().HaveCount(8);

        // Each body row should have week number cell
        var bodyRows = cut.FindAll("tbody tr");
        bodyRows.Should().NotBeEmpty();
        // First td in first body row should contain a week number (numeric)
        var weekNumCells = cut.FindAll("tbody tr td:first-child");
        weekNumCells.Should().NotBeEmpty();
        var firstWeekNum = weekNumCells[0].TextContent.Trim();
        int.TryParse(firstWeekNum, out var weekNum).Should().BeTrue();
        weekNum.Should().BeInRange(1, 53);
    }

    [Fact]
    public void ShowWeekNumbers_False_Has_No_Extra_Column()
    {
        var cut = Render<CalendarCn>(p => p
            .Add(c => c.Value, new DateTime(2025, 1, 15))
            .Add(c => c.ShowWeekNumbers, false));

        var headers = cut.FindAll("th");
        headers.Should().HaveCount(7);
    }

    [Fact]
    public void Previous_Month_Changes_Display()
    {
        var cut = Render<CalendarCn>(p => p.Add(c => c.Value, new DateTime(2025, 3, 15)));
        cut.Markup.Should().Contain("March 2025");

        // Click prev
        var prevBtn = cut.Find("button[aria-label='Previous month']");
        prevBtn.Click();
        cut.Markup.Should().Contain("February 2025");
    }

    [Fact]
    public void Next_Month_Changes_Display()
    {
        var cut = Render<CalendarCn>(p => p.Add(c => c.Value, new DateTime(2025, 3, 15)));
        cut.Markup.Should().Contain("March 2025");

        // Click next
        var nextBtn = cut.Find("button[aria-label='Next month']");
        nextBtn.Click();
        cut.Markup.Should().Contain("April 2025");
    }

    [Fact]
    public void MinDate_Disables_Earlier_Dates()
    {
        var cut = Render<CalendarCn>(p => p
            .Add(c => c.Value, new DateTime(2025, 1, 15))
            .Add(c => c.MinDate, new DateTime(2025, 1, 10)));

        var day5 = cut.FindAll("td button").First(b => b.TextContent.Trim() == "5");
        day5.HasAttribute("disabled").Should().BeTrue();

        var day15 = cut.FindAll("td button").First(b => b.TextContent.Trim() == "15");
        day15.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void MaxDate_Disables_Later_Dates()
    {
        var cut = Render<CalendarCn>(p => p
            .Add(c => c.Value, new DateTime(2025, 1, 15))
            .Add(c => c.MaxDate, new DateTime(2025, 1, 20)));

        var day25 = cut.FindAll("td button").First(b => b.TextContent.Trim() == "25");
        day25.HasAttribute("disabled").Should().BeTrue();

        var day15 = cut.FindAll("td button").First(b => b.TextContent.Trim() == "15");
        day15.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void Mode_Defaults_To_Single()
    {
        var cut = Render<CalendarCn>();
        // Should render without error in default single mode
        cut.Find("[data-slot='calendar']").Should().NotBeNull();
    }

    [Fact]
    public void MinDate_In_Future_Moves_Tab_Stop_To_Enabled_Day()
    {
        // The seeded focus day (Value = June 1) is before MinDate and therefore
        // disabled — a disabled button is unfocusable, so the roving tab stop
        // must move to the nearest enabled day instead of trapping the keyboard.
        var cut = Render<CalendarCn>(p => p
            .Add(c => c.Value, new DateTime(2025, 6, 1))
            .Add(c => c.MinDate, new DateTime(2025, 6, 10)));

        var tabStops = cut.FindAll("td button[tabindex='0']");
        tabStops.Should().HaveCount(1);
        tabStops[0].HasAttribute("disabled").Should().BeFalse();
        tabStops[0].TextContent.Trim().Should().Be("10");
    }

    [Fact]
    public async Task Range_Completion_Survives_Async_Start_Handler()
    {
        // RangeStartChanged yields, so the host re-renders mid-flight and
        // SetParametersAsync resets the calendar's RangeEnd parameter before
        // RangeEndChanged fires — the callback must still report the clicked end.
        // The click's own Task settles as soon as that first yield is hit (Blazor
        // doesn't block event dispatch on the rest of the async chain), so the
        // final RangeEndChanged callback can still be in flight when InvokeAsync
        // returns — WaitForAssertion polls until it actually lands.
        var cut = Render<AsyncRangeHost>();

        await cut.InvokeAsync(() => cut.FindAll("td button").First(b => b.TextContent.Trim() == "5").Click());
        cut.WaitForAssertion(() => cut.Instance.Start.Should().Be(new DateTime(2025, 6, 5)));

        await cut.InvokeAsync(() => cut.FindAll("td button").First(b => b.TextContent.Trim() == "15").Click());
        cut.WaitForAssertion(() => cut.Instance.End.Should().Be(new DateTime(2025, 6, 15)));
    }

    [Fact]
    public void Caption_Stays_English_Under_German_Culture()
    {
        // Weekday headers are hardcoded English (react-day-picker default), so the
        // caption and aria-labels are deliberately InvariantCulture — never mixed-language.
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            var cut = Render<CalendarCn>(p => p.Add(c => c.Value, new DateTime(2025, 6, 15)));
            cut.Markup.Should().Contain("June 2025");
            cut.Markup.Should().NotContain("Juni");
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void DisplayMonth_Parameter_Controls_Visible_Month()
    {
        var cut = Render<CalendarCn>(p => p
            .Add(c => c.Value, new DateTime(2025, 3, 15))
            .Add(c => c.DisplayMonth, new DateTime(2025, 6, 1)));

        cut.Markup.Should().Contain("June 2025");
    }

    [Fact]
    public void Next_Month_Invokes_DisplayMonthChanged()
    {
        DateTime? reported = null;
        var cut = Render<CalendarCn>(p => p
            .Add(c => c.DisplayMonth, new DateTime(2025, 6, 1))
            .Add(c => c.DisplayMonthChanged, (DateTime? v) => reported = v));

        cut.Find("button[aria-label='Next month']").Click();

        reported.Should().Be(new DateTime(2025, 7, 1));
        cut.Markup.Should().Contain("July 2025");
    }

    /// <summary>Host that two-way binds RangeStart/RangeEnd the way a real parent does,
    /// with an async-yielding RangeStartChanged handler — reproduces the re-render race.</summary>
    private sealed class AsyncRangeHost : ComponentBase
    {
        public DateTime? Start;
        public DateTime? End;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<CalendarCn>(0);
            builder.AddComponentParameter(1, nameof(CalendarCn.Mode), CalendarSelectionMode.Range);
            builder.AddComponentParameter(2, nameof(CalendarCn.DisplayMonth), (DateTime?)new DateTime(2025, 6, 1));
            builder.AddComponentParameter(3, nameof(CalendarCn.RangeStart), Start);
            builder.AddComponentParameter(4, nameof(CalendarCn.RangeStartChanged),
                EventCallback.Factory.Create<DateTime?>(this, async v =>
                {
                    Start = v;
                    await Task.Yield();
                }));
            builder.AddComponentParameter(5, nameof(CalendarCn.RangeEnd), End);
            builder.AddComponentParameter(6, nameof(CalendarCn.RangeEndChanged),
                EventCallback.Factory.Create<DateTime?>(this, v => End = v));
            builder.CloseComponent();
        }
    }
}
