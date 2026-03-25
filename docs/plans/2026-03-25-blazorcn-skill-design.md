# BlazorCN Claude Code Skill — Design

**Date:** 2026-03-25
**Audience:** Consumers of the BlazorCN NuGet package
**Location:** `~/.claude/plugins/blazorcn/` (local plugin)
**Integration:** Pure documentation/rules, no CLI/MCP

---

## Goal

Build a Claude Code skill that teaches AI how to write correct BlazorCN code — the same way shadcn's skill teaches AI how to use shadcn/ui. Covers component selection, composition patterns, styling rules, form integration, and points to blocks for complex UI/UX patterns.

---

## Structure

```
~/.claude/plugins/blazorcn/
├── plugin.json
├── skills/
│   └── blazorcn/
│       ├── SKILL.md                 # Main entry: principles, component inventory, blocks, selection guide
│       ├── setup.md                 # NuGet install, services, Tailwind, CSS, dark mode
│       ├── rules/
│       │   ├── styling.md           # Cn.Merge(), semantic colors, gap vs space, dark mode
│       │   ├── composition.md       # Parent/child patterns, overlays, Card, Tabs, Avatar, Toast
│       │   ├── forms.md             # @bind-Value, form controls, validation, SelectCn composition
│       │   ├── icons.md             # Inline SVG, icon sizing, [&_svg] styles
│       │   └── js-interop.md        # AddBlazorCN(), internal JsInteropCn, disposal
│       └── evals/
│           └── evals.json           # 3 test prompts with expectations
```

---

## SKILL.md Content

### Frontmatter
- `name: blazorcn`
- `description:` Triggers on BlazorCN component usage — `@using BlazorCN`, `*Cn` components, `Cn.Merge()`, `AddBlazorCN()`, `ComponentBaseCn`, or any project referencing the BlazorCN NuGet package.
- `user-invocable: false`

### Body Sections

1. **What is BlazorCN** — Blazor component library replicating shadcn-ui. Ships as NuGet package. ~200 components across 55 groups. Tailwind CSS + CSS variables for theming. All render modes (Server, WASM, Auto, Static SSR).

2. **Principles:**
   - Use existing components first — check if a `*Cn` component exists before custom markup
   - Compose, don't reinvent — settings page = `TabsCn` + `CardCn` + form controls
   - Use built-in variants before custom styles — `Variant="ButtonVariant.Outline"`
   - Use semantic colors — `bg-primary`, `text-muted-foreground`, never raw `bg-blue-500`

3. **Critical Rules** — one-liner summaries linking to each rule file

4. **Key Patterns** — 5 most common correct Razor patterns:
   - `Cn.Merge()` for conditional classes
   - `@bind-Value` on form controls
   - Full `CardCn` composition
   - `DialogTitleCn` always present
   - `ToastService` injection and usage

5. **Component Inventory** — all 55 groups organized by category:
   - Layout: CardCn (7), SeparatorCn, AspectRatioCn, ResizableCn (3), ScrollAreaCn (2), SidebarCn (23)
   - Forms: InputCn, TextareaCn, CheckboxCn, SwitchCn, RadioGroupCn (2), SelectCn (8), SliderCn, ComboboxCn (8), InputGroupCn (6), InputOtpCn (4), NativeSelectCn (3), FieldCn (10), FormCn (5), ToggleGroupCn (2)
   - Data Display: TableCn (8), BadgeCn, AvatarCn (6), SkeletonCn, ProgressCn, ChartCn (2), CarouselCn (5), KbdCn, SpinnerCn
   - Overlays: DialogCn (9), SheetCn (9), DrawerCn (9), AlertDialogCn (10), PopoverCn (7), HoverCardCn (3), TooltipCn (3)
   - Menus: DropdownMenuCn (14), ContextMenuCn (14), MenubarCn (15), CommandCn (7)
   - Navigation: TabsCn (4), BreadcrumbCn (7), PaginationCn (7), NavigationMenuCn (8)
   - Feedback: AlertCn (3), ToastCn/ToasterCn (2) + ToastService, EmptyCn (6)
   - Actions: ButtonCn, ButtonGroupCn (3), ToggleCn
   - Content: AccordionCn (4), CollapsibleCn (3), ItemCn (10), LabelCn

6. **Component Selection Guide** — table mapping needs to components

7. **Blocks — Complex UI/UX Design Patterns** — ~1,680+ pre-built blocks across 30 categories. Full design paradigms, not just demos. Categories include Dashboard (201), Dialog (151), About (128), Settings (101), Stats (101), Storefront (101), Blog (101), AI (101), Carousel (100), Team (91), Contact (78), Tables (71), Hero (65), Account (55), Calendar (51), CommandMenu (51), CRUD (51), Features (51), Login (51), ProductCard (50), Onboarding (31), Reviews (31), Footer (26), Pricing (21), Awards (13), Profile (10), Testimonials (9), ProductCards (8), NFT (4). Guidance: check blocks before building complex features from scratch.

8. **Detailed References** — links to setup.md and each rule file

---

## Rule Files

### rules/styling.md
Incorrect/correct pairs:
- Semantic colors vs raw Tailwind colors
- No raw status colors — use BadgeCn or text-destructive
- Built-in variants first, not manual class overrides
- Class for layout only (max-w-md, mx-auto), not color/typography overrides
- gap-* instead of space-x/space-y
- size-* when width equals height
- truncate shorthand
- No manual dark: overrides — use semantic tokens
- Cn.Merge() for conditional classes, not string interpolation ternaries
- No manual z-index on overlay components

### rules/composition.md
Incorrect/correct pairs:
- Full Card composition (CardHeaderCn/CardTitleCn/etc.)
- Dialog/Sheet/Drawer always need a TitleCn (sr-only if hidden)
- TabsTriggerCn must be inside TabsListCn
- AvatarCn always needs AvatarFallbackCn
- Use existing components (SeparatorCn not hr, SkeletonCn not animate-pulse, BadgeCn not styled span)
- Overlay selection guide (Dialog vs Sheet vs Drawer vs AlertDialog)
- Callouts use AlertCn
- Toast via ToastService

### rules/forms.md
Incorrect/correct pairs:
- @bind-Value for two-way binding on InputCn, TextareaCn, SelectCn, SliderCn, RadioGroupCn
- @bind-Checked for CheckboxCn, SwitchCn
- Form control selection guide
- Full SelectCn composition (SelectCn/SelectTriggerCn/SelectContentCn/SelectItemCn)
- RadioGroupCn with RadioGroupItemCn

### rules/icons.md
- Components use inline SVG for built-in chrome (checkmarks, chevrons, close buttons)
- No sizing classes on icons inside components — [&_svg] styles handle it
- Icons in ButtonCn — place SVG in ChildContent, component handles the rest

### rules/js-interop.md
- AddBlazorCN() required in Program.cs
- Consumers don't call JsInteropCn directly — internal to components
- Overlays handle focus trap, scroll lock, positioning automatically
- Interactive components implement IAsyncDisposable — respect disposal chain

---

## setup.md

Step-by-step:
1. `dotnet add package BlazorCN`
2. `builder.Services.AddBlazorCN()` in Program.cs
3. `<link href="_content/BlazorCN/blazorcn.css" rel="stylesheet" />`
4. `<script src="_content/BlazorCN/blazorcn.js"></script>`
5. Import `tailwind-preset.js` as Tailwind preset
6. `@using BlazorCN` in `_Imports.razor`
7. Dark mode: toggle `.dark` class on `<html>`

---

## Evals

### Eval 1: Settings Form
**Prompt:** "I'm building a Blazor app with BlazorCN. Create a settings page with fields for: display name, email, bio (multiline), and a dark mode toggle switch. Show validation errors for required fields."

**Expectations:**
- CardCn with full composition
- InputCn with @bind-Value
- TextareaCn with @bind-Value
- SwitchCn with @bind-Checked
- gap-* not space-y-*
- Semantic colors (text-destructive for errors)
- No manual dark: overrides

### Eval 2: Confirmation Dialog
**Prompt:** "Create a delete confirmation dialog using BlazorCN. It should have a warning icon, title, description explaining what will be deleted, and Cancel/Delete buttons. The delete button should show a spinner while processing."

**Expectations:**
- AlertDialogCn (not DialogCn) for destructive confirmation
- AlertDialogTitleCn for accessibility
- ButtonCn Variant="ButtonVariant.Destructive"
- SpinnerCn composed inside ButtonCn (no IsLoading prop)
- @bind-Open for state
- No manual z-index

### Eval 3: Dashboard Stats
**Prompt:** "Build a dashboard section with 4 stat cards in a responsive grid. Each card shows a metric title, large value, and a percentage change badge. Include a loading skeleton state. Using BlazorCN."

**Expectations:**
- Full CardCn composition
- BadgeCn for percentage change
- SkeletonCn for loading
- Semantic colors, not raw Tailwind
- gap-* for grid spacing
- size-* when width equals height
- References dashboard blocks for complex layouts
