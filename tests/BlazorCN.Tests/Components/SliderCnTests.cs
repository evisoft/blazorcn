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

    // --- Orientation ---

    [Fact]
    public void Default_Orientation_Is_Horizontal()
    {
        var cut = Render<SliderCn>();
        var slider = cut.Find("[data-slot='slider']");
        slider.GetAttribute("data-orientation").Should().Be("horizontal");
    }

    [Fact]
    public void Vertical_Orientation_Sets_DataAttribute()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Orientation, SliderOrientation.Vertical));
        var slider = cut.Find("[data-slot='slider']");
        slider.GetAttribute("data-orientation").Should().Be("vertical");
    }

    [Fact]
    public void Vertical_Orientation_Adds_Flex_Col_Class()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Orientation, SliderOrientation.Vertical));
        var slider = cut.Find("[data-slot='slider']");
        slider.ClassList.Should().Contain("flex-col");
    }

    [Fact]
    public void Track_Has_Orientation_DataAttribute()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Orientation, SliderOrientation.Vertical));
        var track = cut.Find("[data-slot='slider-track']");
        track.GetAttribute("data-orientation").Should().Be("vertical");
    }

    // --- Range / Multiple Values ---

    [Fact]
    public void Range_Values_Renders_Two_Thumbs()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Values, new[] { 25.0, 75.0 }));
        var thumbs = cut.FindAll("[data-slot='slider-thumb']");
        thumbs.Should().HaveCount(2);
    }

    [Fact]
    public void Multiple_Values_Renders_Three_Thumbs()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Values, new[] { 10.0, 50.0, 90.0 }));
        var thumbs = cut.FindAll("[data-slot='slider-thumb']");
        thumbs.Should().HaveCount(3);
    }

    [Fact]
    public void Single_Value_Renders_One_Thumb()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Value, 50));
        var thumbs = cut.FindAll("[data-slot='slider-thumb']");
        thumbs.Should().HaveCount(1);
    }

    [Fact]
    public void Range_Values_Renders_Two_Inputs()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Values, new[] { 25.0, 75.0 }));
        var inputs = cut.FindAll("input[type='range']");
        inputs.Should().HaveCount(2);
    }

    [Fact]
    public void Disabled_Adds_Opacity_Class()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Disabled, true));
        var slider = cut.Find("[data-slot='slider']");
        slider.ClassList.Should().Contain("opacity-50");
    }
}
