using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BlazorCN.Tests.Components;

public class ButtonCnTests : BunitContext
{
    [Fact]
    public void Default_Button_Renders_With_BgPrimary()
    {
        var cut = Render<ButtonCn>(p => p.AddChildContent("Click me"));
        var button = cut.Find("button");
        button.GetAttribute("data-slot").Should().Be("button");
        button.ClassList.Should().Contain("cn-button");
        button.ClassList.Should().Contain("cn-button-variant-default");
        button.ClassList.Should().Contain("cn-button-size-default");
    }

    [Fact]
    public void Destructive_Variant_Has_BgDestructive()
    {
        var cut = Render<ButtonCn>(p => p
            .Add(c => c.Variant, ButtonVariant.Destructive)
            .AddChildContent("Delete"));
        var button = cut.Find("button");
        button.ClassList.Should().Contain("cn-button-variant-destructive");
        button.GetAttribute("data-variant").Should().Be("destructive");
    }

    [Fact]
    public void Outline_Variant_Has_Border_And_BgBackground()
    {
        var cut = Render<ButtonCn>(p => p
            .Add(c => c.Variant, ButtonVariant.Outline)
            .AddChildContent("Outline"));
        var button = cut.Find("button");
        button.ClassList.Should().Contain("cn-button-variant-outline");
    }

    [Fact]
    public void Small_Size_Has_H8()
    {
        var cut = Render<ButtonCn>(p => p
            .Add(c => c.Size, ButtonSize.Sm)
            .AddChildContent("Small"));
        var button = cut.Find("button");
        button.ClassList.Should().Contain("cn-button-size-sm");
    }

    [Fact]
    public void Disabled_State_Renders_Disabled_Attribute()
    {
        var cut = Render<ButtonCn>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent("Disabled"));
        var button = cut.Find("button");
        button.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Custom_Class_Is_Passed_Through()
    {
        var cut = Render<ButtonCn>(p => p
            .Add(c => c.Class, "my-custom-class")
            .AddChildContent("Custom"));
        var button = cut.Find("button");
        button.ClassList.Should().Contain("my-custom-class");
    }

    [Fact]
    public void OnClick_Fires()
    {
        var clicked = false;
        var cut = Render<ButtonCn>(p => p
            .Add(c => c.OnClick, EventCallback.Factory.Create(this, () => clicked = true))
            .AddChildContent("Click"));
        cut.Find("button").Click();
        clicked.Should().BeTrue();
    }

    [Fact]
    public void Additional_Attributes_Passed_Through()
    {
        var cut = Render<ButtonCn>(p => p
            .AddUnmatched("data-testid", "btn-1")
            .AddUnmatched("aria-label", "test button")
            .AddChildContent("Attr"));
        var button = cut.Find("button");
        button.GetAttribute("data-testid").Should().Be("btn-1");
        button.GetAttribute("aria-label").Should().Be("test button");
    }

    [Fact]
    public void Href_Renders_As_Anchor_Tag()
    {
        var cut = Render<ButtonCn>(p => p
            .Add(c => c.Href, "https://example.com")
            .AddChildContent("Link"));
        var anchor = cut.Find("a");
        anchor.GetAttribute("href").Should().Be("https://example.com");
        anchor.GetAttribute("data-slot").Should().Be("button");
        anchor.ClassList.Should().Contain("cn-button-variant-default");
    }

    [Fact]
    public void Default_Type_Is_Button()
    {
        var cut = Render<ButtonCn>(p => p.AddChildContent("Click"));
        var button = cut.Find("button");
        button.GetAttribute("type").Should().Be("button");
    }

    [Fact]
    public void Data_Variant_And_Size_Attributes_Set()
    {
        var cut = Render<ButtonCn>(p => p
            .Add(c => c.Variant, ButtonVariant.Ghost)
            .Add(c => c.Size, ButtonSize.Lg)
            .AddChildContent("Ghost Lg"));
        var button = cut.Find("button");
        button.GetAttribute("data-variant").Should().Be("ghost");
        button.GetAttribute("data-size").Should().Be("lg");
    }
}
