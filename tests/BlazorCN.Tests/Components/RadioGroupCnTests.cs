using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class RadioGroupCnTests : BunitContext
{
    public RadioGroupCnTests()
    {
        // RadioGroupCn injects JsInteropCn (arrow-key scroll suppression).
        // Loose mode lets those interop calls no-op, and registering the service satisfies [Inject].
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddScoped<JsInteropCn>();
    }

    [Fact]
    public void Renders_With_Role_Radiogroup()
    {
        var cut = Render<RadioGroupCn>(p => p
            .AddChildContent<RadioGroupItemCn>(item => item.Add(i => i.Value, "a")));
        var div = cut.Find("[data-slot='radio-group']");
        div.GetAttribute("role").Should().Be("radiogroup");
    }

    [Fact]
    public void Item_Selection_Works()
    {
        string? selectedValue = null;
        var cut = Render<RadioGroupCn>(p => p
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string?>(this, v => selectedValue = v))
            .AddChildContent<RadioGroupItemCn>(item => item.Add(i => i.Value, "option1")));
        cut.Find("[data-slot='radio-group-item']").Click();
        selectedValue.Should().Be("option1");
    }

    [Fact]
    public void Only_Selected_Item_Shows_Checked_State()
    {
        var cut = Render<RadioGroupCn>(p => p
            .Add(c => c.Value, "b")
            .AddChildContent<RadioGroupItemCn>(item => item.Add(i => i.Value, "a"))
            .AddChildContent<RadioGroupItemCn>(item => item.Add(i => i.Value, "b")));

        var items = cut.FindAll("[data-slot='radio-group-item']");
        items[0].GetAttribute("data-state").Should().Be("unchecked");
        items[0].GetAttribute("aria-checked").Should().Be("false");
        items[1].GetAttribute("data-state").Should().Be("checked");
        items[1].GetAttribute("aria-checked").Should().Be("true");
    }

    [Fact]
    public void Has_Correct_DataSlots()
    {
        var cut = Render<RadioGroupCn>(p => p
            .Add(c => c.Value, "a")
            .AddChildContent<RadioGroupItemCn>(item => item.Add(i => i.Value, "a")));
        cut.Find("[data-slot='radio-group']").Should().NotBeNull();
        cut.Find("[data-slot='radio-group-item']").Should().NotBeNull();
    }

    [Fact]
    public void Wires_PreventKeyDefaults_For_Handled_Keys()
    {
        // Blazor's @onkeydown cannot conditionally preventDefault, so without this
        // JS guard every arrow/Home/End press also scrolls the page (regression).
        var module = JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
        var handler = module.SetupVoid("preventKeyDefaults", _ => true);
        handler.SetVoidResult();

        Render<RadioGroupCn>(p => p
            .AddChildContent<RadioGroupItemCn>(item => item.Add(i => i.Value, "a")));

        var invocation = handler.Invocations.Should().ContainSingle().Subject;
        invocation.Arguments[2].Should().BeEquivalentTo(
            new[] { "ArrowDown", "ArrowUp", "ArrowLeft", "ArrowRight", "Home", "End" });
        invocation.Arguments[3].Should().Be("[role=\"radio\"]");
    }
}
