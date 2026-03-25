using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BlazorCN.Tests.Components;

public class NativeSelectCnTests : BunitContext
{
    [Fact]
    public void NativeSelect_Renders_With_DataSlot()
    {
        var cut = Render<NativeSelectCn>(p => p.AddChildContent("Options"));
        cut.Find("[data-slot='native-select']").Should().NotBeNull();
    }

    [Fact]
    public void NativeSelect_Wrapper_Renders_With_DataSlot()
    {
        var cut = Render<NativeSelectCn>(p => p.AddChildContent("Options"));
        cut.Find("[data-slot='native-select-wrapper']").Should().NotBeNull();
    }

    [Fact]
    public void NativeSelect_Is_Select_Element()
    {
        var cut = Render<NativeSelectCn>(p => p.AddChildContent("Options"));
        cut.Find("[data-slot='native-select']").TagName.Should().Be("SELECT");
    }

    [Fact]
    public void NativeSelect_Renders_Chevron_Icon()
    {
        var cut = Render<NativeSelectCn>(p => p.AddChildContent("Options"));
        cut.Find("[data-slot='native-select-icon']").Should().NotBeNull();
    }

    [Fact]
    public void NativeSelect_Default_Size_Has_DataSize_Default()
    {
        var cut = Render<NativeSelectCn>(p => p.AddChildContent("Options"));
        cut.Find("[data-slot='native-select']").GetAttribute("data-size").Should().Be("default");
    }

    [Fact]
    public void NativeSelect_Sm_Size_Has_DataSize_Sm()
    {
        var cut = Render<NativeSelectCn>(p => p
            .Add(c => c.Size, NativeSelectSize.Sm)
            .AddChildContent("Options"));
        cut.Find("[data-slot='native-select']").GetAttribute("data-size").Should().Be("sm");
    }

    [Fact]
    public void NativeSelect_Disabled_Has_Disabled_Attribute()
    {
        var cut = Render<NativeSelectCn>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent("Options"));
        cut.Find("[data-slot='native-select']").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void NativeSelect_Class_Passthrough()
    {
        var cut = Render<NativeSelectCn>(p => p
            .Add(c => c.Class, "custom-class")
            .AddChildContent("Options"));
        cut.Find("[data-slot='native-select-wrapper']").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void NativeSelect_ValueChanged_Fires_On_Change()
    {
        string? changedValue = null;
        var cut = Render<NativeSelectCn>(p => p
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => changedValue = v))
            .AddChildContent("Options"));
        cut.Find("[data-slot='native-select']").Change("apple");
        changedValue.Should().Be("apple");
    }

    // --- NativeSelectOptionCn ---

    [Fact]
    public void NativeSelectOption_Renders_With_DataSlot()
    {
        var cut = Render<NativeSelectCn>(p => p
            .AddChildContent<NativeSelectOptionCn>(o => o
                .AddChildContent("Apple")));
        cut.Find("[data-slot='native-select-option']").Should().NotBeNull();
    }

    [Fact]
    public void NativeSelectOption_Is_Option_Element()
    {
        var cut = Render<NativeSelectCn>(p => p
            .AddChildContent<NativeSelectOptionCn>(o => o
                .AddChildContent("Apple")));
        cut.Find("[data-slot='native-select-option']").TagName.Should().Be("OPTION");
    }

    // --- NativeSelectOptGroupCn ---

    [Fact]
    public void NativeSelectOptGroup_Renders_With_DataSlot()
    {
        var cut = Render<NativeSelectCn>(p => p
            .AddChildContent<NativeSelectOptGroupCn>(g => g
                .Add(c => c.Label, "Fruits")
                .AddChildContent("Options")));
        cut.Find("[data-slot='native-select-optgroup']").Should().NotBeNull();
    }

    [Fact]
    public void NativeSelectOptGroup_Has_Label_Attribute()
    {
        var cut = Render<NativeSelectCn>(p => p
            .AddChildContent<NativeSelectOptGroupCn>(g => g
                .Add(c => c.Label, "Fruits")
                .AddChildContent("Options")));
        cut.Find("[data-slot='native-select-optgroup']").GetAttribute("label").Should().Be("Fruits");
    }

    [Fact]
    public void NativeSelectOptGroup_Is_OptGroup_Element()
    {
        var cut = Render<NativeSelectCn>(p => p
            .AddChildContent<NativeSelectOptGroupCn>(g => g
                .Add(c => c.Label, "Fruits")
                .AddChildContent("Options")));
        cut.Find("[data-slot='native-select-optgroup']").TagName.Should().Be("OPTGROUP");
    }
}
