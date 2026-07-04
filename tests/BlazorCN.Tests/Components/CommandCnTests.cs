using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class CommandCnTests : BunitContext
{
    public CommandCnTests()
    {
        // CommandCn/CommandItemCn inject JsInteropCn (keyboard nav + filter-text capture).
        // Loose mode lets those interop calls no-op, and registering the service satisfies [Inject].
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddScoped<JsInteropCn>();
    }

    // --- CommandCn ---

    [Fact]
    public void Command_Renders_With_DataSlot()
    {
        var cut = Render<CommandCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='command']").Should().NotBeNull();
    }

    [Fact]
    public void Command_Has_Default_Classes()
    {
        var cut = Render<CommandCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='command']");
        el.ClassList.Should().Contain("cn-command");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("h-full");
        el.ClassList.Should().Contain("w-full");
        el.ClassList.Should().Contain("flex-col");
        el.ClassList.Should().Contain("overflow-hidden");
        el.ClassList.Should().Contain("border");
    }

    [Fact]
    public void Command_Class_Passthrough()
    {
        var cut = Render<CommandCn>(p => p
            .Add(c => c.Class, "custom-class")
            .AddChildContent("Content"));
        cut.Find("[data-slot='command']").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void Command_AdditionalAttributes_Passthrough()
    {
        var cut = Render<CommandCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-cmd" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='command']").GetAttribute("id").Should().Be("my-cmd");
    }

    // --- CommandInputCn ---

    [Fact]
    public void CommandInput_Renders_With_DataSlot()
    {
        var cut = Render<CommandCn>(p => p
            .AddChildContent<CommandInputCn>(i => i
                .Add(c => c.Placeholder, "Search...")));
        cut.Find("[data-slot='command-input']").Should().NotBeNull();
    }

    [Fact]
    public void CommandInput_Contains_Input_Element()
    {
        var cut = Render<CommandCn>(p => p
            .AddChildContent<CommandInputCn>(i => i
                .Add(c => c.Placeholder, "Search...")));
        cut.Find("input[data-slot='command-input']").Should().NotBeNull();
    }

    [Fact]
    public void CommandInput_Has_Default_Classes_On_Input()
    {
        var cut = Render<CommandCn>(p => p
            .AddChildContent<CommandInputCn>(i => i
                .Add(c => c.Placeholder, "Search...")));
        var el = cut.Find("input[data-slot='command-input']");
        el.ClassList.Should().Contain("cn-command-input");
        el.ClassList.Should().Contain("outline-hidden");
    }

    [Fact]
    public void CommandInput_Has_Wrapper_Classes()
    {
        var cut = Render<CommandCn>(p => p
            .AddChildContent<CommandInputCn>(i => i
                .Add(c => c.Placeholder, "Search...")));
        var wrapper = cut.Find("[data-slot='command-input-wrapper']");
        wrapper.ClassList.Should().Contain("cn-command-input-wrapper");
        var group = cut.Find(".cn-command-input-group");
        group.ClassList.Should().Contain("cn-command-input-group");
    }

    [Fact]
    public void CommandInput_Has_Search_Icon()
    {
        var cut = Render<CommandCn>(p => p
            .AddChildContent<CommandInputCn>(i => i
                .Add(c => c.Placeholder, "Search...")));
        cut.FindAll("[data-slot='command-input-wrapper'] svg").Should().NotBeEmpty();
    }

    [Fact]
    public void CommandInput_Has_Placeholder()
    {
        var cut = Render<CommandCn>(p => p
            .AddChildContent<CommandInputCn>(i => i
                .Add(c => c.Placeholder, "Type a command...")));
        cut.Find("input[data-slot='command-input']").GetAttribute("placeholder").Should().Be("Type a command...");
    }

    [Fact]
    public void CommandInput_Fires_ValueChanged_On_Input()
    {
        string inputValue = "";
        var cut = Render<CommandCn>(p => p
            .AddChildContent<CommandInputCn>(i => i
                .Add(c => c.Placeholder, "Search...")
                .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => inputValue = v))));
        cut.Find("input[data-slot='command-input']").Input("test");
        inputValue.Should().Be("test");
    }

    [Fact]
    public void CommandInput_Class_Passthrough()
    {
        var cut = Render<CommandCn>(p => p
            .AddChildContent<CommandInputCn>(i => i
                .Add(c => c.Placeholder, "Search...")
                .Add(c => c.Class, "custom-input")));
        cut.Find("input[data-slot='command-input']").ClassList.Should().Contain("custom-input");
    }

    // --- CommandListCn ---

    [Fact]
    public void CommandList_Renders_With_DataSlot()
    {
        var cut = Render<CommandListCn>(p => p.AddChildContent("Items"));
        cut.Find("[data-slot='command-list']").Should().NotBeNull();
    }

    [Fact]
    public void CommandList_Has_Default_Classes()
    {
        var cut = Render<CommandListCn>(p => p.AddChildContent("Items"));
        var el = cut.Find("[data-slot='command-list']");
        el.ClassList.Should().Contain("cn-command-list");
        el.ClassList.Should().Contain("overflow-y-auto");
        el.ClassList.Should().Contain("overflow-x-hidden");
    }

    [Fact]
    public void CommandList_Class_Passthrough()
    {
        var cut = Render<CommandListCn>(p => p
            .Add(c => c.Class, "custom-list")
            .AddChildContent("Items"));
        cut.Find("[data-slot='command-list']").ClassList.Should().Contain("custom-list");
    }

    // --- CommandEmptyCn ---

    [Fact]
    public void CommandEmpty_Renders_With_DataSlot()
    {
        var cut = Render<CommandEmptyCn>(p => p.AddChildContent("No results"));
        cut.Find("[data-slot='command-empty']").Should().NotBeNull();
    }

    [Fact]
    public void CommandEmpty_Has_Default_Classes()
    {
        var cut = Render<CommandEmptyCn>(p => p.AddChildContent("No results"));
        var el = cut.Find("[data-slot='command-empty']");
        el.ClassList.Should().Contain("cn-command-empty");
    }

    [Fact]
    public void CommandEmpty_Renders_Content()
    {
        var cut = Render<CommandEmptyCn>(p => p.AddChildContent("No results found."));
        cut.Find("[data-slot='command-empty']").TextContent.Trim().Should().Be("No results found.");
    }

    [Fact]
    public void CommandEmpty_Class_Passthrough()
    {
        var cut = Render<CommandEmptyCn>(p => p
            .Add(c => c.Class, "custom-empty")
            .AddChildContent("No results"));
        cut.Find("[data-slot='command-empty']").ClassList.Should().Contain("custom-empty");
    }

    // --- CommandGroupCn ---

    [Fact]
    public void CommandGroup_Renders_With_DataSlot()
    {
        var cut = Render<CommandGroupCn>(p => p.AddChildContent("Group items"));
        cut.Find("[data-slot='command-group']").Should().NotBeNull();
    }

    [Fact]
    public void CommandGroup_Has_Default_Classes()
    {
        var cut = Render<CommandGroupCn>(p => p.AddChildContent("Group items"));
        var el = cut.Find("[data-slot='command-group']");
        el.ClassList.Should().Contain("cn-command-group");
    }

    [Fact]
    public void CommandGroup_Renders_Heading_When_Provided()
    {
        var cut = Render<CommandGroupCn>(p => p
            .Add(c => c.Heading, "Suggestions")
            .AddChildContent("Group items"));
        var el = cut.Find("[data-slot='command-group']");
        el.TextContent.Should().Contain("Suggestions");
    }

    [Fact]
    public void CommandGroup_Heading_Has_Correct_Classes()
    {
        var cut = Render<CommandGroupCn>(p => p
            .Add(c => c.Heading, "Suggestions")
            .AddChildContent("Group items"));
        // The heading is a div inside the group
        var headingDiv = cut.Find("[data-slot='command-group'] > div:first-child");
        headingDiv.ClassList.Should().Contain("px-2");
        headingDiv.ClassList.Should().Contain("py-1.5");
        headingDiv.ClassList.Should().Contain("text-xs");
        headingDiv.ClassList.Should().Contain("font-medium");
        headingDiv.ClassList.Should().Contain("text-muted-foreground");
    }

    [Fact]
    public void CommandGroup_No_Heading_When_Not_Provided()
    {
        var cut = Render<CommandGroupCn>(p => p.AddChildContent("Group items"));
        var groupChildren = cut.FindAll("[data-slot='command-group'] > div");
        // When no heading, only the child content divs should be present, not a heading div
        groupChildren.Should().BeEmpty(); // "Group items" is text, not a div
    }

    [Fact]
    public void CommandGroup_Class_Passthrough()
    {
        var cut = Render<CommandGroupCn>(p => p
            .Add(c => c.Class, "custom-group")
            .AddChildContent("Group items"));
        cut.Find("[data-slot='command-group']").ClassList.Should().Contain("custom-group");
    }

    // --- CommandItemCn ---

    [Fact]
    public void CommandItem_Renders_With_DataSlot()
    {
        var cut = Render<CommandItemCn>(p => p.AddChildContent("Item"));
        cut.Find("[data-slot='command-item']").Should().NotBeNull();
    }

    [Fact]
    public void CommandItem_Has_DataMenuItem_Attribute()
    {
        var cut = Render<CommandItemCn>(p => p.AddChildContent("Item"));
        cut.Find("[data-menu-item]").Should().NotBeNull();
    }

    [Fact]
    public void CommandItem_Has_Default_Classes()
    {
        var cut = Render<CommandItemCn>(p => p.AddChildContent("Item"));
        var item = cut.Find("[data-slot='command-item']");
        item.ClassList.Should().Contain("cn-command-item");
    }

    [Fact]
    public void CommandItem_Has_DataValue_Attribute()
    {
        var cut = Render<CommandItemCn>(p => p
            .Add(c => c.Value, "my-command")
            .AddChildContent("Item"));
        cut.Find("[data-slot='command-item']").GetAttribute("data-value").Should().Be("my-command");
    }

    [Fact]
    public void CommandItem_OnSelect_Fires_On_Click()
    {
        var selected = false;
        var cut = Render<CommandItemCn>(p => p
            .Add(c => c.OnSelect, EventCallback.Factory.Create(this, () => selected = true))
            .AddChildContent("Item"));
        cut.Find("[data-slot='command-item']").Click();
        selected.Should().BeTrue();
    }

    [Fact]
    public void CommandItem_Disabled_Has_DataDisabled()
    {
        var cut = Render<CommandItemCn>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent("Item"));
        cut.Find("[data-slot='command-item']").GetAttribute("data-disabled").Should().Be("true");
    }

    [Fact]
    public void CommandItem_NotDisabled_No_DataDisabled()
    {
        var cut = Render<CommandItemCn>(p => p.AddChildContent("Item"));
        cut.Find("[data-slot='command-item']").GetAttribute("data-disabled").Should().BeNull();
    }

    [Fact]
    public void CommandItem_Disabled_OnSelect_DoesNotFire()
    {
        var selected = false;
        var cut = Render<CommandItemCn>(p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.OnSelect, EventCallback.Factory.Create(this, () => selected = true))
            .AddChildContent("Item"));
        cut.Find("[data-slot='command-item']").Click();
        selected.Should().BeFalse();
    }

    [Fact]
    public void CommandItem_Class_Passthrough()
    {
        var cut = Render<CommandItemCn>(p => p
            .Add(c => c.Class, "custom-item")
            .AddChildContent("Item"));
        cut.Find("[data-slot='command-item']").ClassList.Should().Contain("custom-item");
    }

    [Fact]
    public void CommandItem_AdditionalAttributes_Passthrough()
    {
        var cut = Render<CommandItemCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "aria-label", "My item" } })
            .AddChildContent("Item"));
        cut.Find("[data-slot='command-item']").GetAttribute("aria-label").Should().Be("My item");
    }

    // --- CommandSeparatorCn ---

    [Fact]
    public void CommandSeparator_Renders_With_DataSlot()
    {
        var cut = Render<CommandSeparatorCn>();
        cut.Find("[data-slot='command-separator']").Should().NotBeNull();
    }

    [Fact]
    public void CommandSeparator_Has_Role_Separator()
    {
        var cut = Render<CommandSeparatorCn>();
        cut.Find("[data-slot='command-separator']").GetAttribute("role").Should().Be("separator");
    }

    [Fact]
    public void CommandSeparator_Has_Default_Classes()
    {
        var cut = Render<CommandSeparatorCn>();
        var el = cut.Find("[data-slot='command-separator']");
        el.ClassList.Should().Contain("cn-command-separator");
    }

    [Fact]
    public void CommandSeparator_Class_Passthrough()
    {
        var cut = Render<CommandSeparatorCn>(p => p
            .Add(c => c.Class, "custom-sep"));
        cut.Find("[data-slot='command-separator']").ClassList.Should().Contain("custom-sep");
    }

    // --- Integration ---

    [Fact]
    public void Command_Full_Structure()
    {
        var cut = Render<CommandCn>(p => p
            .AddChildContent(builder =>
            {
                builder.OpenComponent<CommandInputCn>(0);
                builder.AddAttribute(1, "Placeholder", "Type a command...");
                builder.CloseComponent();
                builder.OpenComponent<CommandListCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b =>
                {
                    b.OpenComponent<CommandEmptyCn>(0);
                    b.AddAttribute(1, "ChildContent", (RenderFragment)(eb => eb.AddContent(0, "No results found.")));
                    b.CloseComponent();
                    b.OpenComponent<CommandGroupCn>(2);
                    b.AddAttribute(3, "Heading", "Suggestions");
                    b.AddAttribute(4, "ChildContent", (RenderFragment)(gb =>
                    {
                        gb.OpenComponent<CommandItemCn>(0);
                        gb.AddAttribute(1, "Value", "calendar");
                        gb.AddAttribute(2, "ChildContent", (RenderFragment)(ib => ib.AddContent(0, "Calendar")));
                        gb.CloseComponent();
                        gb.OpenComponent<CommandItemCn>(3);
                        gb.AddAttribute(4, "Value", "search");
                        gb.AddAttribute(5, "ChildContent", (RenderFragment)(ib => ib.AddContent(0, "Search Emoji")));
                        gb.CloseComponent();
                    }));
                    b.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        // Verify structure
        cut.Find("[data-slot='command']").Should().NotBeNull();
        cut.Find("[data-slot='command-input']").Should().NotBeNull();
        cut.Find("[data-slot='command-list']").Should().NotBeNull();
        cut.Find("[data-slot='command-empty']").Should().NotBeNull();
        cut.Find("[data-slot='command-group']").Should().NotBeNull();
        cut.FindAll("[data-slot='command-item']").Should().HaveCount(2);

        // Verify content
        cut.Find("[data-slot='command-empty']").TextContent.Trim().Should().Be("No results found.");
        cut.Find("[data-slot='command-group']").TextContent.Should().Contain("Suggestions");
        cut.FindAll("[data-slot='command-item']")[0].TextContent.Trim().Should().Be("Calendar");
        cut.FindAll("[data-slot='command-item']")[1].TextContent.Trim().Should().Be("Search Emoji");
    }

    [Fact]
    public void Command_Item_OnSelect_In_Structure()
    {
        var selectedItem = "";
        var cut = Render<CommandCn>(p => p
            .AddChildContent(builder =>
            {
                builder.OpenComponent<CommandListCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b =>
                {
                    b.OpenComponent<CommandItemCn>(0);
                    b.AddAttribute(1, "Value", "profile");
                    b.AddAttribute(2, "OnSelect", EventCallback.Factory.Create(this, () => selectedItem = "profile"));
                    b.AddAttribute(3, "ChildContent", (RenderFragment)(ib => ib.AddContent(0, "Profile")));
                    b.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        cut.Find("[data-slot='command-item']").Click();
        selectedItem.Should().Be("profile");
    }

    [Fact]
    public void Command_With_Separator()
    {
        var cut = Render<CommandCn>(p => p
            .AddChildContent(builder =>
            {
                builder.OpenComponent<CommandListCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b =>
                {
                    b.OpenComponent<CommandGroupCn>(0);
                    b.AddAttribute(1, "ChildContent", (RenderFragment)(gb =>
                    {
                        gb.OpenComponent<CommandItemCn>(0);
                        gb.AddAttribute(1, "ChildContent", (RenderFragment)(ib => ib.AddContent(0, "Item 1")));
                        gb.CloseComponent();
                    }));
                    b.CloseComponent();
                    b.OpenComponent<CommandSeparatorCn>(2);
                    b.CloseComponent();
                    b.OpenComponent<CommandGroupCn>(3);
                    b.AddAttribute(4, "ChildContent", (RenderFragment)(gb =>
                    {
                        gb.OpenComponent<CommandItemCn>(0);
                        gb.AddAttribute(1, "ChildContent", (RenderFragment)(ib => ib.AddContent(0, "Item 2")));
                        gb.CloseComponent();
                    }));
                    b.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        cut.FindAll("[data-slot='command-group']").Should().HaveCount(2);
        cut.Find("[data-slot='command-separator']").Should().NotBeNull();
        cut.FindAll("[data-slot='command-item']").Should().HaveCount(2);
    }

    // --- Filtering (cmdk behavior) ---

    private IRenderedComponent<CommandCn> RenderFilterableCommand()
    {
        return Render<CommandCn>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<CommandInputCn>(0);
            builder.CloseComponent();
            builder.OpenComponent<CommandEmptyCn>(1);
            builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(0, "No results found.")));
            builder.CloseComponent();
            builder.OpenComponent<CommandGroupCn>(3);
            builder.AddAttribute(4, "Heading", "Suggestions");
            builder.AddAttribute(5, "ChildContent", (RenderFragment)(gb =>
            {
                gb.OpenComponent<CommandItemCn>(0);
                gb.AddAttribute(1, "Value", "Calendar");
                gb.AddAttribute(2, "ChildContent", (RenderFragment)(ib => ib.AddContent(0, "Calendar")));
                gb.CloseComponent();
                gb.OpenComponent<CommandItemCn>(3);
                gb.AddAttribute(4, "Value", "Settings");
                gb.AddAttribute(5, "ChildContent", (RenderFragment)(ib => ib.AddContent(0, "Settings")));
                gb.CloseComponent();
            }));
            builder.CloseComponent();
        }));
    }

    [Fact]
    public void Command_Typing_Filters_NonMatching_Items()
    {
        var cut = RenderFilterableCommand();
        cut.Find("[data-slot='command-input']").Input("cal");

        cut.Find("[data-value='Calendar']").ClassList.Should().NotContain("hidden");
        cut.Find("[data-value='Settings']").ClassList.Should().Contain("hidden");
    }

    [Fact]
    public void Command_Clearing_Search_Restores_All_Items()
    {
        var cut = RenderFilterableCommand();
        cut.Find("[data-slot='command-input']").Input("cal");
        cut.Find("[data-slot='command-input']").Input("");

        cut.Find("[data-value='Calendar']").ClassList.Should().NotContain("hidden");
        cut.Find("[data-value='Settings']").ClassList.Should().NotContain("hidden");
    }

    [Fact]
    public void Command_Empty_Hidden_Until_Search_Matches_Nothing()
    {
        var cut = RenderFilterableCommand();
        cut.Find("[data-slot='command-empty']").ClassList.Should().Contain("hidden");

        cut.Find("[data-slot='command-input']").Input("zzz");

        cut.Find("[data-slot='command-empty']").ClassList.Should().NotContain("hidden");
        cut.Find("[data-slot='command-group']").ClassList.Should().Contain("hidden");
    }
}
