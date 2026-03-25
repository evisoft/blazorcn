# Blocks Showcase Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Port all 1,876 shadcn-ui blocks from `original/blocks/` to Blazor as self-contained `.razor` files with inline documentation, organized into 29 category pages in the demo site.

**Architecture:** Each block is a standalone Blazor component (no `@page` directive) living in its own `.razor` file under `docs/BlazorCN.Demo/Pages/Docs/Blocks/{Category}/`. Each category has an index page (`{Category}BlocksPage.razor`) that renders all blocks using `ComponentPreview`. Navigation adds a "Blocks" section as a sidebar peer to "Components".

**Tech Stack:** Blazor (.NET 10), Tailwind CSS, BlazorCN components (Cn suffix), Lucide icon components (LucideXxxCn), minimal JS interop for complex interactions.

---

## Conversion Reference

### React → Blazor Mapping

| React Pattern | Blazor Equivalent |
|---|---|
| `className="..."` | `class="..."` |
| `<Button variant="outline">` | `<ButtonCn Variant="ButtonVariant.Outline">` |
| `<Input value={x} onChange={...} />` | `<InputCn @bind-Value="_x" />` |
| `useState("foo")` | `private string _foo = "foo";` in `@code {}` |
| `{items.map(x => <div>...</div>)}` | `@foreach (var x in items) { <div>...</div> }` |
| `{condition && <div>...</div>}` | `@if (condition) { <div>...</div> }` |
| `{cond ? <A/> : <B/>}` | `@if (cond) { <A/> } else { <B/> }` |
| `<ArrowRight className="h-5 w-5" />` | `<LucideArrowRightCn Size="20" />` |
| `<motion.div>` (framer-motion) | `<div>` (omit animation or use CSS transitions) |
| `cn("class1", cond && "class2")` | `Cn.Merge("class1", cond ? "class2" : null)` |
| `htmlFor="id"` | `For="id"` |
| `<form onSubmit={handler}>` | `<form @onsubmit="Handler" @onsubmit:preventDefault>` |
| TypeScript `interface` | C# `record` or `class` in `@code {}` |
| `const data: Type[] = [...]` | `private static readonly Type[] Data = [...]` in `@code {}` |
| `<svg>` inline | Keep as-is or use `LucideXxxCn` if available |
| Google/GitHub brand SVGs | Keep as inline `<svg>` (brand logos not in Lucide) |

### Block File Template

```razor
@* ============================================================
   Block: {block-name}
   Title: {title}
   Description: {description}
   Long Description: {longDescription}
   Dependencies: {BlazorCN components used}
   Source: original/blocks/{category}/{block-name}.tsx
   ============================================================ *@

<div class="...">
    @* Block markup here *@
</div>

@code {
    // State, data models, helper methods
}
```

### Category Index Page Template

```razor
@page "/docs/blocks/{category-slug}"
@layout DocsLayout

<PageTitle>{Category} Blocks — BlazorCN</PageTitle>

<div class="space-y-6">
    <div>
        <h1 class="text-3xl font-bold tracking-tight">{Category}</h1>
        <p class="mt-2 text-lg text-muted-foreground">{category description}</p>
    </div>

    <SeparatorCn />

    <ComponentPreview Title="{Block Title}" Description="{block description}" Code="@Block1Code">
        <Block1Component />
    </ComponentPreview>

    @* ... one ComponentPreview per block ... *@
</div>

@code {
    private const string Block1Code = @"<Block1Component />";
    @* ... one code constant per block ... *@
}
```

### Handling framer-motion

Blocks using `framer-motion` for entrance animations: replace `<motion.div>` with plain `<div>`. The visual content is preserved; animations are cosmetic and non-essential. If a block has meaningful CSS transitions (hover, active states), keep those via Tailwind classes.

### Handling Complex Interactions

| Pattern | Approach |
|---|---|
| Form state (email, password, toggles) | C# fields + `@bind-Value` |
| Click handlers (theme toggle, tab select) | C# methods + `@onclick` |
| Sorting/filtering tables | C# LINQ in `@code` |
| Drag-drop (kanban) | JS interop via `blazorcn.js` |
| Timers (saved message, countdown) | `System.Timers.Timer` + `InvokeAsync(StateHasChanged)` |
| Charts/sparklines | Inline `<svg>` markup |
| Streaming/real-time | `Task.Delay` simulation |

---

## Task 1: Infrastructure Setup

**Files:**
- Create: `docs/BlazorCN.Demo/Pages/Docs/Blocks/` directory
- Modify: `docs/BlazorCN.Demo/Data/NavData.cs`
- Modify: `docs/BlazorCN.Demo/Layout/DocsSidebar.razor`
- Modify: `docs/BlazorCN.Demo/Components/CommandMenu.razor`

**Step 1: Create Blocks directory structure**

Create all 29 category subdirectories under `docs/BlazorCN.Demo/Pages/Docs/Blocks/`:
```
About/, Account/, Ai/, Awards/, Blog/, Calendar/, Carousel/, CommandMenu/,
Contact/, Crud/, Dashboard/, Dialog/, Features/, Footer/, Hero/, Login/,
Nft/, Onboarding/, Pricing/, ProductCard/, ProductCards/, Profile/,
Reviews/, Settings/, Stats/, Storefront/, Tables/, Team/, Testimonials/
```

**Step 2: Add Blocks nav data to NavData.cs**

Add a new `Blocks` array after `Components`:

```csharp
public static readonly NavSection[] Blocks =
[
    new("Blocks", [
        new("About", "/docs/blocks/about"),
        new("Account", "/docs/blocks/account"),
        new("AI", "/docs/blocks/ai"),
        new("Awards", "/docs/blocks/awards"),
        new("Blog", "/docs/blocks/blog"),
        new("Calendar", "/docs/blocks/calendar"),
        new("Carousel", "/docs/blocks/carousel"),
        new("Command Menu", "/docs/blocks/command-menu"),
        new("Contact", "/docs/blocks/contact"),
        new("CRUD", "/docs/blocks/crud"),
        new("Dashboard", "/docs/blocks/dashboard"),
        new("Dialog", "/docs/blocks/dialog"),
        new("Features", "/docs/blocks/features"),
        new("Footer", "/docs/blocks/footer"),
        new("Hero", "/docs/blocks/hero"),
        new("Login", "/docs/blocks/login"),
        new("NFT", "/docs/blocks/nft"),
        new("Onboarding", "/docs/blocks/onboarding"),
        new("Pricing", "/docs/blocks/pricing"),
        new("Product Card", "/docs/blocks/product-card"),
        new("Product Cards", "/docs/blocks/product-cards"),
        new("Profile", "/docs/blocks/profile"),
        new("Reviews", "/docs/blocks/reviews"),
        new("Settings", "/docs/blocks/settings"),
        new("Stats", "/docs/blocks/stats"),
        new("Storefront", "/docs/blocks/storefront"),
        new("Tables", "/docs/blocks/tables"),
        new("Team", "/docs/blocks/team"),
        new("Testimonials", "/docs/blocks/testimonials"),
    ]),
];
```

**Step 3: Update DocsSidebar.razor**

Add a third `@foreach` block for Blocks after the Components section:

```razor
@foreach (var navSection in Data.NavData.Blocks)
{
    <div class="mb-4">
        <h4 class="mb-1 rounded-md px-2 py-1 text-sm font-semibold">@navSection.Title</h4>
        @foreach (var item in navSection.Items)
        {
            <a href="@item.Href"
               class="@ItemClass(item.Href)">
                @item.Label
            </a>
        }
    </div>
}
```

**Step 4: Update CommandMenu.razor**

Update the search to include Blocks:

```csharp
var allSections = NavData.GettingStarted.Concat(NavData.Components).Concat(NavData.Blocks);
```

**Step 5: Build and verify**

Run: `dotnet build docs/BlazorCN.Demo/BlazorCN.Demo.csproj`
Expected: 0 errors, 0 warnings

**Step 6: Commit**

```bash
git add docs/BlazorCN.Demo/Data/NavData.cs docs/BlazorCN.Demo/Layout/DocsSidebar.razor docs/BlazorCN.Demo/Components/CommandMenu.razor
git commit -m "feat: add Blocks navigation infrastructure"
```

---

## Tasks 2-30: Block Categories

Each category task follows the same pattern. A subagent should:

1. Read ALL `.tsx` files in `original/blocks/{category}/`
2. Read the block metadata from `original/blocks/all-blocks-metadata.json` for each block
3. Convert each `.tsx` to a `.razor` file using the Conversion Reference above
4. Create the category index page
5. Build and verify

### Category Task Template

For category `{CATEGORY}` with `{N}` blocks at `original/blocks/{category}/`:

**Files:**
- Create: `docs/BlazorCN.Demo/Pages/Docs/Blocks/{Category}/{BlockName}.razor` × N
- Create: `docs/BlazorCN.Demo/Pages/Docs/Blocks/{Category}/{Category}BlocksPage.razor`

**Process per block:**
1. Read `original/blocks/{category}/{block-name}.tsx`
2. Read metadata from `all-blocks-metadata.json` for `{block-name}`
3. Convert JSX → Razor using the mapping table
4. Replace React components with BlazorCN Cn-suffix equivalents
5. Replace `lucide-react` icons with `LucideXxxCn` components
6. Convert `useState` → C# fields, event handlers → `@onclick`/`@bind`
7. Convert TypeScript interfaces → C# records
8. Convert mock data arrays → C# static readonly arrays
9. Omit `framer-motion` wrappers (keep content, drop `<motion.div>`)
10. Add inline documentation block comment from metadata
11. Write `.razor` file

**After all blocks converted:**
- Create `{Category}BlocksPage.razor` index page referencing all block components
- Build: `dotnet build docs/BlazorCN.Demo/BlazorCN.Demo.csproj`
- Fix any build errors
- Commit

### Category List (Tasks 2-30)

| Task | Category | Blocks | Directory |
|------|----------|--------|-----------|
| 2 | NFT | 3 | Nft/ |
| 3 | Product Cards | 7 | ProductCards/ |
| 4 | Testimonials | 8 | Testimonials/ |
| 5 | Profile | 9 | Profile/ |
| 6 | Awards | 12 | Awards/ |
| 7 | Pricing | 20 | Pricing/ |
| 8 | Footer | 25 | Footer/ |
| 9 | Onboarding | 30 | Onboarding/ |
| 10 | Reviews | 30 | Reviews/ |
| 11 | Calendar | 50 | Calendar/ |
| 12 | Command Menu | 50 | CommandMenu/ |
| 13 | CRUD | 50 | Crud/ |
| 14 | Features | 50 | Features/ |
| 15 | Login | 50 | Login/ |
| 16 | Product Card | 50 | ProductCard/ |
| 17 | Account | 54 | Account/ |
| 18 | Hero | 64 | Hero/ |
| 19 | Tables | 70 | Tables/ |
| 20 | Contact | 77 | Contact/ |
| 21 | Team | 90 | Team/ |
| 22 | AI | 100 | Ai/ |
| 23 | Blog | 100 | Blog/ |
| 24 | Carousel | 100 | Carousel/ |
| 25 | Settings | 100 | Settings/ |
| 26 | Stats | 100 | Stats/ |
| 27 | Storefront | 100 | Storefront/ |
| 28 | About | 127 | About/ |
| 29 | Dialog | 150 | Dialog/ |
| 30 | Dashboard | 200 | Dashboard/ |

**Order rationale:** Smallest categories first — proves the pattern, catches issues early. Large categories later when the conversion pattern is validated.

---

## Task 31: Final Verification

**Step 1:** Full build
```bash
dotnet build docs/BlazorCN.Demo/BlazorCN.Demo.csproj
```
Expected: 0 errors, 0 warnings

**Step 2:** Run all tests
```bash
dotnet test tests/BlazorCN.Tests/BlazorCN.Tests.csproj
```
Expected: All tests pass

**Step 3:** Verify all 29 category pages are reachable (check routes)

**Step 4:** Final commit
```bash
git add -A
git commit -m "feat: complete blocks showcase — 1,876 blocks across 29 categories"
```

---

## Execution Strategy

Given the massive scale (1,876 blocks), this should be executed using **subagent-driven development**:

- **Small categories (3-30 blocks):** 1 subagent per category
- **Medium categories (50-77 blocks):** 1-2 subagents per category
- **Large categories (90-200 blocks):** Split into batches of ~25-30 blocks per subagent

Each subagent receives:
1. The conversion reference table (from this plan)
2. The specific `.tsx` files to convert
3. The metadata for those blocks
4. The target `.razor` file paths
5. Instructions to build and verify after writing

**Parallelization:** Independent categories can run simultaneously. Multiple subagents within a large category can also run in parallel (each batch writes different files).
