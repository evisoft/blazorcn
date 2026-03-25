using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class ItemCnTests : BunitContext
{
    [Fact]
    public void ItemCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<ItemCn>(p => p.AddChildContent("Item content"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("item");
    }

    [Fact]
    public void ItemCn_Default_Variant_Renders_Correct_Classes()
    {
        var cut = Render<ItemCn>(p => p.AddChildContent("Item"));
        var div = cut.Find("div");
        div.GetAttribute("data-variant").Should().Be("default");
        div.ClassList.Should().Contain("cn-item");
        div.ClassList.Should().Contain("cn-item-variant-default");
    }

    [Fact]
    public void ItemCn_Outline_Variant_Renders_Correct_Classes()
    {
        var cut = Render<ItemCn>(p => p
            .Add(c => c.Variant, ItemVariant.Outline)
            .AddChildContent("Item"));
        var div = cut.Find("div");
        div.GetAttribute("data-variant").Should().Be("outline");
        div.ClassList.Should().Contain("cn-item-variant-outline");
    }

    [Fact]
    public void ItemCn_Muted_Variant_Renders_Correct_Classes()
    {
        var cut = Render<ItemCn>(p => p
            .Add(c => c.Variant, ItemVariant.Muted)
            .AddChildContent("Item"));
        var div = cut.Find("div");
        div.GetAttribute("data-variant").Should().Be("muted");
        div.ClassList.Should().Contain("cn-item-variant-muted");
    }

    [Fact]
    public void ItemCn_Default_Size_Renders_Correct_DataAttribute()
    {
        var cut = Render<ItemCn>(p => p.AddChildContent("Item"));
        var div = cut.Find("div");
        div.GetAttribute("data-size").Should().Be("default");
    }

    [Fact]
    public void ItemCn_Sm_Size_Renders_Correct_DataAttribute()
    {
        var cut = Render<ItemCn>(p => p
            .Add(c => c.Size, ItemSize.Sm)
            .AddChildContent("Item"));
        var div = cut.Find("div");
        div.GetAttribute("data-size").Should().Be("sm");
    }

    [Fact]
    public void ItemCn_Xs_Size_Renders_Correct_DataAttribute()
    {
        var cut = Render<ItemCn>(p => p
            .Add(c => c.Size, ItemSize.Xs)
            .AddChildContent("Item"));
        var div = cut.Find("div");
        div.GetAttribute("data-size").Should().Be("xs");
    }

    [Fact]
    public void ItemGroupCn_Renders_With_Correct_DataSlot_And_Role()
    {
        var cut = Render<ItemGroupCn>(p => p.AddChildContent("Group"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("item-group");
        div.GetAttribute("role").Should().Be("list");
        div.ClassList.Should().Contain("flex-col");
    }

    [Fact]
    public void ItemMediaCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<ItemMediaCn>(p => p.AddChildContent("Media"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("item-media");
        div.ClassList.Should().Contain("shrink-0");
    }

    [Fact]
    public void ItemMediaCn_Icon_Variant_Renders_Correct_DataAttribute()
    {
        var cut = Render<ItemMediaCn>(p => p
            .Add(c => c.Variant, ItemMediaVariant.Icon)
            .AddChildContent("Icon"));
        var div = cut.Find("div");
        div.GetAttribute("data-variant").Should().Be("icon");
    }

    [Fact]
    public void ItemMediaCn_Image_Variant_Renders_Correct_DataAttribute()
    {
        var cut = Render<ItemMediaCn>(p => p
            .Add(c => c.Variant, ItemMediaVariant.Image)
            .AddChildContent("Img"));
        var div = cut.Find("div");
        div.GetAttribute("data-variant").Should().Be("image");
        div.ClassList.Should().Contain("overflow-hidden");
    }

    [Fact]
    public void ItemContentCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<ItemContentCn>(p => p.AddChildContent("Content"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("item-content");
        div.ClassList.Should().Contain("flex-col");
    }

    [Fact]
    public void ItemTitleCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<ItemTitleCn>(p => p.AddChildContent("Title"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("item-title");
        div.ClassList.Should().Contain("cn-item-title");
    }

    [Fact]
    public void ItemDescriptionCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<ItemDescriptionCn>(p => p.AddChildContent("Description"));
        var el = cut.Find("p");
        el.GetAttribute("data-slot").Should().Be("item-description");
        el.ClassList.Should().Contain("cn-item-description");
    }

    [Fact]
    public void ItemHeaderCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<ItemHeaderCn>(p => p.AddChildContent("Header"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("item-header");
        div.ClassList.Should().Contain("justify-between");
    }

    [Fact]
    public void ItemFooterCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<ItemFooterCn>(p => p.AddChildContent("Footer"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("item-footer");
        div.ClassList.Should().Contain("justify-between");
    }

    [Fact]
    public void ItemActionsCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<ItemActionsCn>(p => p.AddChildContent("Actions"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("item-actions");
        div.ClassList.Should().Contain("items-center");
    }

    [Fact]
    public void ItemCn_Custom_Class_Passed_Through()
    {
        var cut = Render<ItemCn>(p => p
            .Add(c => c.Class, "my-custom-class")
            .AddChildContent("Content"));
        var div = cut.Find("div");
        div.ClassList.Should().Contain("my-custom-class");
    }

    [Fact]
    public void ItemCn_Additional_Attributes_Passed_Through()
    {
        var cut = Render<ItemCn>(p => p
            .AddUnmatched("data-testid", "item-1")
            .AddChildContent("Content"));
        var div = cut.Find("div");
        div.GetAttribute("data-testid").Should().Be("item-1");
    }
}
