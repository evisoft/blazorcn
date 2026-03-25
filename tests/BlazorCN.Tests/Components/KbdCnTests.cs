using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class KbdCnTests : BunitContext
{
    [Fact]
    public void Kbd_Renders_With_DataSlot()
    {
        var cut = Render<KbdCn>(p => p.AddChildContent("Ctrl"));
        cut.Find("[data-slot='kbd']").Should().NotBeNull();
    }

    [Fact]
    public void Kbd_Has_Default_Classes()
    {
        var cut = Render<KbdCn>(p => p.AddChildContent("Ctrl"));
        var el = cut.Find("[data-slot='kbd']");
        el.ClassList.Should().Contain("cn-kbd");
        el.ClassList.Should().Contain("pointer-events-none");
        el.ClassList.Should().Contain("inline-flex");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("justify-center");
        el.ClassList.Should().Contain("select-none");
    }

    [Fact]
    public void Kbd_Renders_As_Kbd_Element()
    {
        var cut = Render<KbdCn>(p => p.AddChildContent("K"));
        cut.Find("kbd").Should().NotBeNull();
    }

    [Fact]
    public void Kbd_Class_Passthrough()
    {
        var cut = Render<KbdCn>(p => p
            .Add(c => c.Class, "custom-kbd")
            .AddChildContent("K"));
        cut.Find("[data-slot='kbd']").ClassList.Should().Contain("custom-kbd");
    }

    [Fact]
    public void Kbd_AdditionalAttributes_Passthrough()
    {
        var cut = Render<KbdCn>(p => p
            .AddUnmatched("data-testid", "kbd-1")
            .AddChildContent("K"));
        cut.Find("[data-slot='kbd']").GetAttribute("data-testid").Should().Be("kbd-1");
    }
}
