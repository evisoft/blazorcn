using System.Linq;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Rendering;
using Xunit;

namespace BlazorCN.Tests;

public class ComponentBaseCnTests : BunitContext
{
    public ComponentBaseCnTests()
    {
        // InputOtpCn injects JsInteropCn (DOM value resync for rejected characters).
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddScoped<JsInteropCn>();
    }

    private class TestComponent : ComponentBaseCn
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            if (AdditionalAttributes != null)
            {
                builder.AddMultipleAttributes(1, AdditionalAttributes!);
            }
            if (!string.IsNullOrEmpty(Class))
            {
                builder.AddAttribute(2, "class", Class);
            }
            if (!string.IsNullOrEmpty(Style))
            {
                builder.AddAttribute(3, "style", Style);
            }
            builder.CloseElement();
        }
    }

    [Fact]
    public void Class_Parameter_Is_Rendered()
    {
        var cut = Render<TestComponent>(p => p.Add(c => c.Class, "my-class"));
        cut.Find("div").ClassList.Should().Contain("my-class");
    }

    [Fact]
    public void Style_Parameter_Is_Rendered()
    {
        var cut = Render<TestComponent>(p => p.Add(c => c.Style, "color: red"));
        cut.Find("div").GetAttribute("style").Should().Be("color: red");
    }

    [Fact]
    public void AdditionalAttributes_Are_Passed_Through()
    {
        var cut = Render<TestComponent>(p => p
            .AddUnmatched("data-testid", "test-123")
            .AddUnmatched("aria-label", "test label"));
        var div = cut.Find("div");
        div.GetAttribute("data-testid").Should().Be("test-123");
        div.GetAttribute("aria-label").Should().Be("test label");
    }

    // Blazor matches component parameters case-insensitively, so a lowercase `class=` on a
    // BlazorCN component tag binds to the `Class` PARAMETER rather than falling into the
    // attribute splat. That means it is merged with the component's own classes instead of
    // replacing them or emitting a second class attribute. Pinned because the demo uses
    // lowercase `class=` in places and the difference is invisible until it isn't.
    [Fact]
    public void Lowercase_Class_Attribute_Binds_To_Class_Parameter_And_Merges()
    {
        var cut = Render<SelectTriggerCn>(p => p.AddUnmatched("class", "h-8 text-sm"));
        var button = cut.Find("button");
        button.Attributes.Count(a => a.Name == "class").Should().Be(1);   // not two class attributes
        button.ClassList.Should().Contain("cn-select-trigger");   // component's own class survives
        button.ClassList.Should().Contain("h-8");                 // and the consumer's is merged in
        button.ClassList.Should().Contain("text-sm");
    }

    // `id` has no matching parameter, so it DOES land in the splat — and because
    // `@attributes` is rendered after the component's own `id`, the consumer's value wins.
    // This is what makes `<LabelCn For="x">` + `<SelectTriggerCn id="x">` name the combobox:
    // SelectTriggerCn renders role="combobox", which takes no name from its contents.
    [Fact]
    public void Consumer_Id_Overrides_The_Generated_TriggerId()
    {
        var cut = Render<SelectTriggerCn>(p => p.AddUnmatched("id", "country-select"));
        cut.Find("button").GetAttribute("id").Should().Be("country-select");
    }

    // HTML attribute names are case-insensitive and Blazor keeps the casing the consumer typed,
    // so a component asking `ContainsKey("id")` misses `Id="x"`. That mattered: InputOtpCn then
    // added its own aria-label, and an aria-label on an input OVERRIDES the <label for> the
    // consumer just wired up — replacing their label text with "One-time code".
    [Theory]
    [InlineData("id")]
    [InlineData("Id")]
    [InlineData("ID")]
    public void Consumer_Id_Suppresses_The_InputOtp_Default_AriaLabel(string attributeName)
    {
        var cut = Render<InputOtpCn>(p => p.AddUnmatched(attributeName, "code"));

        cut.Find("input").GetAttribute("aria-label").Should().BeNull();
    }

    [Fact]
    public void InputOtp_Still_Names_Itself_When_The_Consumer_Wires_Nothing()
    {
        var cut = Render<InputOtpCn>();

        cut.Find("input").GetAttribute("aria-label").Should().Be("One-time code");
    }

    // Same trap on icons, opposite direction: a PascalCase Role= left the icon aria-hidden, so an
    // icon the consumer deliberately exposed stayed invisible to assistive technology.
    [Theory]
    [InlineData("role")]
    [InlineData("Role")]
    public void Consumer_Role_Stops_An_Icon_Being_Hidden(string attributeName)
    {
        var cut = Render<LucideCheckCn>(p => p.AddUnmatched(attributeName, "img"));

        cut.Find("svg").HasAttribute("aria-hidden").Should().BeFalse();
    }
}
