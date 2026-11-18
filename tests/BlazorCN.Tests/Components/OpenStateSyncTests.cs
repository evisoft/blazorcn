using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

/// <summary>
/// How an overlay's own open state meets the consumer's <c>Open</c> parameter when the parent re-renders.
/// Without <c>OpenChanged</c>, <c>Open</c> is only a starting value: an unrelated parent re-render (a state
/// push, a timer, a live update) must not close a menu the user opened. With <c>OpenChanged</c> bound the
/// consumer owns the state, so a re-render still syncs to it; that is how a consumer refuses a close.
/// </summary>
public class OpenStateSyncTests : BunitContext
{
    private void SetupJsInterop()
    {
        var module = JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
        module.Setup<string>("createFloating", _ => true).SetResult("bottom");
        module.SetupVoid(_ => true).SetVoidResult();
        Services.AddScoped<JsInteropCn>();
    }

    private static string State(IRenderedComponent<DropdownMenuCn> cut) =>
        cut.Find("[data-slot='dropdown-menu']").GetAttribute("data-state")!;

    [Fact]
    public void Uncontrolled_DropdownMenu_Stays_Open_When_The_Parent_Rerenders()
    {
        SetupJsInterop();
        var cut = Render<DropdownMenuCn>(p => p
            .AddChildContent<DropdownMenuTriggerCn>(t => t.AddChildContent("Open")));

        cut.Find("[data-slot='dropdown-menu-trigger']").Click();
        State(cut).Should().Be("open");

        // A parent re-render passes the parameters again; Open is still its default false.
        cut.Render(p => p.Add(c => c.Class, "re-rendered"));

        State(cut).Should().Be("open");
    }

    [Fact]
    public void Uncontrolled_DropdownMenu_Still_Follows_A_Changed_Open()
    {
        SetupJsInterop();
        var cut = Render<DropdownMenuCn>(p => p
            .AddChildContent<DropdownMenuTriggerCn>(t => t.AddChildContent("Open")));

        cut.Render(p => p.Add(c => c.Open, true));
        State(cut).Should().Be("open");

        cut.Render(p => p.Add(c => c.Open, false));
        State(cut).Should().Be("closed");
    }

    [Fact]
    public void Uncontrolled_DropdownMenu_Closed_By_The_User_Stays_Closed_On_Rerender()
    {
        SetupJsInterop();
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DropdownMenuTriggerCn>(t => t.AddChildContent("Open")));

        cut.Find("[data-slot='dropdown-menu-trigger']").Click();
        State(cut).Should().Be("closed");

        cut.Render(p => p.Add(c => c.Class, "re-rendered"));

        State(cut).Should().Be("closed");
    }

    [Fact]
    public void Bound_DropdownMenu_Follows_The_Consumer_Value()
    {
        SetupJsInterop();
        var isOpen = false;
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, isOpen)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<DropdownMenuTriggerCn>(t => t.AddChildContent("Open")));

        cut.Find("[data-slot='dropdown-menu-trigger']").Click();
        isOpen.Should().BeTrue();
        cut.Render(p => p.Add(c => c.Open, isOpen));
        State(cut).Should().Be("open");

        cut.Render(p => p.Add(c => c.Open, false));
        State(cut).Should().Be("closed");
    }

    [Fact]
    public void Controlled_Dialog_Can_Refuse_A_Close()
    {
        SetupJsInterop();
        var asked = false;
        // The consumer hears the close request and keeps Open = true (e.g. "can't close while saving").
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, _ => asked = true))
            .AddChildContent<DialogContentCn>(c => c.AddChildContent("Dialog body")));

        cut.Find("[data-slot='dialog-content'] [data-slot='dialog-close']").Click();
        asked.Should().BeTrue();

        cut.Render(p => p.Add(c => c.Open, true));

        cut.Find("[data-slot='dialog']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void Uncontrolled_Popover_Stays_Open_When_The_Parent_Rerenders()
    {
        SetupJsInterop();
        var cut = Render<PopoverCn>(p => p
            .AddChildContent<PopoverTriggerCn>(t => t.AddChildContent("Open")));

        cut.Find("[data-slot='popover-trigger']").Click();
        cut.Find("[data-slot='popover']").GetAttribute("data-state").Should().Be("open");

        cut.Render(p => p.Add(c => c.Class, "re-rendered"));

        cut.Find("[data-slot='popover']").GetAttribute("data-state").Should().Be("open");
    }
}
