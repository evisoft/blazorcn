using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class LabelCnTests : BunitContext
{
    [Fact]
    public void Default_Label_Renders_With_DataSlot_And_BaseClasses()
    {
        var cut = Render<LabelCn>(p => p.AddChildContent("Username"));
        var label = cut.Find("label");
        label.GetAttribute("data-slot").Should().Be("label");
        label.ClassList.Should().Contain("text-sm");
        label.ClassList.Should().Contain("font-medium");
        label.ClassList.Should().Contain("select-none");
    }

    [Fact]
    public void For_Attribute_Is_Set()
    {
        var cut = Render<LabelCn>(p => p
            .Add(c => c.For, "username-input")
            .AddChildContent("Username"));
        var label = cut.Find("label");
        label.GetAttribute("for").Should().Be("username-input");
    }

    [Fact]
    public void Custom_Class_Is_Passed_Through()
    {
        var cut = Render<LabelCn>(p => p
            .Add(c => c.Class, "my-label")
            .AddChildContent("Label"));
        var label = cut.Find("label");
        label.ClassList.Should().Contain("my-label");
    }

    [Fact]
    public void Additional_Attributes_Passed_Through()
    {
        var cut = Render<LabelCn>(p => p
            .AddUnmatched("data-testid", "label-1")
            .AddChildContent("Label"));
        var label = cut.Find("label");
        label.GetAttribute("data-testid").Should().Be("label-1");
    }
}
