using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class AlertCnTests : BunitContext
{
    [Fact]
    public void Default_Alert_Renders_With_DataSlot_And_BaseClasses()
    {
        var cut = Render<AlertCn>(p => p.AddChildContent("Alert message"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("alert");
        div.GetAttribute("role").Should().Be("alert");
        div.ClassList.Should().Contain("cn-alert");
        div.ClassList.Should().Contain("cn-alert-variant-default");
    }

    [Fact]
    public void Destructive_Variant_Has_TextDestructive()
    {
        var cut = Render<AlertCn>(p => p
            .Add(c => c.Variant, AlertVariant.Destructive)
            .AddChildContent("Error"));
        var div = cut.Find("div");
        div.ClassList.Should().Contain("cn-alert-variant-destructive");
    }

    [Fact]
    public void Custom_Class_Is_Passed_Through()
    {
        var cut = Render<AlertCn>(p => p
            .Add(c => c.Class, "my-alert")
            .AddChildContent("Alert"));
        var div = cut.Find("div");
        div.ClassList.Should().Contain("my-alert");
    }

    [Fact]
    public void Additional_Attributes_Passed_Through()
    {
        var cut = Render<AlertCn>(p => p
            .AddUnmatched("data-testid", "alert-1")
            .AddChildContent("Alert"));
        var div = cut.Find("div");
        div.GetAttribute("data-testid").Should().Be("alert-1");
    }

    [Fact]
    public void AlertTitleCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<AlertTitleCn>(p => p.AddChildContent("Warning"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("alert-title");
        div.ClassList.Should().Contain("cn-alert-title");
    }

    [Fact]
    public void AlertDescriptionCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<AlertDescriptionCn>(p => p.AddChildContent("Details here"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("alert-description");
        div.ClassList.Should().Contain("cn-alert-description");
    }

    [Fact]
    public void AlertTitleCn_Custom_Class_Is_Passed_Through()
    {
        var cut = Render<AlertTitleCn>(p => p
            .Add(c => c.Class, "extra-title")
            .AddChildContent("Title"));
        var div = cut.Find("div");
        div.ClassList.Should().Contain("extra-title");
    }

    [Fact]
    public void AlertDescriptionCn_Custom_Class_Is_Passed_Through()
    {
        var cut = Render<AlertDescriptionCn>(p => p
            .Add(c => c.Class, "extra-desc")
            .AddChildContent("Desc"));
        var div = cut.Find("div");
        div.ClassList.Should().Contain("extra-desc");
    }
}
