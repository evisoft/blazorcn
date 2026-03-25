# BlazorCN Claude Code Skill Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Create a local Claude Code plugin at `~/.claude/plugins/blazorcn/` that teaches AI to write correct BlazorCN code.

**Architecture:** A Claude Code plugin with a single auto-triggered skill containing a main SKILL.md, setup guide, 5 rule files with incorrect/correct Razor code pairs, and 3 evaluation prompts. All content is static documentation — no CLI tooling or MCP servers.

**Tech Stack:** Claude Code plugin format (plugin.json + SKILL.md frontmatter), Markdown, JSON (evals)

---

### Task 1: Create plugin scaffold

**Files:**
- Create: `~/.claude/plugins/blazorcn/plugin.json`

**Step 1: Create directory structure**

```bash
mkdir -p ~/.claude/plugins/blazorcn/skills/blazorcn/rules
mkdir -p ~/.claude/plugins/blazorcn/skills/blazorcn/evals
```

**Step 2: Write plugin.json**

Create `~/.claude/plugins/blazorcn/plugin.json`:

```json
{
  "name": "blazorcn",
  "description": "BlazorCN component library skill — teaches AI to write correct BlazorCN code with proper composition, styling, and patterns.",
  "version": "1.0.0"
}
```

**Step 3: Commit**

```bash
cd ~/.claude/plugins/blazorcn
git init
git add plugin.json
git commit -m "chore: scaffold blazorcn plugin"
```

---

### Task 2: Write SKILL.md — main skill entry

**Files:**
- Create: `~/.claude/plugins/blazorcn/skills/blazorcn/SKILL.md`

**Step 1: Write SKILL.md**

The file needs YAML frontmatter with:
- `name: blazorcn`
- `description:` covering all trigger conditions (`@using BlazorCN`, `*Cn` components, `Cn.Merge()`, `AddBlazorCN()`, `ComponentBaseCn`, BlazorCN NuGet reference)
- `user-invocable: false`

Body sections in order:
1. **BlazorCN** — one paragraph intro
2. **Principles** — 4 numbered rules (use existing components, compose don't reinvent, built-in variants first, semantic colors)
3. **Critical Rules** — one-liner per rule file linking to `./rules/styling.md`, `./rules/composition.md`, `./rules/forms.md`, `./rules/icons.md`, `./rules/js-interop.md`
4. **Key Patterns** — single Razor code block with 5 correct patterns:
   - `Cn.Merge()` conditional classes
   - `@bind-Value` on InputCn
   - Full CardCn composition
   - DialogTitleCn always present
   - `@inject ToastService` + `ToastService.Success()`
5. **Component Inventory** — organized by category (Layout, Forms, Data Display, Overlays, Menus, Navigation, Feedback, Actions, Content) with component names and sub-component counts
6. **Component Selection** — table with columns: Need | Use
   - Button/action → ButtonCn with variant
   - Form inputs → InputCn, SelectCn, CheckboxCn, SwitchCn, RadioGroupCn, TextareaCn, SliderCn, ComboboxCn, InputOtpCn
   - Toggle options → ToggleGroupCn + ToggleGroupItemCn
   - Data display → TableCn, CardCn, BadgeCn, AvatarCn, ChartCn
   - Navigation → TabsCn, BreadcrumbCn, PaginationCn, NavigationMenuCn, SidebarCn
   - Overlays → DialogCn (modal), SheetCn (side panel), DrawerCn (bottom), AlertDialogCn (confirmation)
   - Feedback → ToastService, AlertCn, ProgressCn, SkeletonCn, SpinnerCn
   - Command palette → CommandCn inside DialogCn
   - Layout → CardCn, SeparatorCn, ResizableCn, ScrollAreaCn, AccordionCn, CollapsibleCn
   - Empty states → EmptyCn
   - Menus → DropdownMenuCn, ContextMenuCn, MenubarCn
   - Tooltips/info → TooltipCn, HoverCardCn, PopoverCn
7. **Blocks — Complex UI/UX Design Patterns** — explanation paragraph, then table of all 30 block categories with counts and descriptions. Guidance to check blocks before building complex features from scratch.
8. **Detailed References** — links to `./setup.md` and each rule file

**Step 2: Commit**

```bash
git add skills/blazorcn/SKILL.md
git commit -m "feat: add main SKILL.md with principles, inventory, and blocks guide"
```

---

### Task 3: Write setup.md

**Files:**
- Create: `~/.claude/plugins/blazorcn/skills/blazorcn/setup.md`

**Step 1: Write setup.md**

Title: `# Setup & Getting Started`

Sections:
1. **Install** — `dotnet add package BlazorCN`
2. **Register Services** — `builder.Services.AddBlazorCN();` in Program.cs. Explain: registers `JsInteropCn` (scoped) and `ToastService` (scoped).
3. **Add CSS** — `<link href="_content/BlazorCN/blazorcn.css" rel="stylesheet" />` in `<head>` of App.razor or _Host.cshtml. Contains CSS variables (OKLCH format) for theming + component base styles.
4. **Add JS** — `<script src="_content/BlazorCN/blazorcn.js"></script>` before closing `</body>`. Required for interactive components (Dialog, Sheet, Select, Popover, etc.) — handles focus trapping, scroll lock, floating positioning, outside click detection.
5. **Configure Tailwind** — import preset in tailwind.config.js:
   ```js
   import blazorcnPreset from './_content/BlazorCN/tailwind-preset.js'
   export default {
     presets: [blazorcnPreset],
     content: [
       // your content paths
       "./**/*.razor",
     ],
   }
   ```
   The preset maps CSS variables to Tailwind utilities (bg-primary, text-muted-foreground, etc.) and configures border-radius tokens.
6. **Add Namespace** — `@using BlazorCN` in `_Imports.razor`. Single flat namespace for all ~200 components.
7. **Dark Mode** — toggle `.dark` class on `<html>` element. All semantic color tokens automatically switch. CSS variables defined in `:root` (light) and `.dark` (dark).
8. **Render Modes** — BlazorCN supports all Blazor render modes: Server, WebAssembly, Auto, Static SSR. Interactive components (Dialog, Select, etc.) require an interactive render mode.
9. **Minimal Example** — complete Razor snippet showing a page with CardCn, InputCn, ButtonCn to verify setup works.

**Step 2: Commit**

```bash
git add skills/blazorcn/setup.md
git commit -m "feat: add setup.md with installation and configuration guide"
```

---

### Task 4: Write rules/styling.md

**Files:**
- Create: `~/.claude/plugins/blazorcn/skills/blazorcn/rules/styling.md`

**Step 1: Write styling.md**

Title: `# Styling & Tailwind`

Each rule has a short heading, then **Incorrect** and **Correct** Razor code blocks.

Rules to cover (10 total):
1. **Semantic colors** — `class="bg-blue-500 text-white"` (wrong) vs `class="bg-primary text-primary-foreground"` (correct)
2. **No raw status colors** — `<span class="text-green-500">Active</span>` (wrong) vs `<BadgeCn Variant="BadgeVariant.Secondary">Active</BadgeCn>` (correct). Also: `<span class="text-red-600">-3.2%</span>` (wrong) vs `<span class="text-destructive">-3.2%</span>` (correct)
3. **Built-in variants first** — `<ButtonCn Class="border border-input bg-transparent">` (wrong) vs `<ButtonCn Variant="ButtonVariant.Outline">` (correct)
4. **Class for layout only** — `<CardCn Class="bg-blue-100 text-blue-900 font-bold">` (wrong) vs `<CardCn Class="max-w-md mx-auto">` (correct). Hierarchy: built-in variants → semantic tokens → CSS variables.
5. **No space-x/space-y** — `class="space-y-4"` (wrong) vs `class="flex flex-col gap-4"` (correct)
6. **size-* when equal** — `class="w-10 h-10"` (wrong) vs `class="size-10"` (correct)
7. **truncate shorthand** — `class="overflow-hidden text-ellipsis whitespace-nowrap"` (wrong) vs `class="truncate"` (correct)
8. **No manual dark: overrides** — `class="bg-white dark:bg-gray-950"` (wrong) vs `class="bg-background"` (correct)
9. **Use Cn.Merge()** — `class="@($"flex items-center {(isActive ? "bg-primary" : "bg-muted")}")"` (wrong) vs `class="@(Cn.Merge("flex items-center", isActive ? "bg-primary text-primary-foreground" : "bg-muted"))"` (correct)
10. **No manual z-index on overlays** — DialogCn, SheetCn, DrawerCn, PopoverCn, TooltipCn, DropdownMenuCn handle their own stacking. Never add `z-50` or `z-[999]`.

**Step 2: Commit**

```bash
git add skills/blazorcn/rules/styling.md
git commit -m "feat: add styling rules with incorrect/correct Razor examples"
```

---

### Task 5: Write rules/composition.md

**Files:**
- Create: `~/.claude/plugins/blazorcn/skills/blazorcn/rules/composition.md`

**Step 1: Write composition.md**

Title: `# Component Composition`

Rules with incorrect/correct pairs:

1. **Full Card composition** — dumping content in a single div (wrong) vs `CardCn > CardHeaderCn > CardTitleCn + CardDescriptionCn > CardContentCn > CardFooterCn` (correct)
2. **Dialog/Sheet/Drawer always need a Title** — missing DialogTitleCn (wrong) vs always including it (correct). Use `Class="sr-only"` if visually hidden. Applies to: `DialogTitleCn`, `SheetTitleCn`, `DrawerTitleCn`, `AlertDialogTitleCn`.
3. **TabsTriggerCn inside TabsListCn** — `TabsTriggerCn` directly in `TabsCn` (wrong) vs wrapped in `TabsListCn` (correct). Full correct example with `@bind-Value`.
4. **AvatarCn always needs AvatarFallbackCn** — `AvatarImageCn` alone (wrong) vs with `AvatarFallbackCn` showing initials (correct)
5. **Use existing components instead of custom markup** — table: `<hr>` → `SeparatorCn`, `<div class="animate-pulse">` → `SkeletonCn`, styled `<span>` → `BadgeCn`, custom callout div → `AlertCn`
6. **Callouts use AlertCn** — custom styled div (wrong) vs `AlertCn > AlertTitleCn + AlertDescriptionCn` (correct)
7. **Toast via ToastService** — custom notification markup (wrong) vs inject `ToastService`, call `ToastService.Success("Saved!")` (correct). Include `<ToasterCn />` in layout.
8. **Choosing overlay components** — table: Dialog (focused task), AlertDialog (destructive confirmation), Sheet (side panel), Drawer (mobile bottom panel), HoverCard (info on hover), Popover (contextual on click)
9. **Empty states use EmptyCn** — custom empty div (wrong) vs `EmptyCn > EmptyHeaderCn > EmptyMediaCn + EmptyTitleCn + EmptyDescriptionCn > EmptyContentCn` (correct)
10. **Button has no IsLoading prop** — imaginary `IsLoading="true"` (wrong) vs compose `SpinnerCn` inside disabled `ButtonCn` (correct)

**Step 2: Commit**

```bash
git add skills/blazorcn/rules/composition.md
git commit -m "feat: add composition rules with incorrect/correct Razor examples"
```

---

### Task 6: Write rules/forms.md

**Files:**
- Create: `~/.claude/plugins/blazorcn/skills/blazorcn/rules/forms.md`

**Step 1: Write forms.md**

Title: `# Forms & Inputs`

Rules with incorrect/correct pairs:

1. **@bind-Value for two-way binding** — manual `Value="@val"` + `@oninput` handler (wrong) vs `@bind-Value="val"` (correct). Applies to: InputCn, TextareaCn, SelectCn, RadioGroupCn, SliderCn, ComboboxCn.
2. **@bind-Checked for toggles** — manual `Checked="@isOn"` + `@onclick` handler (wrong) vs `@bind-Checked="isOn"` (correct). Applies to: CheckboxCn, SwitchCn.
3. **Full SelectCn composition** — raw `<select>` element (wrong) vs `SelectCn > SelectTriggerCn > SelectValueCn + SelectContentCn > SelectGroupCn > SelectItemCn` (correct). Include `@bind-Value` on `SelectCn`.
4. **RadioGroupCn with RadioGroupItemCn** — manual radio inputs (wrong) vs `RadioGroupCn @bind-Value + RadioGroupItemCn` (correct). Include label association.
5. **Form control selection guide** — table:
   - Simple text → InputCn
   - Dropdown with options → SelectCn
   - Searchable dropdown → ComboboxCn
   - Native HTML select → NativeSelectCn
   - Boolean toggle → SwitchCn (settings) or CheckboxCn (forms)
   - Single choice from few → RadioGroupCn
   - Toggle between 2-5 options → ToggleGroupCn
   - OTP/verification → InputOtpCn
   - Multi-line text → TextareaCn
   - Numeric range → SliderCn
6. **InputGroupCn for buttons inside inputs** — absolute-positioned button over input (wrong) vs `InputGroupCn > InputGroupInputCn + InputGroupButtonCn` (correct)
7. **SliderCn binding** — single value uses `@bind-Value="doubleVal"`, range uses `@bind-Values="doubleArray"`. Show both examples.

**Step 2: Commit**

```bash
git add skills/blazorcn/rules/forms.md
git commit -m "feat: add forms rules with incorrect/correct Razor examples"
```

---

### Task 7: Write rules/icons.md

**Files:**
- Create: `~/.claude/plugins/blazorcn/skills/blazorcn/rules/icons.md`

**Step 1: Write icons.md**

Title: `# Icons`

Rules:

1. **Components include their own icons** — BlazorCN components render built-in SVG icons for UI chrome: checkmarks (CheckboxCn, SelectItemCn, RadioGroupItemCn), chevrons (SelectTriggerCn, AccordionTriggerCn), close buttons (DialogContentCn, SheetContentCn, ToastCn). Consumers don't need to provide these.
2. **No sizing classes on icons inside components** — adding `class="w-4 h-4"` to an SVG inside ButtonCn (wrong) vs letting `[&_svg]:pointer-events-none [&_svg]:shrink-0` handle it (correct). Components style descendant SVGs automatically.
3. **Icons in ButtonCn** — just place your SVG or icon component inside `ChildContent`. The button's base styles handle sizing and pointer events. Show correct example with inline SVG.
4. **Icon-only buttons** — use `Size="ButtonSize.Icon"` (or `IconSm`, `IconXs`, `IconLg`). Show example.

**Step 2: Commit**

```bash
git add skills/blazorcn/rules/icons.md
git commit -m "feat: add icons rules"
```

---

### Task 8: Write rules/js-interop.md

**Files:**
- Create: `~/.claude/plugins/blazorcn/skills/blazorcn/rules/js-interop.md`

**Step 1: Write js-interop.md**

Title: `# JavaScript Interop`

Sections:

1. **AddBlazorCN() is required** — forgetting service registration (wrong, runtime error) vs `builder.Services.AddBlazorCN()` (correct). Without it, interactive components (Dialog, Select, Sheet, Popover, Combobox, etc.) will fail at runtime.
2. **Consumers don't call JsInteropCn directly** — it's an internal service. Components handle all JS interop automatically:
   - **Focus trapping** — DialogContentCn, SheetContentCn, AlertDialogContentCn
   - **Scroll locking** — DialogContentCn, SheetContentCn, DrawerContentCn
   - **Floating positioning** — SelectContentCn, PopoverContentCn, ComboboxContentCn, TooltipContentCn, DropdownMenuContentCn (uses Floating UI)
   - **Outside click** — SelectCn, PopoverCn, ComboboxCn, DropdownMenuCn
   - **Keyboard navigation** — SelectCn, ComboboxCn, CommandCn, MenubarCn
3. **Disposal chain** — interactive components implement `IAsyncDisposable`. If wrapping BlazorCN components in your own components, ensure you don't break the disposal chain. Don't manually dispose BlazorCN component references.
4. **Static SSR limitation** — interactive components require an interactive render mode (`@rendermode InteractiveServer` or `InteractiveWebAssembly` or `InteractiveAuto`). Static SSR only works with non-interactive components (Card, Badge, Alert, Table, etc.).

**Step 2: Commit**

```bash
git add skills/blazorcn/rules/js-interop.md
git commit -m "feat: add js-interop rules"
```

---

### Task 9: Write evals/evals.json

**Files:**
- Create: `~/.claude/plugins/blazorcn/skills/blazorcn/evals/evals.json`

**Step 1: Write evals.json**

JSON structure matching shadcn's format — array of 3 eval objects, each with `id`, `prompt`, `expected_output`, `files` (empty array), and `expectations` (string array).

Content from the design doc:

- **Eval 1** (settings form): expects CardCn composition, InputCn @bind-Value, TextareaCn @bind-Value, SwitchCn @bind-Checked, gap-*, semantic colors, no dark: overrides
- **Eval 2** (delete dialog): expects AlertDialogCn, AlertDialogTitleCn, ButtonVariant.Destructive, SpinnerCn in ButtonCn, @bind-Open, no manual z-index
- **Eval 3** (dashboard stats): expects full CardCn, BadgeCn, SkeletonCn, semantic colors, gap-*, size-*, references blocks

**Step 2: Commit**

```bash
git add skills/blazorcn/evals/evals.json
git commit -m "feat: add eval prompts for skill validation"
```

---

### Task 10: Final verification

**Step 1: Verify file structure**

```bash
find ~/.claude/plugins/blazorcn -type f | sort
```

Expected output:
```
~/.claude/plugins/blazorcn/plugin.json
~/.claude/plugins/blazorcn/skills/blazorcn/SKILL.md
~/.claude/plugins/blazorcn/skills/blazorcn/evals/evals.json
~/.claude/plugins/blazorcn/skills/blazorcn/rules/composition.md
~/.claude/plugins/blazorcn/skills/blazorcn/rules/forms.md
~/.claude/plugins/blazorcn/skills/blazorcn/rules/icons.md
~/.claude/plugins/blazorcn/skills/blazorcn/rules/js-interop.md
~/.claude/plugins/blazorcn/skills/blazorcn/rules/styling.md
~/.claude/plugins/blazorcn/skills/blazorcn/setup.md
```

**Step 2: Verify SKILL.md frontmatter parses correctly**

Read the first 10 lines of SKILL.md and confirm the YAML frontmatter has `---` delimiters, `name`, `description`, and `user-invocable` fields.

**Step 3: Verify all rule file links in SKILL.md resolve**

Check that every `./rules/*.md` and `./setup.md` reference in SKILL.md corresponds to an actual file.

**Step 4: Verify evals.json is valid JSON**

```bash
cat ~/.claude/plugins/blazorcn/skills/blazorcn/evals/evals.json | python3 -m json.tool > /dev/null && echo "Valid JSON"
```

**Step 5: Final commit if any fixes needed**

```bash
git add -A
git status
# Only commit if there are changes
```
