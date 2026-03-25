using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class TextareaCnTests : BunitContext
{
    [Fact]
    public void Default_Textarea_Renders_With_DataSlot_And_BaseClasses()
    {
        var cut = Render<TextareaCn>();
        var textarea = cut.Find("textarea");
        textarea.GetAttribute("data-slot").Should().Be("textarea");
        textarea.ClassList.Should().Contain("cn-textarea");
        textarea.ClassList.Should().Contain("min-h-16");
        textarea.ClassList.Should().Contain("w-full");
    }

    [Fact]
    public void Placeholder_Is_Rendered()
    {
        var cut = Render<TextareaCn>(p => p.Add(c => c.Placeholder, "Write here..."));
        var textarea = cut.Find("textarea");
        textarea.GetAttribute("placeholder").Should().Be("Write here...");
    }

    [Fact]
    public void TwoWay_Binding_Works()
    {
        string? receivedValue = null;
        var cut = Render<TextareaCn>(p => p
            .Add(c => c.Value, "initial")
            .Add(c => c.ValueChanged, (string val) => receivedValue = val));
        var textarea = cut.Find("textarea");
        textarea.Input("updated text");
        receivedValue.Should().Be("updated text");
    }

    [Fact]
    public void Disabled_State_Renders_Disabled_Attribute()
    {
        var cut = Render<TextareaCn>(p => p.Add(c => c.Disabled, true));
        var textarea = cut.Find("textarea");
        textarea.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Custom_Class_Is_Passed_Through()
    {
        var cut = Render<TextareaCn>(p => p.Add(c => c.Class, "extra-textarea"));
        var textarea = cut.Find("textarea");
        textarea.ClassList.Should().Contain("extra-textarea");
    }

    [Fact]
    public void Additional_Attributes_Passed_Through()
    {
        var cut = Render<TextareaCn>(p => p
            .AddUnmatched("data-testid", "ta-1")
            .AddUnmatched("rows", "5"));
        var textarea = cut.Find("textarea");
        textarea.GetAttribute("data-testid").Should().Be("ta-1");
        textarea.GetAttribute("rows").Should().Be("5");
    }
}
