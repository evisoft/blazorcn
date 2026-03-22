using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BlazorCN.Tests.Components;

public class SwitchCnTests : BunitContext
{
    [Fact]
    public void Renders_Unchecked_By_Default()
    {
        var cut = Render<SwitchCn>();
        var button = cut.Find("button");
        button.GetAttribute("aria-checked").Should().Be("false");
        button.GetAttribute("data-state").Should().Be("unchecked");
    }

    [Fact]
    public void Click_Toggles_State()
    {
        var checkedValue = false;
        var cut = Render<SwitchCn>(p => p
            .Add(c => c.CheckedChanged, EventCallback.Factory.Create<bool>(this, v => checkedValue = v)));
        cut.Find("button").Click();
        checkedValue.Should().BeTrue();
    }

    [Fact]
    public void Has_Thumb_Element()
    {
        var cut = Render<SwitchCn>();
        var thumb = cut.Find("[data-slot='switch-thumb']");
        thumb.Should().NotBeNull();
        thumb.TagName.Should().Be("SPAN");
    }

    [Fact]
    public void Disabled_Prevents_Toggle()
    {
        var checkedValue = false;
        var cut = Render<SwitchCn>(p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.CheckedChanged, EventCallback.Factory.Create<bool>(this, v => checkedValue = v)));
        cut.Find("button").Click();
        checkedValue.Should().BeFalse();
    }

    [Fact]
    public void Has_DataSlot_Switch()
    {
        var cut = Render<SwitchCn>();
        var button = cut.Find("button");
        button.GetAttribute("data-slot").Should().Be("switch");
    }

    [Fact]
    public void Thumb_Has_Correct_DataState()
    {
        var cut = Render<SwitchCn>(p => p.Add(c => c.Checked, true));
        var thumb = cut.Find("[data-slot='switch-thumb']");
        thumb.GetAttribute("data-state").Should().Be("checked");
    }
}
