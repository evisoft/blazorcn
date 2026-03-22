using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class SliderCnTests : BunitContext
{
    [Fact]
    public void Renders_With_Default_Value()
    {
        var cut = Render<SliderCn>();
        var slider = cut.Find("[data-slot='slider']");
        slider.Should().NotBeNull();
    }

    [Fact]
    public void Has_Track_And_Thumb_Elements()
    {
        var cut = Render<SliderCn>();
        var track = cut.Find("[data-slot='slider-track']");
        track.Should().NotBeNull();
        var thumb = cut.Find("[data-slot='slider-thumb']");
        thumb.Should().NotBeNull();
    }

    [Fact]
    public void Has_DataSlot_Slider()
    {
        var cut = Render<SliderCn>();
        var slider = cut.Find("[data-slot='slider']");
        slider.GetAttribute("data-slot").Should().Be("slider");
    }

    [Fact]
    public void Has_Range_Element()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Value, 50));
        var range = cut.Find("[data-slot='slider-range']");
        range.Should().NotBeNull();
    }

    [Fact]
    public void Custom_Class_Is_Passed_Through()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Class, "my-slider"));
        var slider = cut.Find("[data-slot='slider']");
        slider.ClassList.Should().Contain("my-slider");
    }
}
