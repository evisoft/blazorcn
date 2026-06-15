---
name: blazorcn
description: Build, debug, style, and theme Blazor UI with the BlazorCN component library — the .NET/Blazor port of shadcn/ui whose components are suffixed Cn (ButtonCn, DialogCn, CardCn, SelectCn, SwitchCn, TableCn, SidebarCn, …). Use this skill whenever a Blazor/Razor project references the BlazorCN package, calls AddBlazorCN(), or uses any *Cn component — even if the user never says "BlazorCN" by name. Covers composing components and blocks (forms, dialogs, sheets, drawers, dashboards, sidebars, data tables, command palettes, dropdown and notification menus, pricing sections), data binding (@bind-Value / @bind-Open / @bind-Checked), variants and enums, Lucide icons, Tailwind setup, dark mode and CSS-variable theming, fixing BlazorCN issues (broken two-way binds, overlays/popovers opening off-screen at -9999px under trimmed or AOT WASM), and porting shadcn React snippets to Blazor. Not for React/Next.js shadcn/ui, MudBlazor or other Blazor UI kits, or plain Blazor with no component library.
user-invocable: false
allowed-tools: Bash(dotnet build*), Bash(dotnet test*)
---

# BlazorCN

A Blazor component library that replicates **shadcn/ui** one-to-one — ~200
components across ~55 groups, Tailwind CSS styling, CSS-variable theming, minimal
JS interop, across all render modes (Server, WebAssembly, Auto, Static SSR).

**Mental model — read this first.** BlazorCN is **one NuGet package**, not a
registry. There is no CLI, no `components.json`, no per-component install. "Adding
a component" means *just using it*: `<ButtonCn>`. Every component is suffixed
`Cn` and lives in the single flat namespace `@using BlazorCN`.

## Project Setup Context

There is no auto-injected project context (no CLI to query). Before building UI,
confirm the project is wired up — see [setup.md](./setup.md). Fast check:

1. `BlazorCN` package referenced (`dotnet list package | grep -i BlazorCN`).
2. `builder.Services.AddBlazorCN();` in `Program.cs`.
3. `_content/BlazorCN/blazorcn.css` linked in the host page.
4. Tailwind configured: a `@source` line covering the BlazorCN `.razor` you use,
   plus the `@theme inline` token map. **If a component renders unstyled, this is
   the cause** — see the [`@source` gotcha](./setup.md#4-configure-tailwind-v4-css-first).
5. `@using BlazorCN` in `_Imports.razor`.
6. One `<ToasterCn />` in the layout if you use toasts.

## Principles

1. **Use existing components first.** ~200 are in the package — check the
   [catalog](./components.md) before writing a styled `<div>`.
2. **Compose, don't reinvent.** Settings page = Tabs + Card + form controls.
   Dashboard = Sidebar + Card + Chart + Table.
3. **Use built-in variants before custom styles.** `Variant="ButtonVariant.Outline"`,
   `Size="ButtonSize.Sm"` — enum parameters, not strings.
4. **Use semantic colors.** `bg-primary`, `text-muted-foreground` — never raw
   values like `bg-blue-500`.

## Critical Rules

Always enforced. Each links to a file with Incorrect/Correct pairs.

### Styling & Tailwind → [styling.md](./rules/styling.md)

- **`Class` for layout, not restyling.** Don't override component colors/typography
  via `Class` — prefer variants, semantic tokens, or retheme the CSS variable.
  (A `Class` color override can even be *silently ignored* — see the cascade-layer
  caveat.)
- **No `space-x-*` / `space-y-*`.** Use `flex` + `gap-*`; vertical = `flex flex-col gap-*`.
- **`size-*` when width = height.** `size-10`, not `w-10 h-10`.
- **No manual `dark:` color overrides.** Semantic tokens flip with `.dark`.
- **Conditional classes via `Cn.Merge(...)`,** not string-concatenated ternaries.
- **No manual `z-index` on overlays.** Dialog/Sheet/Popover/etc. self-stack.

### Forms & Inputs → [forms.md](./rules/forms.md)

- **Form layout uses `FieldCn` / `FieldGroupCn`,** not a raw `div` with `space-y-*`.
- **Bind values with `@bind-Value`** (e.g. `<InputCn @bind-Value="_name" />`); use
  the correct value **type** per control (string, bool, double…).
- **Option sets (2–7 choices) use `ToggleGroupCn`,** not a loop of `ButtonCn` with
  manual active state.
- **Group related checkboxes/radios with `FieldSetCn` + `FieldLegendCn`,** not a
  `div` + heading.
- **For `EditForm`/DataAnnotations, use the `Form*Cn` family** (`FormFieldCn`,
  `FormLabelCn`, `FormControlCn`, `FormMessageCn`).

### Composition & Overlays → [composition.md](./rules/composition.md)

- **Bind overlay open state with `@bind-Open`,** and open via the `*TriggerCn`
  (which *is* the trigger button). Don't hand-roll show/hide.
- **Dialog/Sheet/Drawer include a `*TitleCn`** for accessibility (`Class="sr-only"`
  if visually hidden).
- **Group wrappers are OPTIONAL in BlazorCN** (unlike shadcn React). `SelectItemCn`
  sits directly in `SelectContentCn`; `DropdownMenuItemCn` directly in
  `DropdownMenuContentCn`. Use `SelectGroupCn` / `DropdownMenuGroupCn` only for
  *labeled sections*.
- **Use full Card composition** (`CardHeaderCn`/`CardTitleCn`/`CardContentCn`/
  `CardFooterCn`), don't dump everything into `CardContentCn`.
- **`TabsTriggerCn` goes inside `TabsListCn`.**
- **`AvatarCn` needs an `AvatarFallbackCn`** for when the image fails.

### Use Components, Not Custom Markup → [composition.md](./rules/composition.md)

- Callouts → `AlertCn`. Empty states → `EmptyCn`. Dividers → `SeparatorCn` (not
  `<hr>`). Loading → `SkeletonCn` / `SpinnerCn` (no `animate-pulse` divs). Status
  pills → `BadgeCn` (not styled spans). Toasts → inject `ToastService`.

### Icons → [icons.md](./rules/icons.md)

- **Nest icons directly** in components: `<ButtonCn><LucideSearchCn /> Search</ButtonCn>`.
- **No `data-icon` attribute** (that's shadcn/React; it does nothing here).
- **No sizing classes on nested icons** — components auto-size them via CSS.
- **Concrete `Lucide{Name}Cn` over the `LucideIconCn` dispatcher** for AOT/trim.
- **Don't guess icon names** — Lucide renamed many (`filter`→`funnel`, `home`→`house`,
  `trash`→`trash-2`); verify the name exists rather than assuming.

## Key Patterns

The patterns that most differentiate correct BlazorCN code:

```razor
@* Variants/sizes are ENUMS, not strings. *@
<ButtonCn Variant="ButtonVariant.Outline" Size="ButtonSize.Sm">Click</ButtonCn>

@* Bind form values — note the param name differs per control. *@
<InputCn @bind-Value="_email" />           @* Value:string?  *@
<SliderCn @bind-Value="_volume" />         @* Value:double   *@
<CheckboxCn @bind-Checked="_agree" />      @* Checked:bool — NOT @bind-Value *@
<SwitchCn @bind-Checked="_notify" />       @* Checked:bool *@

@* Form layout: FieldCn, not div + space-y. *@
<FieldGroupCn>
  <FieldCn>
    <FieldLabelCn For="email">Email</FieldLabelCn>
    <InputCn id="email" @bind-Value="_email" />
    <FieldDescriptionCn>We'll never share it.</FieldDescriptionCn>
  </FieldCn>
</FieldGroupCn>

@* Overlays: @bind-Open + the trigger IS the button. *@
<DialogCn @bind-Open="_open">
  <DialogTriggerCn>Open</DialogTriggerCn>
  <DialogContentCn>
    <DialogHeaderCn>
      <DialogTitleCn>Edit profile</DialogTitleCn>
    </DialogHeaderCn>
  </DialogContentCn>
</DialogCn>

@* Items sit directly in Content — no Group wrapper needed. *@
<SelectCn @bind-Value="_size">
  <SelectTriggerCn><SelectValueCn /></SelectTriggerCn>
  <SelectContentCn>
    <SelectItemCn Value="sm">Small</SelectItemCn>
    <SelectItemCn Value="lg">Large</SelectItemCn>
  </SelectContentCn>
</SelectCn>

@* Icons: nest directly, no data-icon, no size classes. *@
<ButtonCn><LucideSearchCn /> Search</ButtonCn>

@* Spacing: gap-*, not space-y-*. *@
<div class="flex flex-col gap-4">…</div>   @* correct *@

@* Status colors: Badge variants / tokens, not raw colors. *@
<BadgeCn Variant="BadgeVariant.Secondary">+20.1%</BadgeCn>
```

## Component Selection

| Need | Use |
| --- | --- |
| Button / action | `ButtonCn` (+ `Variant`/`Size`); `ButtonGroupCn` for connected rows |
| Form inputs | `InputCn`, `SelectCn`, `ComboboxCn`, `NativeSelectCn`, `SwitchCn`, `CheckboxCn`, `RadioGroupCn`, `TextareaCn`, `InputOtpCn`, `SliderCn` |
| Toggle 2–5 options | `ToggleGroupCn` + `ToggleGroupItemCn` |
| Form layout / validation | `FieldCn`/`FieldGroupCn`/`FieldSetCn`; `Form*Cn` for `EditForm` |
| Data display | `TableCn`, `CardCn`, `BadgeCn`, `AvatarCn` |
| Navigation | `SidebarCn`, `NavigationMenuCn`, `BreadcrumbCn`, `TabsCn`, `PaginationCn` |
| Overlays | `DialogCn` (modal), `SheetCn` (side), `DrawerCn` (bottom), `AlertDialogCn` (confirm) |
| Feedback | `ToastService`/`ToasterCn`, `AlertCn`, `ProgressCn`, `SkeletonCn`, `SpinnerCn` |
| Command palette | `CommandCn` (inside `DialogCn` for ⌘K) |
| Charts | `ChartCn` / `ChartContainerCn` |
| Layout | `CardCn`, `SeparatorCn`, `ResizableCn`, `ScrollAreaCn`, `AccordionCn`, `CollapsibleCn` |
| Empty states | `EmptyCn` |
| Menus | `DropdownMenuCn`, `ContextMenuCn`, `MenubarCn` |
| Tooltips / info | `TooltipCn`, `HoverCardCn`, `PopoverCn` |

Full names and sub-components: [components.md](./components.md).

## Key API Conventions

The Blazor analog of shadcn's "key fields":

- **`Cn` suffix + flat namespace.** Every component is `XxxCn` under `@using BlazorCN`.
- **`Class` / `Style` / arbitrary attributes** are accepted by every component
  (`ComponentBaseCn`). `Class` is merged via `Cn.Merge`; `id`/`data-*`/`aria-*`
  are forwarded to the root element. Use **`Class=` (capital C)** — lowercase
  `class=` lands in the attribute splat and can emit a duplicate `class` attribute.
- **`@bind-Value`** for form controls (control exposes `Value` + `ValueChanged`).
  **`@bind-Open`** for overlays (`Open` + `OpenChanged`).
- **Enum variants/sizes** — `ButtonVariant`, `ButtonSize`, `BadgeVariant`,
  `FieldOrientation`, `FloatingAlign`, etc. (not string literals).
- **`OnClick` is `EventCallback<MouseEventArgs>`** — both `OnClick="H"` and
  `@onclick="H"` work.
- **`ChildContent`** (`RenderFragment`) is the default slot; composed components
  use cascading values to connect parts (e.g. `SelectItemCn` ← `SelectCn`).
- **`Href` on `ButtonCn`** renders an `<a>` instead of a `<button>`.

## Finding a Component's Exact API

There is no `docs` command — read the source (it's thin and readable):

- **Source of truth:** `src/BlazorCN/Components/<Group>/*.razor` (+ `*.razor.cs`).
  The `[Parameter]` list and any `enum` `.cs` file are the full API.
- **Real usage:** search `docs/BlazorCN.Demo` (`Pages/**`, `Components/**`) for a
  component to see a working example, or browse the live demo at
  `/docs/components/{name}`.

**When creating, fixing, or debugging a component, read its source and a demo
usage first** — don't guess parameter names.

## Workflow

1. **Confirm setup** (see [Project Setup Context](#project-setup-context)).
2. **Check the catalog** before writing custom markup — [components.md](./components.md).
3. **Read the component source + a demo usage** to get the exact API.
4. **Build with variants and `@bind-`,** following the [Critical Rules](#critical-rules).
5. **Verify** — `dotnet build` (and `dotnet test` if you touched the library).
   For visual/interactive correctness, run the demo app.
6. **Theme via CSS variables,** not per-component color overrides —
   [customization.md](./customization.md).

## Detailed References

- [setup.md](./setup.md) — install, services, Tailwind config, render modes, AOT
- [components.md](./components.md) — full catalog: names, sub-components, key params/enums
- [customization.md](./customization.md) — theming, CSS variables, dark mode, custom colors, the cascade-layer caveat
- [rules/styling.md](./rules/styling.md) — semantic colors, variants, `Class`, spacing, `size`, `Cn.Merge`, z-index
- [rules/forms.md](./rules/forms.md) — Field/FieldGroup, `@bind-Value`, validation, `EditForm`, control selection
- [rules/composition.md](./rules/composition.md) — overlays (`@bind-Open`/triggers/titles), groups, Card, Tabs, Avatar, use-components-not-markup, toasts
- [rules/icons.md](./rules/icons.md) — Lucide components, sizing, no `data-icon`, AOT
- [rules/blazor-vs-react.md](./rules/blazor-vs-react.md) — porting shadcn React snippets to BlazorCN (`asChild`, `className`, `useState`, `cn()`, icons, groups)
