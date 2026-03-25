using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class SpinnerCnTests : BunitContext
{
    [Fact]
    public void Spinner_Renders_With_DataSlot()
    {
        var cut = Render<SpinnerCn>();
        cut.Find("[data-slot='spinner']").Should().NotBeNull();
    }

    [Fact]
    public void Spinner_Has_Default_Classes()
    {
        var cut = Render<SpinnerCn>();
        var el = cut.Find("[data-slot='spinner']");
        el.ClassList.Should().Contain("animate-spin");
        el.ClassList.Should().Contain("size-4");
    }

    [Fact]
    public void Spinner_Has_Role_Status()
    {
        var cut = Render<SpinnerCn>();
        cut.Find("[data-slot='spinner']").GetAttribute("role").Should().Be("status");
    }

    [Fact]
    public void Spinner_Has_AriaLabel_Loading()
    {
        var cut = Render<SpinnerCn>();
        cut.Find("[data-slot='spinner']").GetAttribute("aria-label").Should().Be("Loading");
    }

    [Fact]
    public void Spinner_Is_Svg_Element()
    {
        var cut = Render<SpinnerCn>();
        cut.Find("svg[data-slot='spinner']").Should().NotBeNull();
    }

    [Fact]
    public void Spinner_Class_Passthrough()
    {
        var cut = Render<SpinnerCn>(p => p
            .Add(c => c.Class, "size-8 text-blue-500"));
        cut.Find("[data-slot='spinner']").ClassList.Should().Contain("size-8");
    }
}
