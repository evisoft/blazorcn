using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class BadgeCnTests : BunitContext
{
    [Fact]
    public void Default_Badge_Renders_With_BgPrimary()
    {
        var cut = Render<BadgeCn>(p => p.AddChildContent("New"));
        var span = cut.Find("span");
        span.GetAttribute("data-slot").Should().Be("badge");
        span.ClassList.Should().Contain("cn-badge");
        span.ClassList.Should().Contain("cn-badge-variant-default");
    }

    [Fact]
    public void Destructive_Variant_Has_BgDestructive()
    {
        var cut = Render<BadgeCn>(p => p
            .Add(c => c.Variant, BadgeVariant.Destructive)
            .AddChildContent("Error"));
        var span = cut.Find("span");
        span.ClassList.Should().Contain("cn-badge-variant-destructive");
        span.GetAttribute("data-variant").Should().Be("destructive");
    }

    [Fact]
    public void Outline_Variant_Has_BorderBorder()
    {
        var cut = Render<BadgeCn>(p => p
            .Add(c => c.Variant, BadgeVariant.Outline)
            .AddChildContent("Outline"));
        var span = cut.Find("span");
        span.ClassList.Should().Contain("cn-badge-variant-outline");
    }

    [Fact]
    public void Custom_Class_Is_Passed_Through()
    {
        var cut = Render<BadgeCn>(p => p
            .Add(c => c.Class, "extra-badge-class")
            .AddChildContent("Custom"));
        var span = cut.Find("span");
        span.ClassList.Should().Contain("extra-badge-class");
    }

    [Fact]
    public void Additional_Attributes_Passed_Through()
    {
        var cut = Render<BadgeCn>(p => p
            .AddUnmatched("data-testid", "badge-1")
            .AddChildContent("Attr"));
        var span = cut.Find("span");
        span.GetAttribute("data-testid").Should().Be("badge-1");
    }

    [Fact]
    public void Secondary_Variant_Has_BgSecondary()
    {
        var cut = Render<BadgeCn>(p => p
            .Add(c => c.Variant, BadgeVariant.Secondary)
            .AddChildContent("Sec"));
        var span = cut.Find("span");
        span.ClassList.Should().Contain("cn-badge-variant-secondary");
    }
}
