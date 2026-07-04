using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class ChartCnTests : BunitContext
{
    // --- ChartCn ---

    [Fact]
    public void Chart_Renders_With_DataSlot()
    {
        var cut = Render<ChartCn>(p => p.AddChildContent("Chart content"));
        cut.Find("[data-slot='chart']").Should().NotBeNull();
    }

    [Fact]
    public void Chart_Has_Default_Classes()
    {
        // Layout (flex/aspect-video/justify-center/text-xs) now lives in the cn-chart CSS class.
        var cut = Render<ChartCn>(p => p.AddChildContent("Chart content"));
        var el = cut.Find("[data-slot='chart']");
        el.ClassList.Should().Contain("cn-chart");
    }

    [Fact]
    public void Chart_Renders_ChildContent()
    {
        var cut = Render<ChartCn>(p => p.AddChildContent("<span>My Chart</span>"));
        cut.Find("[data-slot='chart']").TextContent.Should().Contain("My Chart");
    }

    [Fact]
    public void Chart_Class_Passthrough()
    {
        var cut = Render<ChartCn>(p => p
            .Add(c => c.Class, "custom-chart")
            .AddChildContent("Chart"));
        cut.Find("[data-slot='chart']").ClassList.Should().Contain("custom-chart");
    }

    [Fact]
    public void Chart_AdditionalAttributes_Passthrough()
    {
        var cut = Render<ChartCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-chart" } })
            .AddChildContent("Chart"));
        cut.Find("[data-slot='chart']").GetAttribute("id").Should().Be("my-chart");
    }

    // --- ChartContainerCn ---

    [Fact]
    public void ChartContainer_Renders_With_DataSlot()
    {
        var cut = Render<ChartContainerCn>(p => p.AddChildContent("Bars"));
        cut.Find("[data-slot='chart-container']").Should().NotBeNull();
    }

    [Fact]
    public void ChartContainer_Has_Default_Classes()
    {
        var cut = Render<ChartContainerCn>(p => p.AddChildContent("Bars"));
        var el = cut.Find("[data-slot='chart-container']");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("items-end");
        el.ClassList.Should().Contain("gap-2");
    }

    [Fact]
    public void ChartContainer_Renders_ChildContent()
    {
        var cut = Render<ChartContainerCn>(p => p.AddChildContent("<div>Bar 1</div>"));
        cut.Find("[data-slot='chart-container']").TextContent.Should().Contain("Bar 1");
    }

    [Fact]
    public void ChartContainer_Class_Passthrough()
    {
        var cut = Render<ChartContainerCn>(p => p
            .Add(c => c.Class, "custom-container")
            .AddChildContent("Bars"));
        cut.Find("[data-slot='chart-container']").ClassList.Should().Contain("custom-container");
    }

    [Fact]
    public void ChartContainer_AdditionalAttributes_Passthrough()
    {
        var cut = Render<ChartContainerCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "container-1" } })
            .AddChildContent("Bars"));
        cut.Find("[data-slot='chart-container']").GetAttribute("id").Should().Be("container-1");
    }

    // --- ChartLegendCn / ChartLegendItemCn ---

    [Fact]
    public void ChartLegend_Renders_With_DataSlot_And_Class()
    {
        var cut = Render<ChartLegendCn>(p => p.AddChildContent("Legend"));
        cut.Find("[data-slot='chart-legend']").ClassList.Should().Contain("cn-chart-legend");
    }

    [Fact]
    public void ChartLegendItem_With_Color_Renders_Swatch()
    {
        var cut = Render<ChartLegendItemCn>(p => p
            .Add(c => c.Color, "#2563eb")
            .AddChildContent("Desktop"));
        var el = cut.Find("[data-slot='chart-legend-item']");
        el.ClassList.Should().Contain("cn-chart-legend-item");
        var swatch = el.QuerySelector("div");
        swatch.Should().NotBeNull();
        swatch!.GetAttribute("style").Should().Contain("#2563eb");
        el.TextContent.Should().Contain("Desktop");
    }

    [Fact]
    public void ChartLegendItem_Without_Color_Has_No_Swatch()
    {
        var cut = Render<ChartLegendItemCn>(p => p.AddChildContent("<svg></svg>Icon legend"));
        var el = cut.Find("[data-slot='chart-legend-item']");
        el.QuerySelectorAll("div").Should().BeEmpty();
        el.QuerySelector("svg").Should().NotBeNull();
    }

    // --- ChartTooltipCn / ChartTooltipItemCn ---

    [Fact]
    public void ChartTooltip_Renders_With_DataSlot_And_Class()
    {
        var cut = Render<ChartTooltipCn>(p => p.AddChildContent("Tip"));
        cut.Find("[data-slot='chart-tooltip']").ClassList.Should().Contain("cn-chart-tooltip");
    }

    [Fact]
    public void ChartTooltipItem_With_Color_Renders_Dot_And_Centers()
    {
        var cut = Render<ChartTooltipItemCn>(p => p
            .Add(c => c.Color, "#60a5fa")
            .AddChildContent("Mobile"));
        var el = cut.Find("[data-slot='chart-tooltip-item']");
        el.ClassList.Should().Contain("cn-chart-tooltip-item");
        el.ClassList.Should().Contain("items-center");
        el.QuerySelector("div")!.GetAttribute("style").Should().Contain("#60a5fa");
    }

    // --- Nested ---

    [Fact]
    public void Chart_With_Container_Nested()
    {
        var cut = Render<ChartCn>(p => p
            .AddChildContent<ChartContainerCn>(c => c
                .AddChildContent("<div>Bar</div>")));
        cut.Find("[data-slot='chart']").Should().NotBeNull();
        cut.Find("[data-slot='chart-container']").Should().NotBeNull();
    }
}
