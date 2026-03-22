# BlazorCN Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a production-ready Blazor component library that replicates shadcn-ui one-to-one, shipped as a NuGet package.

**Architecture:** Thin Blazor component wrappers outputting Tailwind CSS utility classes. Minimal base class (`ComponentBaseCn`), C# port of CVA for variants, `Cn.Merge()` for class merging, CSS variables for theming, minimal JS interop for interactive behaviors (focus trap, floating positioning, outside click detection).

**Tech Stack:** .NET 10, Blazor (all rendering modes), Tailwind CSS (consumer-configured), Floating UI (JS), bUnit + xUnit (testing)

---

## Phase 1: Project Foundation

### Task 1: Create solution and project structure

**Files:**
- Create: `BlazorCN.slnx`
- Create: `src/BlazorCN/BlazorCN.csproj`
- Create: `tests/BlazorCN.Tests/BlazorCN.Tests.csproj`

**Step 1: Create the solution file**

```bash
cd C:/Users/evisoft/source/repos/blazorcn
dotnet new slnx -n BlazorCN
```

**Step 2: Create the Razor class library project**

```bash
dotnet new razorclasslib -n BlazorCN -o src/BlazorCN -f net10.0
```

**Step 3: Create the test project**

```bash
dotnet new xunit -n BlazorCN.Tests -o tests/BlazorCN.Tests -f net10.0
```

**Step 4: Add projects to solution**

```bash
dotnet sln BlazorCN.slnx add src/BlazorCN/BlazorCN.csproj
dotnet sln BlazorCN.slnx add tests/BlazorCN.Tests/BlazorCN.Tests.csproj
```

**Step 5: Add project reference from tests to library**

```bash
dotnet add tests/BlazorCN.Tests/BlazorCN.Tests.csproj reference src/BlazorCN/BlazorCN.csproj
```

**Step 6: Add bUnit to test project**

```bash
dotnet add tests/BlazorCN.Tests/BlazorCN.Tests.csproj package bunit
```

**Step 7: Verify build**

```bash
dotnet build BlazorCN.slnx
```
Expected: Build succeeded.

**Step 8: Commit**

```bash
git init
git add BlazorCN.slnx src/BlazorCN/BlazorCN.csproj tests/BlazorCN.Tests/BlazorCN.Tests.csproj
git commit -m "chore: scaffold BlazorCN solution with library and test projects"
```

---

### Task 2: Configure BlazorCN.csproj for NuGet

**Files:**
- Modify: `src/BlazorCN/BlazorCN.csproj`

**Step 1: Update the .csproj with NuGet metadata and library settings**

Replace the contents of `src/BlazorCN/BlazorCN.csproj` with:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>BlazorCN</RootNamespace>
  </PropertyGroup>

  <PropertyGroup>
    <PackageId>BlazorCN</PackageId>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <Authors>BlazorCN Contributors</Authors>
    <Description>A Blazor component library that replicates shadcn-ui. Thin components, Tailwind CSS, CSS variables theming.</Description>
    <PackageTags>Blazor;shadcn;Tailwind;Components;UI;BlazorCN</PackageTags>
    <RepositoryUrl>https://github.com/user/blazorcn</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
  </PropertyGroup>

  <PropertyGroup>
    <IsTrimmable>true</IsTrimmable>
    <TrimMode>link</TrimMode>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components" Version="10.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="10.0.0" />
  </ItemGroup>

</Project>
```

**Step 2: Verify build**

```bash
dotnet build BlazorCN.slnx
```
Expected: Build succeeded.

**Step 3: Commit**

```bash
git add src/BlazorCN/BlazorCN.csproj
git commit -m "chore: configure BlazorCN.csproj with NuGet metadata and trimming"
```

---

### Task 3: Create _Imports.razor

**Files:**
- Create: `src/BlazorCN/_Imports.razor`

**Step 1: Write _Imports.razor**

```razor
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Rendering
@using Microsoft.JSInterop
```

**Step 2: Verify build**

```bash
dotnet build src/BlazorCN/BlazorCN.csproj
```

**Step 3: Commit**

```bash
git add src/BlazorCN/_Imports.razor
git commit -m "chore: add _Imports.razor with common usings"
```

---

### Task 4: Create ComponentBaseCn base class

**Files:**
- Create: `src/BlazorCN/ComponentBaseCn.cs`
- Test: `tests/BlazorCN.Tests/ComponentBaseCnTests.cs`

**Step 1: Write the test**

```csharp
using Bunit;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BlazorCN.Tests;

public class ComponentBaseCnTests : TestContext
{
    // We need a concrete implementation to test the abstract base
    private class TestComponent : ComponentBaseCn
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            if (AdditionalAttributes != null)
            {
                builder.AddMultipleAttributes(1, AdditionalAttributes);
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
        var cut = RenderComponent<TestComponent>(p => p.Add(c => c.Class, "my-class"));
        cut.Find("div").ClassList.Should().Contain("my-class");
    }

    [Fact]
    public void Style_Parameter_Is_Rendered()
    {
        var cut = RenderComponent<TestComponent>(p => p.Add(c => c.Style, "color: red"));
        cut.Find("div").GetAttribute("style").Should().Be("color: red");
    }

    [Fact]
    public void AdditionalAttributes_Are_Passed_Through()
    {
        var cut = RenderComponent<TestComponent>(p => p
            .AddUnmatched("data-testid", "test-123")
            .AddUnmatched("aria-label", "test label"));
        var div = cut.Find("div");
        div.GetAttribute("data-testid").Should().Be("test-123");
        div.GetAttribute("aria-label").Should().Be("test label");
    }
}
```

**Step 2: Add FluentAssertions to test project**

```bash
dotnet add tests/BlazorCN.Tests/BlazorCN.Tests.csproj package FluentAssertions
```

**Step 3: Run tests — should fail (ComponentBaseCn doesn't exist yet)**

```bash
dotnet test tests/BlazorCN.Tests/BlazorCN.Tests.csproj
```
Expected: FAIL — `ComponentBaseCn` not found.

**Step 4: Write ComponentBaseCn**

```csharp
using Microsoft.AspNetCore.Components;

namespace BlazorCN;

/// <summary>
/// Base class for all BlazorCN components. Provides Class, Style, and AdditionalAttributes parameters.
/// </summary>
public abstract class ComponentBaseCn : ComponentBase
{
    /// <summary>
    /// Additional CSS class names to apply to the component's root element.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Inline styles to apply to the component's root element.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// Captures any additional attributes not explicitly declared as parameters.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object?>? AdditionalAttributes { get; set; }
}
```

**Step 5: Run tests — should pass**

```bash
dotnet test tests/BlazorCN.Tests/BlazorCN.Tests.csproj
```
Expected: 3 tests passed.

**Step 6: Commit**

```bash
git add src/BlazorCN/ComponentBaseCn.cs tests/BlazorCN.Tests/ComponentBaseCnTests.cs
git commit -m "feat: add ComponentBaseCn base class with Class, Style, AdditionalAttributes"
```

---

### Task 5: Create Cn.Merge() utility

**Files:**
- Create: `src/BlazorCN/Utilities/Cn.cs`
- Test: `tests/BlazorCN.Tests/Utilities/CnTests.cs`

**Step 1: Write the tests**

```csharp
using Xunit;
using FluentAssertions;

namespace BlazorCN.Tests.Utilities;

public class CnTests
{
    [Fact]
    public void Merge_Combines_Multiple_Classes()
    {
        var result = Cn.Merge("foo", "bar");
        result.Should().Be("foo bar");
    }

    [Fact]
    public void Merge_Skips_Null_And_Empty()
    {
        var result = Cn.Merge("foo", null, "", "bar");
        result.Should().Be("foo bar");
    }

    [Fact]
    public void Merge_Last_Tailwind_Utility_Wins()
    {
        // p-2 and p-4 conflict — last should win
        var result = Cn.Merge("p-2", "p-4");
        result.Should().Be("p-4");
    }

    [Fact]
    public void Merge_Different_Utilities_Kept()
    {
        var result = Cn.Merge("p-2 m-4", "text-sm");
        result.Should().Be("p-2 m-4 text-sm");
    }

    [Fact]
    public void Merge_Handles_Conflicting_Colors()
    {
        var result = Cn.Merge("bg-red-500", "bg-blue-500");
        result.Should().Be("bg-blue-500");
    }

    [Fact]
    public void Merge_Handles_Conflicting_Text_Sizes()
    {
        var result = Cn.Merge("text-sm", "text-lg");
        result.Should().Be("text-lg");
    }

    [Fact]
    public void Merge_Handles_Conflicting_Width()
    {
        var result = Cn.Merge("w-full", "w-1/2");
        result.Should().Be("w-1/2");
    }

    [Fact]
    public void Merge_Handles_Conflicting_Height()
    {
        var result = Cn.Merge("h-9 px-4 py-2", "h-10 px-6");
        result.Should().Be("py-2 h-10 px-6");
    }

    [Fact]
    public void Merge_Preserves_Arbitrary_Values()
    {
        var result = Cn.Merge("text-[14px]", "text-[16px]");
        result.Should().Be("text-[16px]");
    }

    [Fact]
    public void Merge_Handles_Responsive_Prefixes()
    {
        var result = Cn.Merge("md:text-sm", "md:text-lg");
        result.Should().Be("md:text-lg");
    }

    [Fact]
    public void Merge_Returns_Empty_For_No_Input()
    {
        var result = Cn.Merge();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Merge_Handles_Conditional_Class_With_Null()
    {
        string? conditionalClass = null;
        var result = Cn.Merge("base-class", conditionalClass);
        result.Should().Be("base-class");
    }
}
```

**Step 2: Run tests — should fail**

```bash
dotnet test tests/BlazorCN.Tests/BlazorCN.Tests.csproj --filter "FullyQualifiedName~CnTests"
```
Expected: FAIL — `Cn` class not found.

**Step 3: Write the Cn utility**

This is a C# port of `tailwind-merge`. It needs to understand Tailwind utility groups to resolve conflicts (e.g., `p-2` vs `p-4` are both padding, last wins).

```csharp
using System.Text.RegularExpressions;

namespace BlazorCN;

/// <summary>
/// Utility for merging Tailwind CSS class names. Conflicting utilities are resolved by last-wins.
/// Port of tailwind-merge + clsx.
/// </summary>
public static partial class Cn
{
    /// <summary>
    /// Merges multiple CSS class strings. Conflicting Tailwind utilities are resolved by last-wins.
    /// </summary>
    public static string Merge(params string?[] classes)
    {
        var allClasses = new List<string>();
        foreach (var cls in classes)
        {
            if (string.IsNullOrWhiteSpace(cls)) continue;
            allClasses.AddRange(cls.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        if (allClasses.Count == 0) return string.Empty;

        // Process in reverse: first occurrence (from end) wins for each group
        var seen = new Dictionary<string, int>();
        var result = new List<(string className, int originalIndex)>();

        for (var i = allClasses.Count - 1; i >= 0; i--)
        {
            var className = allClasses[i];
            var group = GetUtilityGroup(className);

            if (group != null)
            {
                if (seen.ContainsKey(group)) continue; // skip — a later class already covers this group
                seen[group] = i;
            }

            result.Add((className, i));
        }

        result.Reverse();
        return string.Join(' ', result.Select(r => r.className));
    }

    /// <summary>
    /// Extracts the utility group key from a Tailwind class.
    /// Classes in the same group conflict with each other.
    /// Returns null for classes that don't match any known pattern.
    /// </summary>
    private static string? GetUtilityGroup(string className)
    {
        // Extract prefix (responsive, state) and base utility
        // e.g., "md:hover:text-sm" → prefix="md:hover:", base="text-sm"
        var prefixEnd = className.LastIndexOf(':');
        var prefix = prefixEnd >= 0 ? className[..(prefixEnd + 1)] : "";
        var baseClass = prefixEnd >= 0 ? className[(prefixEnd + 1)..] : className;

        var utilityKey = GetBaseUtilityKey(baseClass);
        if (utilityKey == null) return null;

        return prefix + utilityKey;
    }

    private static string? GetBaseUtilityKey(string baseClass)
    {
        // Handle arbitrary values: text-[14px] → "text"
        var arbitraryMatch = ArbitraryRegex().Match(baseClass);
        if (arbitraryMatch.Success)
        {
            return arbitraryMatch.Groups[1].Value;
        }

        // Match against known utility patterns
        // Order matters: more specific patterns first

        // Spacing: p, px, py, pt, pr, pb, pl, m, mx, my, mt, mr, mb, ml, gap
        if (SpacingRegex().IsMatch(baseClass))
            return SpacingRegex().Match(baseClass).Groups[1].Value;

        // Sizing: w, h, min-w, min-h, max-w, max-h, size
        if (SizingRegex().IsMatch(baseClass))
            return SizingRegex().Match(baseClass).Groups[1].Value;

        // Typography: text-{size}, font-{weight}, leading, tracking
        if (TextSizeRegex().IsMatch(baseClass))
            return "text-size";
        if (FontWeightRegex().IsMatch(baseClass))
            return "font-weight";
        if (LeadingRegex().IsMatch(baseClass))
            return "leading";
        if (TrackingRegex().IsMatch(baseClass))
            return "tracking";

        // Colors: bg-, text- (color), border- (color)
        if (BgColorRegex().IsMatch(baseClass))
            return "bg-color";
        if (TextColorRegex().IsMatch(baseClass))
            return "text-color";
        if (BorderColorRegex().IsMatch(baseClass))
            return "border-color";

        // Border: rounded, border-{width}
        if (RoundedRegex().IsMatch(baseClass))
            return "rounded";
        if (BorderWidthRegex().IsMatch(baseClass))
            return "border-width";

        // Display
        if (DisplayRegex().IsMatch(baseClass))
            return "display";

        // Position
        if (PositionRegex().IsMatch(baseClass))
            return "position";

        // Flex/Grid
        if (JustifyRegex().IsMatch(baseClass))
            return "justify";
        if (ItemsRegex().IsMatch(baseClass))
            return "items";
        if (FlexRegex().IsMatch(baseClass))
            return "flex";
        if (GridColsRegex().IsMatch(baseClass))
            return "grid-cols";

        // Opacity
        if (OpacityRegex().IsMatch(baseClass))
            return "opacity";

        // Shadow
        if (ShadowRegex().IsMatch(baseClass))
            return "shadow";

        // Z-index
        if (ZIndexRegex().IsMatch(baseClass))
            return "z-index";

        // Overflow
        if (OverflowRegex().IsMatch(baseClass))
            return "overflow";

        // Cursor
        if (CursorRegex().IsMatch(baseClass))
            return "cursor";

        return null; // unknown utility — no conflict resolution, always kept
    }

    [GeneratedRegex(@"^(.+?)-\[.+\]$")]
    private static partial Regex ArbitraryRegex();

    [GeneratedRegex(@"^(-?(?:p|px|py|pt|pr|pb|pl|ps|pe|m|mx|my|mt|mr|mb|ml|ms|me|gap|gap-x|gap-y|space-x|space-y|inset|inset-x|inset-y|top|right|bottom|left|start|end))-.+$")]
    private static partial Regex SpacingRegex();

    [GeneratedRegex(@"^(w|h|min-w|min-h|max-w|max-h|size)-.+$")]
    private static partial Regex SizingRegex();

    [GeneratedRegex(@"^text-(xs|sm|base|lg|xl|2xl|3xl|4xl|5xl|6xl|7xl|8xl|9xl)$")]
    private static partial Regex TextSizeRegex();

    [GeneratedRegex(@"^font-(thin|extralight|light|normal|medium|semibold|bold|extrabold|black)$")]
    private static partial Regex FontWeightRegex();

    [GeneratedRegex(@"^leading-.+$")]
    private static partial Regex LeadingRegex();

    [GeneratedRegex(@"^tracking-.+$")]
    private static partial Regex TrackingRegex();

    [GeneratedRegex(@"^bg-(transparent|current|inherit|white|black|slate|gray|zinc|neutral|stone|red|orange|amber|yellow|lime|green|emerald|teal|cyan|sky|blue|indigo|violet|purple|fuchsia|pink|rose|primary|secondary|destructive|muted|accent|popover|card|background|foreground|input|ring|border|chart|sidebar|surface|code|selection)")]
    private static partial Regex BgColorRegex();

    [GeneratedRegex(@"^text-(transparent|current|inherit|white|black|slate|gray|zinc|neutral|stone|red|orange|amber|yellow|lime|green|emerald|teal|cyan|sky|blue|indigo|violet|purple|fuchsia|pink|rose|primary|secondary|destructive|muted|accent|popover|card|background|foreground|input|ring|border|chart|sidebar|surface|code|selection)")]
    private static partial Regex TextColorRegex();

    [GeneratedRegex(@"^border-(transparent|current|inherit|white|black|slate|gray|zinc|neutral|stone|red|orange|amber|yellow|lime|green|emerald|teal|cyan|sky|blue|indigo|violet|purple|fuchsia|pink|rose|primary|secondary|destructive|muted|accent|popover|card|background|foreground|input|ring|border)")]
    private static partial Regex BorderColorRegex();

    [GeneratedRegex(@"^rounded(-|$)")]
    private static partial Regex RoundedRegex();

    [GeneratedRegex(@"^border(-[0-9]|$)")]
    private static partial Regex BorderWidthRegex();

    [GeneratedRegex(@"^(block|inline-block|inline|flex|inline-flex|table|inline-table|table-caption|table-cell|table-column|table-column-group|table-footer-group|table-header-group|table-row-group|table-row|flow-root|grid|inline-grid|contents|list-item|hidden)$")]
    private static partial Regex DisplayRegex();

    [GeneratedRegex(@"^(static|fixed|absolute|relative|sticky)$")]
    private static partial Regex PositionRegex();

    [GeneratedRegex(@"^justify-.+$")]
    private static partial Regex JustifyRegex();

    [GeneratedRegex(@"^items-.+$")]
    private static partial Regex ItemsRegex();

    [GeneratedRegex(@"^flex-.+$")]
    private static partial Regex FlexRegex();

    [GeneratedRegex(@"^grid-cols-.+$")]
    private static partial Regex GridColsRegex();

    [GeneratedRegex(@"^opacity-.+$")]
    private static partial Regex OpacityRegex();

    [GeneratedRegex(@"^shadow(-|$)")]
    private static partial Regex ShadowRegex();

    [GeneratedRegex(@"^z-.+$")]
    private static partial Regex ZIndexRegex();

    [GeneratedRegex(@"^overflow-.+$")]
    private static partial Regex OverflowRegex();

    [GeneratedRegex(@"^cursor-.+$")]
    private static partial Regex CursorRegex();
}
```

**Step 4: Run tests — should pass**

```bash
dotnet test tests/BlazorCN.Tests/BlazorCN.Tests.csproj --filter "FullyQualifiedName~CnTests"
```
Expected: All 12 tests pass.

**Step 5: Commit**

```bash
git add src/BlazorCN/Utilities/Cn.cs tests/BlazorCN.Tests/Utilities/CnTests.cs
git commit -m "feat: add Cn.Merge() utility for intelligent Tailwind class merging"
```

---

### Task 6: Create Cva (Class Variance Authority) utility

**Files:**
- Create: `src/BlazorCN/Utilities/Cva.cs`
- Test: `tests/BlazorCN.Tests/Utilities/CvaTests.cs`

**Step 1: Write the tests**

```csharp
using Xunit;
using FluentAssertions;

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
}
```

**Step 2: Run tests — should fail**

```bash
dotnet test tests/BlazorCN.Tests/BlazorCN.Tests.csproj --filter "FullyQualifiedName~CvaTests"
```
Expected: FAIL — `Cva` not found.

**Step 3: Write the Cva class**

```csharp
namespace BlazorCN;

/// <summary>
/// Class Variance Authority — maps variant/size enum values to Tailwind class strings.
/// Port of the CVA JavaScript library.
/// </summary>
/// <typeparam name="TVariant">Variant enum type</typeparam>
/// <typeparam name="TSize">Size enum type</typeparam>
public sealed class Cva<TVariant, TSize>
    where TVariant : struct, Enum
    where TSize : struct, Enum
{
    private readonly string _base;
    private readonly Dictionary<TVariant, string> _variants;
    private readonly Dictionary<TSize, string> _sizes;

    public Cva(
        string baseClasses,
        Dictionary<TVariant, string> variants,
        Dictionary<TSize, string> sizes)
    {
        _base = baseClasses;
        _variants = variants;
        _sizes = sizes;
    }

    /// <summary>
    /// Resolves the final class string by combining base + variant + size + additional classes.
    /// Uses Cn.Merge() so conflicting utilities are resolved by last-wins.
    /// </summary>
    public string Apply(TVariant variant, TSize size, string? additionalClasses = null)
    {
        _variants.TryGetValue(variant, out var variantClasses);
        _sizes.TryGetValue(size, out var sizeClasses);

        return Cn.Merge(_base, variantClasses, sizeClasses, additionalClasses);
    }
}
```

**Step 4: Run tests — should pass**

```bash
dotnet test tests/BlazorCN.Tests/BlazorCN.Tests.csproj --filter "FullyQualifiedName~CvaTests"
```
Expected: All 4 tests pass.

**Step 5: Commit**

```bash
git add src/BlazorCN/Utilities/Cva.cs tests/BlazorCN.Tests/Utilities/CvaTests.cs
git commit -m "feat: add Cva<TVariant, TSize> class variance authority utility"
```

---

### Task 7: Create CSS variables file (blazorcn.css)

**Files:**
- Create: `src/BlazorCN/wwwroot/blazorcn.css`

**Step 1: Write the CSS variables file**

Copy shadcn-ui's exact theming variables using oklch color space:

```css
/* BlazorCN Theme Variables — matches shadcn-ui */

:root {
    --radius: 0.625rem;
    --background: oklch(1 0 0);
    --foreground: oklch(0.145 0 0);
    --card: oklch(1 0 0);
    --card-foreground: oklch(0.145 0 0);
    --popover: oklch(1 0 0);
    --popover-foreground: oklch(0.145 0 0);
    --primary: oklch(0.205 0 0);
    --primary-foreground: oklch(0.985 0 0);
    --secondary: oklch(0.97 0 0);
    --secondary-foreground: oklch(0.205 0 0);
    --muted: oklch(0.97 0 0);
    --muted-foreground: oklch(0.556 0 0);
    --accent: oklch(0.97 0 0);
    --accent-foreground: oklch(0.205 0 0);
    --destructive: oklch(0.577 0.245 27.325);
    --destructive-foreground: oklch(0.97 0.01 17);
    --border: oklch(0.922 0 0);
    --input: oklch(0.922 0 0);
    --ring: oklch(0.708 0 0);
    --chart-1: oklch(0.646 0.222 41.116);
    --chart-2: oklch(0.6 0.118 184.704);
    --chart-3: oklch(0.398 0.07 227.392);
    --chart-4: oklch(0.828 0.189 84.429);
    --chart-5: oklch(0.769 0.188 70.08);
    --sidebar: oklch(0.985 0 0);
    --sidebar-foreground: oklch(0.145 0 0);
    --sidebar-primary: oklch(0.205 0 0);
    --sidebar-primary-foreground: oklch(0.985 0 0);
    --sidebar-accent: oklch(0.97 0 0);
    --sidebar-accent-foreground: oklch(0.205 0 0);
    --sidebar-border: oklch(0.922 0 0);
    --sidebar-ring: oklch(0.708 0 0);
}

.dark {
    --background: oklch(0.145 0 0);
    --foreground: oklch(0.985 0 0);
    --card: oklch(0.205 0 0);
    --card-foreground: oklch(0.985 0 0);
    --popover: oklch(0.205 0 0);
    --popover-foreground: oklch(0.985 0 0);
    --primary: oklch(0.922 0 0);
    --primary-foreground: oklch(0.205 0 0);
    --secondary: oklch(0.269 0 0);
    --secondary-foreground: oklch(0.985 0 0);
    --muted: oklch(0.269 0 0);
    --muted-foreground: oklch(0.708 0 0);
    --accent: oklch(0.371 0 0);
    --accent-foreground: oklch(0.985 0 0);
    --destructive: oklch(0.704 0.191 22.216);
    --destructive-foreground: oklch(0.58 0.22 27);
    --border: oklch(1 0 0 / 10%);
    --input: oklch(1 0 0 / 15%);
    --ring: oklch(0.556 0 0);
    --chart-1: oklch(0.488 0.243 264.376);
    --chart-2: oklch(0.696 0.17 162.48);
    --chart-3: oklch(0.769 0.188 70.08);
    --chart-4: oklch(0.627 0.265 303.9);
    --chart-5: oklch(0.645 0.246 16.439);
    --sidebar: oklch(0.205 0 0);
    --sidebar-foreground: oklch(0.985 0 0);
    --sidebar-primary: oklch(0.488 0.243 264.376);
    --sidebar-primary-foreground: oklch(0.985 0 0);
    --sidebar-accent: oklch(0.269 0 0);
    --sidebar-accent-foreground: oklch(0.985 0 0);
    --sidebar-border: oklch(1 0 0 / 10%);
    --sidebar-ring: oklch(0.439 0 0);
}

/* Base reset for BlazorCN components */
*, *::before, *::after {
    border-color: var(--border);
}
```

**Step 2: Verify the file is included as static web asset**

```bash
dotnet build src/BlazorCN/BlazorCN.csproj
```
Expected: Build succeeded. File will be served at `_content/BlazorCN/blazorcn.css`.

**Step 3: Commit**

```bash
git add src/BlazorCN/wwwroot/blazorcn.css
git commit -m "feat: add blazorcn.css with shadcn-ui theme variables (oklch)"
```

---

### Task 8: Create JS interop shell (blazorcn.js)

**Files:**
- Create: `src/BlazorCN/wwwroot/blazorcn.js`

**Step 1: Write the initial JS module**

```javascript
// BlazorCN JS Interop — minimal behaviors that CSS can't handle

/** @type {Map<string, AbortController>} */
const cleanupMap = new Map();

/**
 * Traps focus within an element (for modals/dialogs).
 * @param {HTMLElement} element
 * @param {string} id - unique ID for cleanup
 */
export function trapFocus(element, id) {
    if (!element) return;
    cleanup(id);

    const controller = new AbortController();
    cleanupMap.set(id, controller);

    const focusable = getFocusableElements(element);
    if (focusable.length === 0) return;

    focusable[0].focus();

    element.addEventListener('keydown', (e) => {
        if (e.key !== 'Tab') return;

        const currentFocusable = getFocusableElements(element);
        const first = currentFocusable[0];
        const last = currentFocusable[currentFocusable.length - 1];

        if (e.shiftKey) {
            if (document.activeElement === first) {
                e.preventDefault();
                last.focus();
            }
        } else {
            if (document.activeElement === last) {
                e.preventDefault();
                first.focus();
            }
        }
    }, { signal: controller.signal });
}

/**
 * Detects clicks outside an element.
 * @param {HTMLElement} element
 * @param {string} id - unique ID for cleanup
 * @param {object} dotnetRef - .NET object reference for callback
 * @param {string} methodName - .NET method to invoke
 */
export function onOutsideClick(element, id, dotnetRef, methodName) {
    if (!element) return;
    cleanup(id);

    const controller = new AbortController();
    cleanupMap.set(id, controller);

    // Delay to avoid catching the opening click
    setTimeout(() => {
        document.addEventListener('pointerdown', (e) => {
            if (!element.contains(e.target)) {
                dotnetRef.invokeMethodAsync(methodName);
            }
        }, { signal: controller.signal });
    }, 0);
}

/**
 * Locks body scroll (for modals).
 * @param {string} id
 */
export function lockScroll(id) {
    cleanup(id);
    const scrollY = window.scrollY;
    document.body.style.position = 'fixed';
    document.body.style.top = `-${scrollY}px`;
    document.body.style.left = '0';
    document.body.style.right = '0';
    document.body.style.overflow = 'hidden';
    cleanupMap.set(id, { abort: () => unlockScrollInternal(scrollY) });
}

function unlockScrollInternal(scrollY) {
    document.body.style.position = '';
    document.body.style.top = '';
    document.body.style.left = '';
    document.body.style.right = '';
    document.body.style.overflow = '';
    window.scrollTo(0, scrollY);
}

/**
 * Cleans up event listeners/state for a given ID.
 * @param {string} id
 */
export function cleanup(id) {
    const existing = cleanupMap.get(id);
    if (existing) {
        if (typeof existing.abort === 'function') existing.abort();
        cleanupMap.delete(id);
    }
}

/**
 * Gets all focusable elements within a container.
 * @param {HTMLElement} container
 * @returns {HTMLElement[]}
 */
function getFocusableElements(container) {
    return [...container.querySelectorAll(
        'a[href], button:not([disabled]), textarea:not([disabled]), input:not([disabled]):not([type="hidden"]), select:not([disabled]), [tabindex]:not([tabindex="-1"])'
    )];
}
```

**Step 2: Verify build**

```bash
dotnet build src/BlazorCN/BlazorCN.csproj
```

**Step 3: Commit**

```bash
git add src/BlazorCN/wwwroot/blazorcn.js
git commit -m "feat: add blazorcn.js with focus trap, outside click, and scroll lock"
```

---

### Task 9: Create JsInteropCn service and DI registration

**Files:**
- Create: `src/BlazorCN/JsInteropCn.cs`
- Create: `src/BlazorCN/ServiceCollectionExtensions.cs`

**Step 1: Write JsInteropCn**

```csharp
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorCN;

/// <summary>
/// Typed wrapper for BlazorCN JavaScript interop calls.
/// </summary>
public sealed class JsInteropCn : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private IJSObjectReference? _module;

    public JsInteropCn(IJSRuntime js)
    {
        _js = js;
    }

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        return _module ??= await _js.InvokeAsync<IJSObjectReference>(
            "import", "./_content/BlazorCN/blazorcn.js");
    }

    public async ValueTask TrapFocusAsync(ElementReference element, string id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("trapFocus", element, id);
    }

    public async ValueTask OnOutsideClickAsync<T>(
        ElementReference element, string id,
        DotNetObjectReference<T> dotnetRef, string methodName) where T : class
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("onOutsideClick", element, id, dotnetRef, methodName);
    }

    public async ValueTask LockScrollAsync(string id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("lockScroll", id);
    }

    public async ValueTask CleanupAsync(string id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("cleanup", id);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}
```

**Step 2: Write ServiceCollectionExtensions**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlazorCN;

/// <summary>
/// Extension methods for registering BlazorCN services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds BlazorCN services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddBlazorCN(this IServiceCollection services)
    {
        services.TryAddScoped<JsInteropCn>();
        return services;
    }
}
```

**Step 3: Verify build**

```bash
dotnet build src/BlazorCN/BlazorCN.csproj
```

**Step 4: Commit**

```bash
git add src/BlazorCN/JsInteropCn.cs src/BlazorCN/ServiceCollectionExtensions.cs
git commit -m "feat: add JsInteropCn service and AddBlazorCN() DI registration"
```

---

### Task 10: Delete scaffolded template files

**Files:**
- Delete: `src/BlazorCN/Component1.razor` (template file)
- Delete: `src/BlazorCN/ExampleJsInterop.cs` (template file)
- Delete: `src/BlazorCN/wwwroot/background.png` (template file)
- Delete: `src/BlazorCN/wwwroot/exampleJsInterop.js` (template file)

**Step 1: Remove template files created by `dotnet new razorclasslib`**

```bash
rm -f src/BlazorCN/Component1.razor src/BlazorCN/ExampleJsInterop.cs
rm -f src/BlazorCN/wwwroot/background.png src/BlazorCN/wwwroot/exampleJsInterop.js
```

**Step 2: Verify build still passes**

```bash
dotnet build BlazorCN.slnx
```

**Step 3: Commit**

```bash
git add -A
git commit -m "chore: remove scaffolded template files"
```

---

## Phase 2: Simple Components (No JS)

### Task 11: ButtonCn component

**Files:**
- Create: `src/BlazorCN/Components/Button/ButtonVariant.cs`
- Create: `src/BlazorCN/Components/Button/ButtonSize.cs`
- Create: `src/BlazorCN/Components/Button/ButtonCn.razor`
- Test: `tests/BlazorCN.Tests/Components/ButtonCnTests.cs`

**Step 1: Write the tests**

```csharp
using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class ButtonCnTests : TestContext
{
    [Fact]
    public void Renders_Default_Button()
    {
        var cut = RenderComponent<ButtonCn>(p => p
            .AddChildContent("Click me"));
        var button = cut.Find("button");
        button.TextContent.Trim().Should().Be("Click me");
        button.ClassList.Should().Contain("bg-primary");
    }

    [Fact]
    public void Renders_Destructive_Variant()
    {
        var cut = RenderComponent<ButtonCn>(p => p
            .Add(b => b.Variant, ButtonVariant.Destructive)
            .AddChildContent("Delete"));
        cut.Find("button").ClassList.Should().Contain("bg-destructive");
    }

    [Fact]
    public void Renders_Outline_Variant()
    {
        var cut = RenderComponent<ButtonCn>(p => p
            .Add(b => b.Variant, ButtonVariant.Outline)
            .AddChildContent("Outline"));
        var button = cut.Find("button");
        button.ClassList.Should().Contain("border");
        button.ClassList.Should().Contain("bg-background");
    }

    [Fact]
    public void Renders_Small_Size()
    {
        var cut = RenderComponent<ButtonCn>(p => p
            .Add(b => b.Size, ButtonSize.Sm)
            .AddChildContent("Small"));
        cut.Find("button").ClassList.Should().Contain("h-8");
    }

    [Fact]
    public void Renders_Disabled_State()
    {
        var cut = RenderComponent<ButtonCn>(p => p
            .Add(b => b.Disabled, true)
            .AddChildContent("Disabled"));
        cut.Find("button").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Passes_Additional_Classes()
    {
        var cut = RenderComponent<ButtonCn>(p => p
            .Add(b => b.Class, "mt-4")
            .AddChildContent("Styled"));
        cut.Find("button").ClassList.Should().Contain("mt-4");
    }

    [Fact]
    public void Fires_OnClick_Event()
    {
        var clicked = false;
        var cut = RenderComponent<ButtonCn>(p => p
            .Add(b => b.OnClick, () => { clicked = true; })
            .AddChildContent("Click"));
        cut.Find("button").Click();
        clicked.Should().BeTrue();
    }

    [Fact]
    public void Passes_Additional_Attributes()
    {
        var cut = RenderComponent<ButtonCn>(p => p
            .AddUnmatched("data-testid", "my-button")
            .AddChildContent("Test"));
        cut.Find("button").GetAttribute("data-testid").Should().Be("my-button");
    }

    [Fact]
    public void Renders_As_Anchor_When_Href_Provided()
    {
        var cut = RenderComponent<ButtonCn>(p => p
            .Add(b => b.Href, "https://example.com")
            .AddChildContent("Link"));
        cut.Find("a").Should().NotBeNull();
        cut.Find("a").GetAttribute("href").Should().Be("https://example.com");
    }
}
```

**Step 2: Run tests — should fail**

```bash
dotnet test tests/BlazorCN.Tests/BlazorCN.Tests.csproj --filter "FullyQualifiedName~ButtonCnTests"
```

**Step 3: Create enums**

`ButtonVariant.cs`:
```csharp
namespace BlazorCN;

public enum ButtonVariant
{
    Default,
    Destructive,
    Outline,
    Secondary,
    Ghost,
    Link
}
```

`ButtonSize.cs`:
```csharp
namespace BlazorCN;

public enum ButtonSize
{
    Default,
    Xs,
    Sm,
    Lg,
    Icon,
    IconXs,
    IconSm,
    IconLg
}
```

**Step 4: Create ButtonCn.razor**

```razor
@namespace BlazorCN
@inherits ComponentBaseCn

@if (!string.IsNullOrEmpty(Href))
{
    <a href="@Href"
       class="@CssClass"
       style="@Style"
       data-slot="button"
       data-variant="@Variant.ToString().ToLowerInvariant()"
       data-size="@Size.ToString().ToLowerInvariant()"
       @attributes="AdditionalAttributes">
        @ChildContent
    </a>
}
else
{
    <button class="@CssClass"
            style="@Style"
            type="@Type"
            disabled="@Disabled"
            data-slot="button"
            data-variant="@Variant.ToString().ToLowerInvariant()"
            data-size="@Size.ToString().ToLowerInvariant()"
            @onclick="OnClick"
            @attributes="AdditionalAttributes">
        @ChildContent
    </button>
}

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public ButtonVariant Variant { get; set; } = ButtonVariant.Default;
    [Parameter] public ButtonSize Size { get; set; } = ButtonSize.Default;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string Type { get; set; } = "button";
    [Parameter] public string? Href { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }

    private static readonly Cva<ButtonVariant, ButtonSize> Variants = new(
        "inline-flex shrink-0 items-center justify-center gap-2 rounded-md text-sm font-medium whitespace-nowrap transition-all outline-none focus-visible:border-ring focus-visible:ring-[3px] focus-visible:ring-ring/50 disabled:pointer-events-none disabled:opacity-50 [&_svg]:pointer-events-none [&_svg]:shrink-0 [&_svg:not([class*='size-'])]:size-4",
        new()
        {
            [ButtonVariant.Default] = "bg-primary text-primary-foreground hover:bg-primary/90",
            [ButtonVariant.Destructive] = "bg-destructive text-white hover:bg-destructive/90 focus-visible:ring-destructive/20 dark:bg-destructive/60 dark:focus-visible:ring-destructive/40",
            [ButtonVariant.Outline] = "border bg-background shadow-xs hover:bg-accent hover:text-accent-foreground dark:border-input dark:bg-input/30 dark:hover:bg-input/50",
            [ButtonVariant.Secondary] = "bg-secondary text-secondary-foreground hover:bg-secondary/80",
            [ButtonVariant.Ghost] = "hover:bg-accent hover:text-accent-foreground dark:hover:bg-accent/50",
            [ButtonVariant.Link] = "text-primary underline-offset-4 hover:underline",
        },
        new()
        {
            [ButtonSize.Default] = "h-9 px-4 py-2 has-[>svg]:px-3",
            [ButtonSize.Xs] = "h-6 gap-1 rounded-md px-2 text-xs has-[>svg]:px-1.5 [&_svg:not([class*='size-'])]:size-3",
            [ButtonSize.Sm] = "h-8 gap-1.5 rounded-md px-3 has-[>svg]:px-2.5",
            [ButtonSize.Lg] = "h-10 rounded-md px-6 has-[>svg]:px-4",
            [ButtonSize.Icon] = "size-9",
            [ButtonSize.IconXs] = "size-6 rounded-md [&_svg:not([class*='size-'])]:size-3",
            [ButtonSize.IconSm] = "size-8",
            [ButtonSize.IconLg] = "size-10",
        });

    private string CssClass => Variants.Apply(Variant, Size, Class);
}
```

**Step 5: Run tests — should pass**

```bash
dotnet test tests/BlazorCN.Tests/BlazorCN.Tests.csproj --filter "FullyQualifiedName~ButtonCnTests"
```
Expected: All 9 tests pass.

**Step 6: Commit**

```bash
git add src/BlazorCN/Components/Button/ tests/BlazorCN.Tests/Components/ButtonCnTests.cs
git commit -m "feat: add ButtonCn component with all shadcn-ui variants and sizes"
```

---

### Task 12: BadgeCn component

**Files:**
- Create: `src/BlazorCN/Components/Badge/BadgeVariant.cs`
- Create: `src/BlazorCN/Components/Badge/BadgeCn.razor`
- Test: `tests/BlazorCN.Tests/Components/BadgeCnTests.cs`

**Step 1: Write the tests**

```csharp
using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class BadgeCnTests : TestContext
{
    [Fact]
    public void Renders_Default_Badge()
    {
        var cut = RenderComponent<BadgeCn>(p => p.AddChildContent("New"));
        var span = cut.Find("span");
        span.TextContent.Trim().Should().Be("New");
        span.ClassList.Should().Contain("bg-primary");
    }

    [Fact]
    public void Renders_Destructive_Variant()
    {
        var cut = RenderComponent<BadgeCn>(p => p
            .Add(b => b.Variant, BadgeVariant.Destructive)
            .AddChildContent("Error"));
        cut.Find("span").ClassList.Should().Contain("bg-destructive");
    }

    [Fact]
    public void Renders_Outline_Variant()
    {
        var cut = RenderComponent<BadgeCn>(p => p
            .Add(b => b.Variant, BadgeVariant.Outline)
            .AddChildContent("Tag"));
        cut.Find("span").ClassList.Should().Contain("border-border");
    }

    [Fact]
    public void Passes_Additional_Classes()
    {
        var cut = RenderComponent<BadgeCn>(p => p
            .Add(b => b.Class, "ml-2")
            .AddChildContent("Tag"));
        cut.Find("span").ClassList.Should().Contain("ml-2");
    }
}
```

**Step 2: Create BadgeVariant enum**

```csharp
namespace BlazorCN;

public enum BadgeVariant
{
    Default,
    Secondary,
    Destructive,
    Outline,
    Ghost,
    Link
}
```

**Step 3: Create BadgeCn.razor**

Uses a single-variant Cva pattern (no size dimension). We need a simpler Cva overload. Instead, use `Cn.Merge()` directly with a helper method.

```razor
@namespace BlazorCN
@inherits ComponentBaseCn

<span class="@CssClass"
      style="@Style"
      data-slot="badge"
      data-variant="@Variant.ToString().ToLowerInvariant()"
      @attributes="AdditionalAttributes">
    @ChildContent
</span>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public BadgeVariant Variant { get; set; } = BadgeVariant.Default;

    private static readonly string Base =
        "inline-flex w-fit shrink-0 items-center justify-center gap-1 overflow-hidden rounded-full border border-transparent px-2 py-0.5 text-xs font-medium whitespace-nowrap transition-[color,box-shadow] focus-visible:border-ring focus-visible:ring-[3px] focus-visible:ring-ring/50 [&>svg]:pointer-events-none [&>svg]:size-3";

    private static readonly Dictionary<BadgeVariant, string> VariantMap = new()
    {
        [BadgeVariant.Default] = "bg-primary text-primary-foreground [a&]:hover:bg-primary/90",
        [BadgeVariant.Secondary] = "bg-secondary text-secondary-foreground [a&]:hover:bg-secondary/90",
        [BadgeVariant.Destructive] = "bg-destructive text-white focus-visible:ring-destructive/20 dark:bg-destructive/60 dark:focus-visible:ring-destructive/40 [a&]:hover:bg-destructive/90",
        [BadgeVariant.Outline] = "border-border text-foreground [a&]:hover:bg-accent [a&]:hover:text-accent-foreground",
        [BadgeVariant.Ghost] = "[a&]:hover:bg-accent [a&]:hover:text-accent-foreground",
        [BadgeVariant.Link] = "text-primary underline-offset-4 [a&]:hover:underline",
    };

    private string CssClass => Cn.Merge(Base, VariantMap.GetValueOrDefault(Variant, ""), Class);
}
```

**Step 4: Run tests — should pass**

```bash
dotnet test tests/BlazorCN.Tests/BlazorCN.Tests.csproj --filter "FullyQualifiedName~BadgeCnTests"
```

**Step 5: Commit**

```bash
git add src/BlazorCN/Components/Badge/ tests/BlazorCN.Tests/Components/BadgeCnTests.cs
git commit -m "feat: add BadgeCn component with all shadcn-ui variants"
```

---

### Task 13: CardCn composed components

**Files:**
- Create: `src/BlazorCN/Components/Card/CardCn.razor`
- Create: `src/BlazorCN/Components/Card/CardHeaderCn.razor`
- Create: `src/BlazorCN/Components/Card/CardTitleCn.razor`
- Create: `src/BlazorCN/Components/Card/CardDescriptionCn.razor`
- Create: `src/BlazorCN/Components/Card/CardActionCn.razor`
- Create: `src/BlazorCN/Components/Card/CardContentCn.razor`
- Create: `src/BlazorCN/Components/Card/CardFooterCn.razor`
- Test: `tests/BlazorCN.Tests/Components/CardCnTests.cs`

**Step 1: Write the tests**

```csharp
using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class CardCnTests : TestContext
{
    [Fact]
    public void CardCn_Renders_With_Default_Classes()
    {
        var cut = RenderComponent<CardCn>(p => p.AddChildContent("Content"));
        var div = cut.Find("[data-slot='card']");
        div.ClassList.Should().Contain("rounded-xl");
        div.ClassList.Should().Contain("border");
        div.ClassList.Should().Contain("bg-card");
    }

    [Fact]
    public void CardHeaderCn_Renders()
    {
        var cut = RenderComponent<CardHeaderCn>(p => p.AddChildContent("Header"));
        cut.Find("[data-slot='card-header']").TextContent.Trim().Should().Be("Header");
    }

    [Fact]
    public void CardTitleCn_Renders()
    {
        var cut = RenderComponent<CardTitleCn>(p => p.AddChildContent("Title"));
        var el = cut.Find("[data-slot='card-title']");
        el.TextContent.Trim().Should().Be("Title");
        el.ClassList.Should().Contain("font-semibold");
    }

    [Fact]
    public void CardContentCn_Renders()
    {
        var cut = RenderComponent<CardContentCn>(p => p.AddChildContent("Body"));
        cut.Find("[data-slot='card-content']").ClassList.Should().Contain("px-6");
    }

    [Fact]
    public void CardFooterCn_Renders()
    {
        var cut = RenderComponent<CardFooterCn>(p => p.AddChildContent("Footer"));
        cut.Find("[data-slot='card-footer']").ClassList.Should().Contain("flex");
    }

    [Fact]
    public void Card_Passes_Additional_Classes()
    {
        var cut = RenderComponent<CardCn>(p => p
            .Add(c => c.Class, "w-[350px]")
            .AddChildContent("Content"));
        cut.Find("[data-slot='card']").ClassList.Should().Contain("w-[350px]");
    }
}
```

**Step 2: Create each Card sub-component**

`CardCn.razor`:
```razor
@namespace BlazorCN
@inherits ComponentBaseCn

<div class="@Cn.Merge("flex flex-col gap-6 rounded-xl border bg-card py-6 text-card-foreground shadow-sm", Class)"
     style="@Style"
     data-slot="card"
     @attributes="AdditionalAttributes">
    @ChildContent
</div>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

`CardHeaderCn.razor`:
```razor
@namespace BlazorCN
@inherits ComponentBaseCn

<div class="@Cn.Merge("@container/card-header grid auto-rows-min grid-rows-[auto_auto] items-start gap-2 px-6 has-data-[slot=card-action]:grid-cols-[1fr_auto] [.border-b]:pb-6", Class)"
     style="@Style"
     data-slot="card-header"
     @attributes="AdditionalAttributes">
    @ChildContent
</div>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

`CardTitleCn.razor`:
```razor
@namespace BlazorCN
@inherits ComponentBaseCn

<div class="@Cn.Merge("leading-none font-semibold", Class)"
     style="@Style"
     data-slot="card-title"
     @attributes="AdditionalAttributes">
    @ChildContent
</div>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

`CardDescriptionCn.razor`:
```razor
@namespace BlazorCN
@inherits ComponentBaseCn

<div class="@Cn.Merge("text-sm text-muted-foreground", Class)"
     style="@Style"
     data-slot="card-description"
     @attributes="AdditionalAttributes">
    @ChildContent
</div>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

`CardActionCn.razor`:
```razor
@namespace BlazorCN
@inherits ComponentBaseCn

<div class="@Cn.Merge("col-start-2 row-span-2 row-start-1 self-start justify-self-end", Class)"
     style="@Style"
     data-slot="card-action"
     @attributes="AdditionalAttributes">
    @ChildContent
</div>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

`CardContentCn.razor`:
```razor
@namespace BlazorCN
@inherits ComponentBaseCn

<div class="@Cn.Merge("px-6", Class)"
     style="@Style"
     data-slot="card-content"
     @attributes="AdditionalAttributes">
    @ChildContent
</div>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

`CardFooterCn.razor`:
```razor
@namespace BlazorCN
@inherits ComponentBaseCn

<div class="@Cn.Merge("flex items-center px-6 [.border-t]:pt-6", Class)"
     style="@Style"
     data-slot="card-footer"
     @attributes="AdditionalAttributes">
    @ChildContent
</div>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

**Step 3: Run tests — should pass**

```bash
dotnet test tests/BlazorCN.Tests/BlazorCN.Tests.csproj --filter "FullyQualifiedName~CardCnTests"
```

**Step 4: Commit**

```bash
git add src/BlazorCN/Components/Card/ tests/BlazorCN.Tests/Components/CardCnTests.cs
git commit -m "feat: add CardCn composed components (Card, Header, Title, Description, Action, Content, Footer)"
```

---

### Task 14: InputCn component

**Files:**
- Create: `src/BlazorCN/Components/Input/InputCn.razor`
- Test: `tests/BlazorCN.Tests/Components/InputCnTests.cs`

**Step 1: Write the tests**

```csharp
using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class InputCnTests : TestContext
{
    [Fact]
    public void Renders_Input_With_Default_Classes()
    {
        var cut = RenderComponent<InputCn>();
        var input = cut.Find("input");
        input.ClassList.Should().Contain("h-9");
        input.ClassList.Should().Contain("w-full");
        input.ClassList.Should().Contain("rounded-md");
    }

    [Fact]
    public void Renders_With_Placeholder()
    {
        var cut = RenderComponent<InputCn>(p => p
            .Add(i => i.Placeholder, "Enter text..."));
        cut.Find("input").GetAttribute("placeholder").Should().Be("Enter text...");
    }

    [Fact]
    public void Renders_With_Type()
    {
        var cut = RenderComponent<InputCn>(p => p
            .Add(i => i.Type, "email"));
        cut.Find("input").GetAttribute("type").Should().Be("email");
    }

    [Fact]
    public void Supports_Two_Way_Binding()
    {
        var value = "initial";
        var cut = RenderComponent<InputCn>(p => p
            .Add(i => i.Value, value)
            .Add(i => i.ValueChanged, (string v) => value = v));
        cut.Find("input").Change("updated");
        value.Should().Be("updated");
    }

    [Fact]
    public void Renders_Disabled_State()
    {
        var cut = RenderComponent<InputCn>(p => p
            .Add(i => i.Disabled, true));
        cut.Find("input").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Passes_Additional_Classes()
    {
        var cut = RenderComponent<InputCn>(p => p
            .Add(i => i.Class, "max-w-sm"));
        cut.Find("input").ClassList.Should().Contain("max-w-sm");
    }
}
```

**Step 2: Create InputCn.razor**

```razor
@namespace BlazorCN
@inherits ComponentBaseCn

<input type="@Type"
       class="@CssClass"
       style="@Style"
       value="@Value"
       placeholder="@Placeholder"
       disabled="@Disabled"
       data-slot="input"
       @oninput="HandleInput"
       @attributes="AdditionalAttributes" />

@code {
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public string Type { get; set; } = "text";
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public bool Disabled { get; set; }

    private static readonly string BaseClasses =
        "h-9 w-full min-w-0 rounded-md border border-input bg-transparent px-3 py-1 text-base shadow-xs transition-[color,box-shadow] outline-none selection:bg-primary selection:text-primary-foreground file:inline-flex file:h-7 file:border-0 file:bg-transparent file:text-sm file:font-medium file:text-foreground placeholder:text-muted-foreground disabled:pointer-events-none disabled:cursor-not-allowed disabled:opacity-50 md:text-sm dark:bg-input/30 focus-visible:border-ring focus-visible:ring-[3px] focus-visible:ring-ring/50 aria-invalid:border-destructive aria-invalid:ring-destructive/20 dark:aria-invalid:ring-destructive/40";

    private string CssClass => Cn.Merge(BaseClasses, Class);

    private async Task HandleInput(ChangeEventArgs e)
    {
        await ValueChanged.InvokeAsync(e.Value?.ToString());
    }
}
```

**Step 3: Run tests — should pass**

```bash
dotnet test tests/BlazorCN.Tests/BlazorCN.Tests.csproj --filter "FullyQualifiedName~InputCnTests"
```

**Step 4: Commit**

```bash
git add src/BlazorCN/Components/Input/ tests/BlazorCN.Tests/Components/InputCnTests.cs
git commit -m "feat: add InputCn component with two-way binding"
```

---

### Task 15: LabelCn, SeparatorCn, SkeletonCn, TextareaCn

**Files:**
- Create: `src/BlazorCN/Components/Label/LabelCn.razor`
- Create: `src/BlazorCN/Components/Separator/SeparatorCn.razor`
- Create: `src/BlazorCN/Components/Skeleton/SkeletonCn.razor`
- Create: `src/BlazorCN/Components/Textarea/TextareaCn.razor`
- Test: `tests/BlazorCN.Tests/Components/SimpleComponentsTests.cs`

These are all thin wrappers — group them in one task.

**Step 1: Write the tests**

```csharp
using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class SimpleComponentsTests : TestContext
{
    [Fact]
    public void LabelCn_Renders()
    {
        var cut = RenderComponent<LabelCn>(p => p.AddChildContent("Email"));
        var label = cut.Find("label");
        label.TextContent.Trim().Should().Be("Email");
        label.GetAttribute("data-slot").Should().Be("label");
    }

    [Fact]
    public void LabelCn_Supports_For_Attribute()
    {
        var cut = RenderComponent<LabelCn>(p => p
            .Add(l => l.For, "email-input")
            .AddChildContent("Email"));
        cut.Find("label").GetAttribute("for").Should().Be("email-input");
    }

    [Fact]
    public void SeparatorCn_Renders_Horizontal()
    {
        var cut = RenderComponent<SeparatorCn>();
        var el = cut.Find("[data-slot='separator']");
        el.GetAttribute("role").Should().Be("separator");
    }

    [Fact]
    public void SeparatorCn_Renders_Vertical()
    {
        var cut = RenderComponent<SeparatorCn>(p => p
            .Add(s => s.Orientation, Orientation.Vertical));
        var el = cut.Find("[data-slot='separator']");
        el.GetAttribute("data-orientation").Should().Be("vertical");
    }

    [Fact]
    public void SkeletonCn_Renders()
    {
        var cut = RenderComponent<SkeletonCn>();
        var div = cut.Find("[data-slot='skeleton']");
        div.ClassList.Should().Contain("animate-pulse");
        div.ClassList.Should().Contain("rounded-md");
    }

    [Fact]
    public void TextareaCn_Renders()
    {
        var cut = RenderComponent<TextareaCn>(p => p
            .Add(t => t.Placeholder, "Type here..."));
        var textarea = cut.Find("textarea");
        textarea.GetAttribute("placeholder").Should().Be("Type here...");
        textarea.GetAttribute("data-slot").Should().Be("textarea");
    }

    [Fact]
    public void TextareaCn_Supports_Two_Way_Binding()
    {
        var value = "";
        var cut = RenderComponent<TextareaCn>(p => p
            .Add(t => t.Value, value)
            .Add(t => t.ValueChanged, (string v) => value = v));
        cut.Find("textarea").Change("hello");
        value.Should().Be("hello");
    }
}
```

**Step 2: Create Orientation enum**

```csharp
namespace BlazorCN;

public enum Orientation
{
    Horizontal,
    Vertical
}
```

**Step 3: Create the four components**

`LabelCn.razor`:
```razor
@namespace BlazorCN
@inherits ComponentBaseCn

<label class="@Cn.Merge("flex items-center gap-2 text-sm leading-none font-medium select-none group-data-[disabled=true]:pointer-events-none group-data-[disabled=true]:opacity-50 peer-disabled:cursor-not-allowed peer-disabled:opacity-50", Class)"
       style="@Style"
       for="@For"
       data-slot="label"
       @attributes="AdditionalAttributes">
    @ChildContent
</label>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? For { get; set; }
}
```

`SeparatorCn.razor`:
```razor
@namespace BlazorCN
@inherits ComponentBaseCn

<div class="@CssClass"
     style="@Style"
     role="separator"
     data-slot="separator"
     data-orientation="@Orientation.ToString().ToLowerInvariant()"
     aria-orientation="@(Orientation == Orientation.Vertical ? "vertical" : null)"
     @attributes="AdditionalAttributes">
</div>

@code {
    [Parameter] public Orientation Orientation { get; set; } = Orientation.Horizontal;
    [Parameter] public bool Decorative { get; set; } = true;

    private string CssClass => Cn.Merge(
        "shrink-0 bg-border",
        Orientation == Orientation.Horizontal ? "h-px w-full" : "h-full w-px",
        Class);
}
```

`SkeletonCn.razor`:
```razor
@namespace BlazorCN
@inherits ComponentBaseCn

<div class="@Cn.Merge("animate-pulse rounded-md bg-accent", Class)"
     style="@Style"
     data-slot="skeleton"
     @attributes="AdditionalAttributes">
</div>
```

`TextareaCn.razor`:
```razor
@namespace BlazorCN
@inherits ComponentBaseCn

<textarea class="@CssClass"
          style="@Style"
          placeholder="@Placeholder"
          disabled="@Disabled"
          data-slot="textarea"
          @oninput="HandleInput"
          @attributes="AdditionalAttributes">@Value</textarea>

@code {
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public bool Disabled { get; set; }

    private static readonly string BaseClasses =
        "flex field-sizing-content min-h-16 w-full rounded-md border border-input bg-transparent px-3 py-2 text-base shadow-xs transition-[color,box-shadow] outline-none placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-[3px] focus-visible:ring-ring/50 disabled:cursor-not-allowed disabled:opacity-50 aria-invalid:border-destructive aria-invalid:ring-destructive/20 md:text-sm dark:bg-input/30 dark:aria-invalid:ring-destructive/40";

    private string CssClass => Cn.Merge(BaseClasses, Class);

    private async Task HandleInput(ChangeEventArgs e)
    {
        await ValueChanged.InvokeAsync(e.Value?.ToString());
    }
}
```

**Step 4: Run tests — should pass**

```bash
dotnet test tests/BlazorCN.Tests/BlazorCN.Tests.csproj --filter "FullyQualifiedName~SimpleComponentsTests"
```

**Step 5: Commit**

```bash
git add src/BlazorCN/Components/Label/ src/BlazorCN/Components/Separator/ src/BlazorCN/Components/Skeleton/ src/BlazorCN/Components/Textarea/ src/BlazorCN/Components/Separator/Orientation.cs tests/BlazorCN.Tests/Components/SimpleComponentsTests.cs
git commit -m "feat: add LabelCn, SeparatorCn, SkeletonCn, TextareaCn components"
```

---

### Task 16: AlertCn composed component

**Files:**
- Create: `src/BlazorCN/Components/Alert/AlertVariant.cs`
- Create: `src/BlazorCN/Components/Alert/AlertCn.razor`
- Create: `src/BlazorCN/Components/Alert/AlertTitleCn.razor`
- Create: `src/BlazorCN/Components/Alert/AlertDescriptionCn.razor`
- Test: `tests/BlazorCN.Tests/Components/AlertCnTests.cs`

Follow the same pattern as CardCn — use exact shadcn-ui classes from the reference.

`AlertVariant.cs`:
```csharp
namespace BlazorCN;
public enum AlertVariant { Default, Destructive }
```

`AlertCn.razor`:
```razor
@namespace BlazorCN
@inherits ComponentBaseCn

<div class="@CssClass" style="@Style" role="alert" data-slot="alert" @attributes="AdditionalAttributes">
    @ChildContent
</div>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public AlertVariant Variant { get; set; } = AlertVariant.Default;

    private static readonly string Base =
        "relative grid w-full grid-cols-[0_1fr] items-start gap-y-0.5 rounded-lg border px-4 py-3 text-sm has-[>svg]:grid-cols-[calc(var(--spacing)*4)_1fr] has-[>svg]:gap-x-3 [&>svg]:size-4 [&>svg]:translate-y-0.5 [&>svg]:text-current";

    private static readonly Dictionary<AlertVariant, string> VariantMap = new()
    {
        [AlertVariant.Default] = "bg-card text-card-foreground",
        [AlertVariant.Destructive] = "bg-card text-destructive *:data-[slot=alert-description]:text-destructive/90 [&>svg]:text-current",
    };

    private string CssClass => Cn.Merge(Base, VariantMap.GetValueOrDefault(Variant, ""), Class);
}
```

`AlertTitleCn.razor`:
```razor
@namespace BlazorCN
@inherits ComponentBaseCn

<div class="@Cn.Merge("col-start-2 line-clamp-1 min-h-4 font-medium tracking-tight", Class)"
     style="@Style" data-slot="alert-title" @attributes="AdditionalAttributes">
    @ChildContent
</div>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

`AlertDescriptionCn.razor`:
```razor
@namespace BlazorCN
@inherits ComponentBaseCn

<div class="@Cn.Merge("col-start-2 grid justify-items-start gap-1 text-sm text-muted-foreground [&_p]:leading-relaxed", Class)"
     style="@Style" data-slot="alert-description" @attributes="AdditionalAttributes">
    @ChildContent
</div>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

Tests, run, commit — same pattern.

**Commit:**
```bash
git add src/BlazorCN/Components/Alert/ tests/BlazorCN.Tests/Components/AlertCnTests.cs
git commit -m "feat: add AlertCn composed component with Default and Destructive variants"
```

---

### Task 17: ToggleCn component

Follow same pattern as ButtonCn with `ToggleVariant` and `ToggleSize` enums, matching shadcn-ui's toggle.tsx classes exactly. Manages pressed state via `@bind-Pressed` parameter.

**Commit message:** `feat: add ToggleCn component with pressed state and variants`

---

### Task 18: ProgressCn, AspectRatioCn, KbdCn, SpinnerCn, EmptyCn

Group remaining simple components. Each is a single thin `.razor` wrapper:

- **ProgressCn** — `<div>` with inner `<div>` for bar, `Value` parameter (0-100)
- **AspectRatioCn** — wrapper `<div>` with `aspect-ratio` style
- **KbdCn** — `<kbd>` element with shadcn-ui keyboard shortcut styling
- **SpinnerCn** — animated SVG spinner
- **EmptyCn** — empty state placeholder

**Commit message:** `feat: add ProgressCn, AspectRatioCn, KbdCn, SpinnerCn, EmptyCn components`

---

## Phase 3: Composed Components (No JS)

### Task 19: AvatarCn (AvatarCn, AvatarImageCn, AvatarFallbackCn)
### Task 20: BreadcrumbCn (7 sub-components)
### Task 21: TableCn (8 sub-components: Table, Header, Body, Footer, Row, Head, Cell, Caption)
### Task 22: TabsCn (TabsCn, TabsListCn, TabsTriggerCn, TabsContentCn) — uses Blazor state, no JS
### Task 23: AccordionCn (AccordionCn, AccordionItemCn, AccordionTriggerCn, AccordionContentCn) — CSS transitions for expand/collapse
### Task 24: CollapsibleCn (CollapsibleCn, CollapsibleTriggerCn, CollapsibleContentCn)
### Task 25: PaginationCn (7 sub-components)
### Task 26: ToggleGroupCn (ToggleGroupCn, ToggleGroupItemCn) — cascading value for single/multi selection
### Task 27: SidebarCn (~12 sub-components) — state managed via CascadingValue, CSS for open/close

Each follows the same TDD pattern: write tests → create components → run tests → commit.

---

## Phase 4: Form Components (No JS)

### Task 28: CheckboxCn — `@bind-Value` bool, custom checkbox styling with SVG check icon
### Task 29: RadioGroupCn, RadioGroupItemCn — cascading value for selection
### Task 30: SwitchCn — toggle switch with `@bind-Value` bool
### Task 31: SliderCn — range input with custom styling, `@bind-Value` double
### Task 32: FormCn (FormCn, FormFieldCn, FormLabelCn, FormControlCn, FormDescriptionCn, FormMessageCn) — EditForm integration
### Task 33: CalendarCn — date picker grid, month/year navigation, selected date state

---

## Phase 5: JS Interop Infrastructure

### Task 34: Add Floating UI to blazorcn.js — bundle @floating-ui/dom for popover/tooltip positioning
### Task 35: Extend JsInteropCn — add `CreateFloatingAsync()`, `UpdateFloatingAsync()`, `DestroyFloatingAsync()` methods
### Task 36: Add keyboard navigation helpers to blazorcn.js — arrow key navigation for menus, escape to close

---

## Phase 6: Interactive Components (Need JS)

### Task 37: DialogCn (8 sub-components) — focus trap, scroll lock, overlay, ESC to close
### Task 38: SheetCn (8 sub-components) — side panel variant of Dialog, slide animations
### Task 39: AlertDialogCn (9 sub-components) — like Dialog but requires explicit action
### Task 40: DrawerCn (8 sub-components) — bottom/side drawer with touch drag
### Task 41: PopoverCn (3 sub-components) — Floating UI positioning, outside click
### Task 42: TooltipCn (3 sub-components) — Floating UI positioning, hover delay
### Task 43: HoverCardCn (3 sub-components) — like Tooltip but with richer content
### Task 44: DropdownMenuCn (~13 sub-components) — Floating UI, keyboard navigation, sub-menus
### Task 45: ContextMenuCn (~13 sub-components) — right-click triggered dropdown
### Task 46: MenubarCn (~14 sub-components) — horizontal menu bar with dropdowns
### Task 47: NavigationMenuCn (~8 sub-components) — navigation with hover-activated content
### Task 48: SelectCn (8 sub-components) — Floating UI, keyboard navigation, @bind-Value
### Task 49: ComboboxCn — searchable select with filtering
### Task 50: CommandCn (7 sub-components) — command palette with search, keyboard navigation

---

## Phase 7: Remaining Components

### Task 51: CarouselCn (5 sub-components) — touch/swipe, auto-play
### Task 52: ResizableCn (3 sub-components) — drag to resize panels
### Task 53: ScrollAreaCn (2 sub-components) — custom scrollbar
### Task 54: InputOtpCn (4 sub-components) — OTP input with focus management
### Task 55: ToasterCn / ToastCn — toast notification service + components
### Task 56: ChartCn — chart wrapper (may integrate with a .NET charting library)

---

## Phase 8: Polish & Release

### Task 57: Run all tests, fix failures
### Task 58: Add XML documentation to all public APIs
### Task 59: Create Tailwind preset file for consumers
### Task 60: Final NuGet pack and verify
### Task 61: Add .gitignore, LICENSE, update CLAUDE.md with final structure

---

**Each task in Phases 3-8 follows the identical TDD pattern established in Phase 2:**
1. Read the corresponding shadcn-ui source from `original/apps/v4/registry/new-york-v4/ui/`
2. Write failing tests
3. Implement component with exact Tailwind classes
4. Run tests to verify
5. Commit

**Total: ~61 tasks, ~200 components, ~50 component groups.**
