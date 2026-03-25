using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BlazorCN.Tests.Components;

public class ToggleCnTests : BunitContext
{
    [Fact]
    public void Default_Toggle_Renders_With_DataSlot()
    {
        var cut = Render<ToggleCn>(p => p.AddChildContent("Bold"));
        var button = cut.Find("button");
        button.GetAttribute("data-slot").Should().Be("toggle");
    }

    [Fact]
    public void Default_Toggle_Has_Off_State()
    {
        var cut = Render<ToggleCn>(p => p.AddChildContent("Bold"));
        var button = cut.Find("button");
        button.GetAttribute("data-state").Should().Be("off");
        button.GetAttribute("aria-pressed").Should().Be("false");
    }

    [Fact]
    public void Pressed_Toggle_Has_On_State()
    {
        var cut = Render<ToggleCn>(p => p
            .Add(c => c.Pressed, true)
            .AddChildContent("Bold"));
        var button = cut.Find("button");
        button.GetAttribute("data-state").Should().Be("on");
        button.GetAttribute("aria-pressed").Should().Be("true");
    }

    [Fact]
    public void Click_Toggles_Pressed_State()
    {
        var pressed = false;
        var cut = Render<ToggleCn>(p => p
            .Add(c => c.PressedChanged, EventCallback.Factory.Create<bool>(this, v => pressed = v))
            .AddChildContent("Bold"));
        cut.Find("button").Click();
        pressed.Should().BeTrue();
    }

    [Fact]
    public void Default_Variant_Has_BgTransparent()
    {
        var cut = Render<ToggleCn>(p => p.AddChildContent("Bold"));
        var button = cut.Find("button");
        button.ClassList.Should().Contain("cn-toggle");
        button.ClassList.Should().Contain("cn-toggle-variant-default");
    }

    [Fact]
    public void Outline_Variant_Has_Border()
    {
        var cut = Render<ToggleCn>(p => p
            .Add(c => c.Variant, ToggleVariant.Outline)
            .AddChildContent("Bold"));
        var button = cut.Find("button");
        button.ClassList.Should().Contain("cn-toggle-variant-outline");
    }

    [Fact]
    public void Small_Size_Has_H8()
    {
        var cut = Render<ToggleCn>(p => p
            .Add(c => c.Size, ToggleSize.Sm)
            .AddChildContent("Bold"));
        var button = cut.Find("button");
        button.ClassList.Should().Contain("cn-toggle-size-sm");
    }

    [Fact]
    public void Large_Size_Has_H10()
    {
        var cut = Render<ToggleCn>(p => p
            .Add(c => c.Size, ToggleSize.Lg)
            .AddChildContent("Bold"));
        var button = cut.Find("button");
        button.ClassList.Should().Contain("cn-toggle-size-lg");
    }

    [Fact]
    public void Disabled_Toggle_Has_Disabled_Attribute()
    {
        var cut = Render<ToggleCn>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent("Bold"));
        var button = cut.Find("button");
        button.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Toggle_Has_Button_Type()
    {
        var cut = Render<ToggleCn>(p => p.AddChildContent("Bold"));
        var button = cut.Find("button");
        button.GetAttribute("type").Should().Be("button");
    }

    [Fact]
    public void Custom_Class_Is_Passed_Through()
    {
        var cut = Render<ToggleCn>(p => p
            .Add(c => c.Class, "my-class")
            .AddChildContent("Bold"));
        var button = cut.Find("button");
        button.ClassList.Should().Contain("my-class");
    }
}
