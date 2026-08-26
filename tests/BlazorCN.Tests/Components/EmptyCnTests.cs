using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class EmptyCnTests : BunitContext
{
    // --- EmptyCn ---

    [Fact]
    public void Empty_Renders_With_DataSlot()
    {
        var cut = Render<EmptyCn>(p => p.AddChildContent("No data"));
        cut.Find("[data-slot='empty']").Should().NotBeNull();
    }

    [Fact]
    public void Empty_Has_Default_Classes()
    {
        var cut = Render<EmptyCn>(p => p.AddChildContent("No data"));
        var el = cut.Find("[data-slot='empty']");
        el.ClassList.Should().Contain("cn-empty");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("min-w-0");
        el.ClassList.Should().Contain("flex-col");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("justify-center");
        el.ClassList.Should().Contain("text-center");
        el.ClassList.Should().Contain("text-balance");
    }

    [Fact]
    public void Empty_Class_Passthrough()
    {
        var cut = Render<EmptyCn>(p => p
            .Add(c => c.Class, "custom-empty")
            .AddChildContent("No data"));
        cut.Find("[data-slot='empty']").ClassList.Should().Contain("custom-empty");
    }

    [Fact]
    public void Empty_AdditionalAttributes_Passthrough()
    {
        var cut = Render<EmptyCn>(p => p
            .AddUnmatched("data-testid", "empty-1")
            .AddChildContent("No data"));
        cut.Find("[data-slot='empty']").GetAttribute("data-testid").Should().Be("empty-1");
    }

    // --- EmptyHeaderCn ---

    [Fact]
    public void EmptyHeader_Renders_With_DataSlot()
    {
        var cut = Render<EmptyHeaderCn>(p => p.AddChildContent("Header"));
        cut.Find("[data-slot='empty-header']").Should().NotBeNull();
    }

    [Fact]
    public void EmptyHeader_Has_Default_Classes()
    {
        var cut = Render<EmptyHeaderCn>(p => p.AddChildContent("Header"));
        var el = cut.Find("[data-slot='empty-header']");
        el.ClassList.Should().Contain("cn-empty-header");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("max-w-sm");
        el.ClassList.Should().Contain("flex-col");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("text-center");
    }

    // --- EmptyMediaCn ---

    [Fact]
    public void EmptyMedia_Renders_With_DataSlot()
    {
        var cut = Render<EmptyMediaCn>(p => p.AddChildContent("Icon"));
        cut.Find("[data-slot='empty-icon']").Should().NotBeNull();
    }

    [Fact]
    public void EmptyMedia_Icon_Variant_Has_Default_Classes()
    {
        var cut = Render<EmptyMediaCn>(p => p
            .Add(c => c.Variant, EmptyMediaVariant.Icon)
            .AddChildContent("Icon"));
        var el = cut.Find("[data-slot='empty-icon']");
        el.GetAttribute("data-variant").Should().Be("icon");
        el.ClassList.Should().Contain("cn-empty-media");
        el.ClassList.Should().Contain("cn-empty-media-icon");
    }

    [Fact]
    public void EmptyMedia_Default_Variant_Has_Minimal_Classes()
    {
        var cut = Render<EmptyMediaCn>(p => p
            .Add(c => c.Variant, EmptyMediaVariant.Default)
            .AddChildContent("Image"));
        var el = cut.Find("[data-slot='empty-icon']");
        el.GetAttribute("data-variant").Should().Be("default");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("justify-center");
    }

    // --- EmptyTitleCn ---

    [Fact]
    public void EmptyTitle_Renders_With_DataSlot()
    {
        var cut = Render<EmptyTitleCn>(p => p.AddChildContent("Title"));
        cut.Find("[data-slot='empty-title']").Should().NotBeNull();
    }

    [Fact]
    public void EmptyTitle_Has_Default_Classes()
    {
        var cut = Render<EmptyTitleCn>(p => p.AddChildContent("Title"));
        var el = cut.Find("[data-slot='empty-title']");
        el.ClassList.Should().Contain("cn-empty-title");
    }

    [Fact]
    public void EmptyTitle_Renders_As_Div()
    {
        var cut = Render<EmptyTitleCn>(p => p.AddChildContent("Title"));
        cut.Find("[data-slot='empty-title']").TagName.Should().Be("DIV");
    }

    // --- EmptyDescriptionCn ---

    [Fact]
    public void EmptyDescription_Renders_With_DataSlot()
    {
        var cut = Render<EmptyDescriptionCn>(p => p.AddChildContent("Description"));
        cut.Find("[data-slot='empty-description']").Should().NotBeNull();
    }

    [Fact]
    public void EmptyDescription_Has_Default_Classes()
    {
        var cut = Render<EmptyDescriptionCn>(p => p.AddChildContent("Description"));
        var el = cut.Find("[data-slot='empty-description']");
        el.ClassList.Should().Contain("cn-empty-description");
        el.ClassList.Should().Contain("text-muted-foreground");
    }

    [Fact]
    public void EmptyDescription_Renders_As_Div()
    {
        var cut = Render<EmptyDescriptionCn>(p => p.AddChildContent("Description"));
        cut.Find("[data-slot='empty-description']").TagName.Should().Be("DIV");
    }

    // --- EmptyContentCn ---

    [Fact]
    public void EmptyContent_Renders_With_DataSlot()
    {
        var cut = Render<EmptyContentCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='empty-content']").Should().NotBeNull();
    }

    [Fact]
    public void EmptyContent_Has_Default_Classes()
    {
        var cut = Render<EmptyContentCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='empty-content']");
        el.ClassList.Should().Contain("cn-empty-content");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("w-full");
        el.ClassList.Should().Contain("max-w-sm");
        el.ClassList.Should().Contain("min-w-0");
        el.ClassList.Should().Contain("flex-col");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("text-balance");
    }

    [Fact]
    public void EmptyContent_Class_Passthrough()
    {
        var cut = Render<EmptyContentCn>(p => p
            .Add(c => c.Class, "custom-content")
            .AddChildContent("Content"));
        cut.Find("[data-slot='empty-content']").ClassList.Should().Contain("custom-content");
    }
}
