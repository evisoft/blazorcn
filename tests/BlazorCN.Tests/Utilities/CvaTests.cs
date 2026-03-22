using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Utilities;

public class CvaTests
{
    private enum TestVariant { Default, Destructive, Outline }
    private enum TestSize { Default, Sm, Lg }

    [Fact]
    public void Apply_Returns_Base_Classes_With_Defaults()
    {
        var cva = new Cva<TestVariant, TestSize>(
            "base-class font-medium",
            new Dictionary<TestVariant, string>
            {
                [TestVariant.Default] = "bg-primary",
                [TestVariant.Destructive] = "bg-destructive",
                [TestVariant.Outline] = "border bg-background",
            },
            new Dictionary<TestSize, string>
            {
                [TestSize.Default] = "h-10 px-4",
                [TestSize.Sm] = "h-8 px-3",
                [TestSize.Lg] = "h-12 px-6",
            });

        var result = cva.Apply(TestVariant.Default, TestSize.Default);
        result.Should().Contain("base-class");
        result.Should().Contain("font-medium");
        result.Should().Contain("bg-primary");
        result.Should().Contain("h-10");
        result.Should().Contain("px-4");
    }

    [Fact]
    public void Apply_Resolves_Variant()
    {
        var cva = new Cva<TestVariant, TestSize>(
            "base",
            new Dictionary<TestVariant, string>
            {
                [TestVariant.Default] = "bg-primary",
                [TestVariant.Destructive] = "bg-destructive",
            },
            new Dictionary<TestSize, string>
            {
                [TestSize.Default] = "h-10",
            });

        var result = cva.Apply(TestVariant.Destructive, TestSize.Default);
        result.Should().Contain("bg-destructive");
        result.Should().NotContain("bg-primary");
    }

    [Fact]
    public void Apply_Merges_Additional_Classes()
    {
        var cva = new Cva<TestVariant, TestSize>(
            "base",
            new Dictionary<TestVariant, string>
            {
                [TestVariant.Default] = "bg-primary",
            },
            new Dictionary<TestSize, string>
            {
                [TestSize.Default] = "h-10",
            });

        var result = cva.Apply(TestVariant.Default, TestSize.Default, "custom-class");
        result.Should().Contain("custom-class");
    }

    [Fact]
    public void Apply_Additional_Class_Overrides_Conflicting_Base()
    {
        var cva = new Cva<TestVariant, TestSize>(
            "h-9",
            new Dictionary<TestVariant, string>
            {
                [TestVariant.Default] = "bg-primary",
            },
            new Dictionary<TestSize, string>
            {
                [TestSize.Default] = "px-4",
            });

        var result = cva.Apply(TestVariant.Default, TestSize.Default, "h-12");
        result.Should().Contain("h-12");
        result.Should().NotContain("h-9");
    }

    [Fact]
    public void Single_Dimension_Cva_Applies_Variant()
    {
        var cva = new Cva<TestVariant>(
            "base-class",
            new Dictionary<TestVariant, string>
            {
                [TestVariant.Default] = "bg-primary",
                [TestVariant.Destructive] = "bg-destructive",
            });

        var result = cva.Apply(TestVariant.Destructive, "custom");
        result.Should().Contain("base-class");
        result.Should().Contain("bg-destructive");
        result.Should().Contain("custom");
        result.Should().NotContain("bg-primary");
    }
}
