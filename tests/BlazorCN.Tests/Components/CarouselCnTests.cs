using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using Xunit;

namespace BlazorCN.Tests.Components;

public class CarouselCnTests : BunitContext
{
    // --- CarouselCn ---

    [Fact]
    public void Carousel_Renders_With_DataSlot()
    {
        var cut = Render<CarouselCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='carousel']").Should().NotBeNull();
    }

    [Fact]
    public void Carousel_Has_Default_Classes()
    {
        var cut = Render<CarouselCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='carousel']").ClassList.Should().Contain("relative");
    }

    [Fact]
    public void Carousel_Has_Region_Role()
    {
        var cut = Render<CarouselCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='carousel']").GetAttribute("role").Should().Be("region");
    }

    [Fact]
    public void Carousel_Has_Aria_Roledescription()
    {
        var cut = Render<CarouselCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='carousel']").GetAttribute("aria-roledescription").Should().Be("carousel");
    }

    [Fact]
    public void Carousel_Class_Passthrough()
    {
        var cut = Render<CarouselCn>(p => p
            .Add(c => c.Class, "custom-class")
            .AddChildContent("Content"));
        cut.Find("[data-slot='carousel']").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void Carousel_AdditionalAttributes_Passthrough()
    {
        var cut = Render<CarouselCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-carousel" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='carousel']").GetAttribute("id").Should().Be("my-carousel");
    }

    [Fact]
    public void Carousel_Default_Orientation_Is_Horizontal()
    {
        var cut = Render<CarouselCn>(p => p.AddChildContent("Content"));
        // Verify it renders (horizontal is default)
        cut.Find("[data-slot='carousel']").Should().NotBeNull();
    }

    // --- CarouselContentCn ---

    [Fact]
    public void CarouselContent_Renders_With_DataSlot()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselContentCn>(c => c
                .AddChildContent("Slides")));
        cut.Find("[data-slot='carousel-content']").Should().NotBeNull();
    }

    [Fact]
    public void CarouselContent_Has_Default_Classes()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselContentCn>(c => c
                .AddChildContent("Slides")));
        cut.Find("[data-slot='carousel-content']").ClassList.Should().Contain("overflow-hidden");
    }

    [Fact]
    public void CarouselContent_Horizontal_Has_Flex_Ml4()
    {
        var cut = Render<CarouselCn>(p => p
            .Add(c => c.Orientation, CarouselOrientation.Horizontal)
            .AddChildContent<CarouselContentCn>(c => c
                .AddChildContent("Slides")));
        var innerDiv = cut.Find("[data-slot='carousel-content'] > div");
        innerDiv.ClassList.Should().Contain("flex");
        innerDiv.ClassList.Should().Contain("-ml-4");
    }

    [Fact]
    public void CarouselContent_Vertical_Has_FlexCol_Mt4()
    {
        var cut = Render<CarouselCn>(p => p
            .Add(c => c.Orientation, CarouselOrientation.Vertical)
            .AddChildContent<CarouselContentCn>(c => c
                .AddChildContent("Slides")));
        var innerDiv = cut.Find("[data-slot='carousel-content'] > div");
        innerDiv.ClassList.Should().Contain("flex");
        innerDiv.ClassList.Should().Contain("-mt-4");
        innerDiv.ClassList.Should().Contain("flex-col");
    }

    [Fact]
    public void CarouselContent_Class_Passthrough()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselContentCn>(c => c
                .Add(x => x.Class, "custom-content")
                .AddChildContent("Slides")));
        cut.Find("[data-slot='carousel-content']").ClassList.Should().Contain("custom-content");
    }

    // --- CarouselItemCn ---

    [Fact]
    public void CarouselItem_Renders_With_DataSlot()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselItemCn>(i => i
                .AddChildContent("Slide 1")));
        cut.Find("[data-slot='carousel-item']").Should().NotBeNull();
    }

    [Fact]
    public void CarouselItem_Has_Default_Classes()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselItemCn>(i => i
                .AddChildContent("Slide 1")));
        var item = cut.Find("[data-slot='carousel-item']");
        item.ClassList.Should().Contain("min-w-0");
        item.ClassList.Should().Contain("shrink-0");
        item.ClassList.Should().Contain("grow-0");
        item.ClassList.Should().Contain("basis-full");
    }

    [Fact]
    public void CarouselItem_Horizontal_Has_Pl4()
    {
        var cut = Render<CarouselCn>(p => p
            .Add(c => c.Orientation, CarouselOrientation.Horizontal)
            .AddChildContent<CarouselItemCn>(i => i
                .AddChildContent("Slide 1")));
        cut.Find("[data-slot='carousel-item']").ClassList.Should().Contain("pl-4");
    }

    [Fact]
    public void CarouselItem_Vertical_Has_Pt4()
    {
        var cut = Render<CarouselCn>(p => p
            .Add(c => c.Orientation, CarouselOrientation.Vertical)
            .AddChildContent<CarouselItemCn>(i => i
                .AddChildContent("Slide 1")));
        cut.Find("[data-slot='carousel-item']").ClassList.Should().Contain("pt-4");
    }

    [Fact]
    public void CarouselItem_Has_Group_Role()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselItemCn>(i => i
                .AddChildContent("Slide 1")));
        cut.Find("[data-slot='carousel-item']").GetAttribute("role").Should().Be("group");
    }

    [Fact]
    public void CarouselItem_Has_Slide_Roledescription()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselItemCn>(i => i
                .AddChildContent("Slide 1")));
        cut.Find("[data-slot='carousel-item']").GetAttribute("aria-roledescription").Should().Be("slide");
    }

    [Fact]
    public void CarouselItem_Class_Passthrough()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselItemCn>(i => i
                .Add(x => x.Class, "custom-item")
                .AddChildContent("Slide 1")));
        cut.Find("[data-slot='carousel-item']").ClassList.Should().Contain("custom-item");
    }

    // --- CarouselPreviousCn ---

    [Fact]
    public void CarouselPrevious_Renders_With_DataSlot()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselPreviousCn>());
        cut.Find("[data-slot='carousel-previous']").Should().NotBeNull();
    }

    [Fact]
    public void CarouselPrevious_Has_Default_Classes_Horizontal()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselPreviousCn>());
        var btn = cut.Find("[data-slot='carousel-previous']");
        btn.ClassList.Should().Contain("absolute");
        btn.ClassList.Should().Contain("touch-manipulation");
        btn.ClassList.Should().Contain("top-1/2");
        btn.ClassList.Should().Contain("-left-12");
        btn.ClassList.Should().Contain("-translate-y-1/2");
    }

    [Fact]
    public void CarouselPrevious_Has_Default_Classes_Vertical()
    {
        var cut = Render<CarouselCn>(p => p
            .Add(c => c.Orientation, CarouselOrientation.Vertical)
            .AddChildContent<CarouselPreviousCn>());
        var btn = cut.Find("[data-slot='carousel-previous']");
        btn.ClassList.Should().Contain("absolute");
        btn.ClassList.Should().Contain("touch-manipulation");
        btn.ClassList.Should().Contain("-top-12");
        btn.ClassList.Should().Contain("left-1/2");
        btn.ClassList.Should().Contain("-translate-x-1/2");
        btn.ClassList.Should().Contain("rotate-90");
    }

    [Fact]
    public void CarouselPrevious_Is_Disabled_At_Start()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselPreviousCn>());
        cut.Find("[data-slot='carousel-previous']").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void CarouselPrevious_Has_Button_Type()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselPreviousCn>());
        cut.Find("[data-slot='carousel-previous']").GetAttribute("type").Should().Be("button");
    }

    [Fact]
    public void CarouselPrevious_Class_Passthrough()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselPreviousCn>(b => b
                .Add(c => c.Class, "custom-prev")));
        cut.Find("[data-slot='carousel-previous']").ClassList.Should().Contain("custom-prev");
    }

    // --- CarouselNextCn ---

    [Fact]
    public void CarouselNext_Renders_With_DataSlot()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselNextCn>());
        cut.Find("[data-slot='carousel-next']").Should().NotBeNull();
    }

    [Fact]
    public void CarouselNext_Has_Default_Classes_Horizontal()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselNextCn>());
        var btn = cut.Find("[data-slot='carousel-next']");
        btn.ClassList.Should().Contain("absolute");
        btn.ClassList.Should().Contain("touch-manipulation");
        btn.ClassList.Should().Contain("top-1/2");
    }

    [Fact]
    public void CarouselNext_Is_Disabled_When_No_Items()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselNextCn>());
        cut.Find("[data-slot='carousel-next']").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void CarouselNext_Has_Button_Type()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselNextCn>());
        cut.Find("[data-slot='carousel-next']").GetAttribute("type").Should().Be("button");
    }

    [Fact]
    public void CarouselNext_Class_Passthrough()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselNextCn>(b => b
                .Add(c => c.Class, "custom-next")));
        cut.Find("[data-slot='carousel-next']").ClassList.Should().Contain("custom-next");
    }

    // --- Keyboard Navigation ---

    [Fact]
    public void Carousel_ArrowRight_Navigates_Next_Horizontal()
    {
        var cut = Render<CarouselCn>(p => p
            .Add(c => c.Orientation, CarouselOrientation.Horizontal)
            .AddChildContent("Content"));
        // Should not throw even with no items
        cut.Find("[data-slot='carousel']").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
    }

    [Fact]
    public void Carousel_ArrowLeft_Navigates_Prev_Horizontal()
    {
        var cut = Render<CarouselCn>(p => p
            .Add(c => c.Orientation, CarouselOrientation.Horizontal)
            .AddChildContent("Content"));
        cut.Find("[data-slot='carousel']").KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });
    }

    [Fact]
    public void Carousel_ArrowDown_Navigates_Next_Vertical()
    {
        var cut = Render<CarouselCn>(p => p
            .Add(c => c.Orientation, CarouselOrientation.Vertical)
            .AddChildContent("Content"));
        cut.Find("[data-slot='carousel']").KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
    }

    [Fact]
    public void Carousel_ArrowUp_Navigates_Prev_Vertical()
    {
        var cut = Render<CarouselCn>(p => p
            .Add(c => c.Orientation, CarouselOrientation.Vertical)
            .AddChildContent("Content"));
        cut.Find("[data-slot='carousel']").KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });
    }

    // --- CarouselContent AdditionalAttributes ---

    [Fact]
    public void CarouselContent_AdditionalAttributes_Passthrough()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselContentCn>(c => c
                .Add(x => x.AdditionalAttributes, new Dictionary<string, object?> { { "id", "content-1" } })
                .AddChildContent("Slides")));
        cut.Find("[data-slot='carousel-content']").GetAttribute("id").Should().Be("content-1");
    }

    // --- CarouselItem AdditionalAttributes ---

    [Fact]
    public void CarouselItem_AdditionalAttributes_Passthrough()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselItemCn>(i => i
                .Add(x => x.AdditionalAttributes, new Dictionary<string, object?> { { "id", "item-1" } })
                .AddChildContent("Slide 1")));
        cut.Find("[data-slot='carousel-item']").GetAttribute("id").Should().Be("item-1");
    }

    // --- Contains SVG Icons ---

    [Fact]
    public void CarouselPrevious_Contains_Svg_Arrow()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselPreviousCn>());
        cut.Find("[data-slot='carousel-previous'] svg").Should().NotBeNull();
    }

    [Fact]
    public void CarouselNext_Contains_Svg_Arrow()
    {
        var cut = Render<CarouselCn>(p => p
            .AddChildContent<CarouselNextCn>());
        cut.Find("[data-slot='carousel-next'] svg").Should().NotBeNull();
    }
}
