using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class InputCnTests : BunitContext
{
    [Fact]
    public void Default_Input_Renders_With_DataSlot_And_BaseClasses()
    {
        var cut = Render<InputCn>();
        var input = cut.Find("input");
        input.GetAttribute("data-slot").Should().Be("input");
        input.GetAttribute("type").Should().Be("text");
        input.ClassList.Should().Contain("cn-input");
        input.ClassList.Should().Contain("w-full");
    }

    [Fact]
    public void Placeholder_Is_Rendered()
    {
        var cut = Render<InputCn>(p => p.Add(c => c.Placeholder, "Enter text..."));
        var input = cut.Find("input");
        input.GetAttribute("placeholder").Should().Be("Enter text...");
    }

    [Fact]
    public void Type_Can_Be_Changed()
    {
        var cut = Render<InputCn>(p => p.Add(c => c.Type, "email"));
        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("email");
    }

    [Fact]
    public void TwoWay_Binding_Works()
    {
        string? receivedValue = null;
        var cut = Render<InputCn>(p => p
            .Add(c => c.Value, "initial")
            .Add(c => c.ValueChanged, (string val) => receivedValue = val));
        var input = cut.Find("input");
        input.Input("new value");
        receivedValue.Should().Be("new value");
    }

    [Fact]
    public void Disabled_State_Renders_Disabled_Attribute()
    {
        var cut = Render<InputCn>(p => p.Add(c => c.Disabled, true));
        var input = cut.Find("input");
        input.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Custom_Class_Is_Passed_Through()
    {
        var cut = Render<InputCn>(p => p.Add(c => c.Class, "extra-input"));
        var input = cut.Find("input");
        input.ClassList.Should().Contain("extra-input");
    }

    [Fact]
    public void Additional_Attributes_Passed_Through()
    {
        var cut = Render<InputCn>(p => p
            .AddUnmatched("data-testid", "input-1")
            .AddUnmatched("aria-label", "test input"));
        var input = cut.Find("input");
        input.GetAttribute("data-testid").Should().Be("input-1");
        input.GetAttribute("aria-label").Should().Be("test input");
    }
}
