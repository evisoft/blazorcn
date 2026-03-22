using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BlazorCN.Tests.Components;

public class CheckboxCnTests : BunitContext
{
    [Fact]
    public void Renders_Unchecked_By_Default()
    {
        var cut = Render<CheckboxCn>();
        var button = cut.Find("button");
        button.GetAttribute("aria-checked").Should().Be("false");
        button.GetAttribute("data-state").Should().Be("unchecked");
    }

    [Fact]
    public void Renders_Checked_State_With_Check_Icon()
    {
        var cut = Render<CheckboxCn>(p => p.Add(c => c.Checked, true));
        var button = cut.Find("button");
        button.GetAttribute("aria-checked").Should().Be("true");
        button.GetAttribute("data-state").Should().Be("checked");
        button.QuerySelector("svg").Should().NotBeNull();
    }

    [Fact]
    public void Click_Toggles_Checked_State()
    {
        var checkedValue = false;
        var cut = Render<CheckboxCn>(p => p
            .Add(c => c.CheckedChanged, EventCallback.Factory.Create<bool>(this, v => checkedValue = v)));
        cut.Find("button").Click();
        checkedValue.Should().BeTrue();
    }

    [Fact]
    public void Disabled_Prevents_Toggle()
    {
        var checkedValue = false;
        var cut = Render<CheckboxCn>(p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.CheckedChanged, EventCallback.Factory.Create<bool>(this, v => checkedValue = v)));
        cut.Find("button").Click();
        checkedValue.Should().BeFalse();
    }

    [Fact]
    public void Has_DataSlot_Checkbox()
    {
        var cut = Render<CheckboxCn>();
        var button = cut.Find("button");
        button.GetAttribute("data-slot").Should().Be("checkbox");
    }

    [Fact]
    public void Custom_Class_Is_Passed_Through()
    {
        var cut = Render<CheckboxCn>(p => p.Add(c => c.Class, "my-class"));
        var button = cut.Find("button");
        button.ClassList.Should().Contain("my-class");
    }
}
