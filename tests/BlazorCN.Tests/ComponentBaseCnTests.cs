using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Xunit;

namespace BlazorCN.Tests;

public class ComponentBaseCnTests : BunitContext
{
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
}
