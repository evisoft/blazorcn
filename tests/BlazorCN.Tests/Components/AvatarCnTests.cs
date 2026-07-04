using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class AvatarCnTests : BunitContext
{
    // --- AvatarCn ---

    [Fact]
    public void Avatar_Renders_With_DataSlot()
    {
        var cut = Render<AvatarCn>(p => p.AddChildContent("AB"));
        cut.Find("[data-slot='avatar']").Should().NotBeNull();
    }

    [Fact]
    public void Avatar_Has_Default_Classes()
    {
        var cut = Render<AvatarCn>(p => p.AddChildContent("AB"));
        var el = cut.Find("[data-slot='avatar']");
        el.ClassList.Should().Contain("cn-avatar");
        el.ClassList.Should().Contain("group/avatar");
        el.ClassList.Should().Contain("relative");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("shrink-0");
        el.ClassList.Should().Contain("select-none");
        el.ClassList.Should().Contain("overflow-hidden");
    }

    [Fact]
    public void Avatar_Has_Size_Data_Attributes()
    {
        var cut = Render<AvatarCn>(p => p.AddChildContent("AB"));
        var el = cut.Find("[data-slot='avatar']");
        el.ClassList.Should().Contain("cn-avatar");
    }

    [Fact]
    public void Avatar_Class_Passthrough()
    {
        var cut = Render<AvatarCn>(p => p
            .Add(c => c.Class, "custom-avatar")
            .AddChildContent("AB"));
        cut.Find("[data-slot='avatar']").ClassList.Should().Contain("custom-avatar");
    }

    [Fact]
    public void Avatar_AdditionalAttributes_Passthrough()
    {
        var cut = Render<AvatarCn>(p => p
            .AddUnmatched("data-testid", "avatar-1")
            .AddChildContent("AB"));
        cut.Find("[data-slot='avatar']").GetAttribute("data-testid").Should().Be("avatar-1");
    }

    // --- AvatarFallbackCn ---

    [Fact]
    public void AvatarFallback_Renders_With_DataSlot()
    {
        var cut = Render<AvatarCn>(p => p
            .AddChildContent<AvatarFallbackCn>(f => f.AddChildContent("AB")));
        cut.Find("[data-slot='avatar-fallback']").Should().NotBeNull();
    }

    [Fact]
    public void AvatarFallback_Has_Default_Classes()
    {
        var cut = Render<AvatarCn>(p => p
            .AddChildContent<AvatarFallbackCn>(f => f.AddChildContent("AB")));
        var el = cut.Find("[data-slot='avatar-fallback']");
        el.ClassList.Should().Contain("cn-avatar-fallback");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("size-full");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("justify-center");
        el.ClassList.Should().Contain("text-sm");
    }

    [Fact]
    public void AvatarFallback_Has_SmallSize_TextXs()
    {
        var cut = Render<AvatarCn>(p => p
            .AddChildContent<AvatarFallbackCn>(f => f.AddChildContent("AB")));
        var el = cut.Find("[data-slot='avatar-fallback']");
        el.ClassList.Should().Contain("group-data-[size=sm]/avatar:text-xs");
    }

    // --- AvatarImageCn ---

    [Fact]
    public void AvatarImage_Renders_With_DataSlot()
    {
        var cut = Render<AvatarCn>(p => p
            .AddChildContent<AvatarImageCn>(i => i
                .Add(c => c.Src, "/img.png")
                .Add(c => c.Alt, "Test")));
        cut.Find("[data-slot='avatar-image']").Should().NotBeNull();
    }

    // --- AvatarBadgeCn ---

    [Fact]
    public void AvatarBadge_Renders_With_DataSlot()
    {
        var cut = Render<AvatarCn>(p => p
            .AddChildContent<AvatarBadgeCn>());
        cut.Find("[data-slot='avatar-badge']").Should().NotBeNull();
    }

    [Fact]
    public void AvatarBadge_Has_Default_Classes()
    {
        var cut = Render<AvatarCn>(p => p
            .AddChildContent<AvatarBadgeCn>());
        var el = cut.Find("[data-slot='avatar-badge']");
        el.ClassList.Should().Contain("cn-avatar-badge");
        el.ClassList.Should().Contain("absolute");
        el.ClassList.Should().Contain("bottom-0");
        el.ClassList.Should().Contain("right-0");
        el.ClassList.Should().Contain("rounded-full");
        // Size is now driven by the parent avatar's data-size group variants; the ring
        // (formerly border-2) lives in the cn-avatar-badge CSS class.
        el.ClassList.Should().Contain("group-data-[size=default]/avatar:size-2.5");
    }

    [Fact]
    public void AvatarBadge_Class_Passthrough()
    {
        var cut = Render<AvatarCn>(p => p
            .AddChildContent<AvatarBadgeCn>(b => b
                .Add(c => c.Class, "bg-green-500")));
        cut.Find("[data-slot='avatar-badge']").ClassList.Should().Contain("bg-green-500");
    }

    // --- AvatarGroupCn ---

    [Fact]
    public void AvatarGroup_Renders_With_DataSlot()
    {
        var cut = Render<AvatarGroupCn>(p => p.AddChildContent("Avatars"));
        cut.Find("[data-slot='avatar-group']").Should().NotBeNull();
    }

    [Fact]
    public void AvatarGroup_Has_Default_Classes()
    {
        var cut = Render<AvatarGroupCn>(p => p.AddChildContent("Avatars"));
        var el = cut.Find("[data-slot='avatar-group']");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("-space-x-2");
    }

    [Fact]
    public void AvatarGroup_Class_Passthrough()
    {
        var cut = Render<AvatarGroupCn>(p => p
            .Add(c => c.Class, "custom-group")
            .AddChildContent("Avatars"));
        cut.Find("[data-slot='avatar-group']").ClassList.Should().Contain("custom-group");
    }

    // --- AvatarGroupCountCn ---

    [Fact]
    public void AvatarGroupCount_Renders_With_DataSlot()
    {
        var cut = Render<AvatarGroupCountCn>(p => p.Add(c => c.Count, 5));
        cut.Find("[data-slot='avatar-group-count']").Should().NotBeNull();
    }

    [Fact]
    public void AvatarGroupCount_Displays_Count()
    {
        var cut = Render<AvatarGroupCountCn>(p => p.Add(c => c.Count, 3));
        cut.Find("[data-slot='avatar-group-count']").TextContent.Should().Contain("+3");
    }

    [Fact]
    public void AvatarGroupCount_Has_Default_Classes()
    {
        var cut = Render<AvatarGroupCountCn>(p => p.Add(c => c.Count, 5));
        var el = cut.Find("[data-slot='avatar-group-count']");
        el.ClassList.Should().Contain("cn-avatar-group-count");
        el.ClassList.Should().Contain("relative");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("shrink-0");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("justify-center");
        // Reference now uses a ring instead of a border.
        el.ClassList.Should().Contain("ring-2");
        el.ClassList.Should().Contain("ring-background");
    }

    [Fact]
    public void AvatarGroupCount_Class_Passthrough()
    {
        var cut = Render<AvatarGroupCountCn>(p => p
            .Add(c => c.Count, 5)
            .Add(c => c.Class, "custom-count"));
        cut.Find("[data-slot='avatar-group-count']").ClassList.Should().Contain("custom-count");
    }
}
