using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

/// <summary>
/// The by-name icon dispatcher resolves `Name="foo-bar"` to the type `LucideFooBarCn` by
/// reflection. When Lucide renames an icon, the old name resolves to NOTHING and the icon
/// silently disappears — no exception, no warning, no icon. An alias table in LucideIconCn
/// absorbs those renames; these tests pin the ones that were found missing in the demo
/// (582 uses across 39 names).
/// </summary>
public class LucideIconCnAliasTests : BunitContext
{
    [Theory]
    [InlineData("loader-2")]        // -> loader-circle
    [InlineData("bar-chart-3")]     // -> chart-column
    [InlineData("filter")]          // -> funnel
    [InlineData("upload-cloud")]    // -> cloud-upload
    [InlineData("help-circle")]     // -> circle-question-mark
    [InlineData("circle-help")]
    [InlineData("pie-chart")]       // -> chart-pie
    [InlineData("line-chart")]      // -> chart-line
    [InlineData("home")]            // -> house
    [InlineData("more-horizontal")] // -> ellipsis
    [InlineData("unlock")]          // -> lock-open
    [InlineData("grid")]            // -> layout-grid
    [InlineData("text")]            // -> type
    [InlineData("palmtree")]        // -> tree-palm
    [InlineData("file-audio")]      // -> file-music
    [InlineData("pen-square")]      // -> square-pen
    public void Renamed_Icon_Names_Still_Render_An_Svg(string name)
    {
        var cut = Render<LucideIconCn>(p => p.Add(c => c.Name, name));
        cut.FindAll("svg").Should().NotBeEmpty($"icon name '{name}' must resolve to a real component");
    }

    [Fact]
    public void Current_Icon_Names_Render_An_Svg()
    {
        foreach (var name in new[] { "check", "circle-alert", "house", "funnel", "loader-circle" })
            Render<LucideIconCn>(p => p.Add(c => c.Name, name)).FindAll("svg").Should().NotBeEmpty();
    }

    [Fact]
    public void Unknown_Icon_Name_Renders_Nothing_Rather_Than_Throwing()
    {
        var cut = Render<LucideIconCn>(p => p.Add(c => c.Name, "definitely-not-an-icon"));
        cut.FindAll("svg").Should().BeEmpty();
    }
}
