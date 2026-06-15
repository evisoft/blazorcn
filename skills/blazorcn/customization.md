# Customization & Theming

BlazorCN components reference semantic CSS-variable tokens (the exact shadcn/ui
system). Change the variables once and every component updates. This is the
**first** way to customize — reach for raw Tailwind colors only as a last resort.

## Contents

- How it works (CSS variables → Tailwind tokens → components)
- Color variables (full token table) and OKLCH
- Dark mode
- Retheming (override variables)
- Adding custom colors (Tailwind v4)
- Border radius (`--radius`)
- Customizing a component (variants → `Class` → new `Cva` variant → wrapper)
- The cascade-layer caveat (why a `Class` color override can be ignored)

---

## How It Works

1. CSS variables are defined on `:root` (light) and `.dark` (dark) in
   `_content/BlazorCN/blazorcn.css`.
2. Your app's Tailwind `@theme inline` block maps them to utility tokens
   (`--color-primary: var(--primary)` → `bg-primary`, `text-primary-foreground`, …).
3. Components emit those semantic utilities, so changing a variable re-themes
   every component that references it.

---

## Color Variables

Every color follows the `name` / `name-foreground` convention: the base variable
is the surface/background, `-foreground` is text/icons on that surface. These are
the variables BlazorCN ships in `blazorcn.css` (light `:root` values shown):

| Variable | Light value | Purpose |
| --- | --- | --- |
| `--background` / `--foreground` | `oklch(1 0 0)` / `oklch(0.145 0 0)` | Page background and default text |
| `--card` / `--card-foreground` | `oklch(1 0 0)` / `oklch(0.145 0 0)` | Card surfaces |
| `--popover` / `--popover-foreground` | `oklch(1 0 0)` / `oklch(0.145 0 0)` | Popover/dropdown surfaces |
| `--primary` / `--primary-foreground` | `oklch(0.205 0 0)` / `oklch(0.985 0 0)` | Primary buttons and actions |
| `--secondary` / `--secondary-foreground` | `oklch(0.97 0 0)` / `oklch(0.205 0 0)` | Secondary actions |
| `--muted` / `--muted-foreground` | `oklch(0.97 0 0)` / `oklch(0.556 0 0)` | Muted/disabled states, secondary text |
| `--accent` / `--accent-foreground` | `oklch(0.97 0 0)` / `oklch(0.205 0 0)` | Hover/accent states |
| `--destructive` / `--destructive-foreground` | `oklch(0.577 0.245 27.325)` / `oklch(0.97 0.01 17)` | Errors and destructive actions |
| `--border` | `oklch(0.922 0 0)` | Default border color |
| `--input` | `oklch(0.922 0 0)` | Form input borders |
| `--ring` | `oklch(0.708 0 0)` | Focus ring color |
| `--chart-1` … `--chart-5` | (5 hues) | Chart/data-visualization series |
| `--sidebar`, `--sidebar-foreground`, `--sidebar-primary*`, `--sidebar-accent*`, `--sidebar-border`, `--sidebar-ring` | — | Sidebar-specific colors |
| `--radius` | `0.625rem` | Global border radius |

Colors use **OKLCH**: `oklch(L C H)` — lightness `0–1`, chroma (`0` = gray),
hue `0–360`. e.g. `--primary: oklch(0.205 0 0)` is a near-black neutral.

---

## Dark Mode

Dark mode is **class-based**: the `.dark` class on the `<html>` element swaps the
variable block. `blazorcn.css` also flips `color-scheme` (`light`/`dark`) so
native UI (the `<select>` popup, scrollbars, date pickers) follows the theme.

Toggle it from C# via JS interop:

```csharp
@inject IJSRuntime JS

private async Task ToggleTheme()
    => await JS.InvokeVoidAsync("document.documentElement.classList.toggle", "dark");
```

To set it explicitly (e.g. from a stored preference), add/remove the class:

```csharp
await JS.InvokeVoidAsync("document.documentElement.classList.toggle", "dark", isDark);
```

There is no `next-themes` equivalent — manage the class yourself (typically in a
layout/`OnAfterRenderAsync`, persisting the choice to `localStorage`).

---

## Retheming (override variables)

Override the variables anywhere your CSS loads after `blazorcn.css` (e.g. your
`app.css`). You only need to set what you want to change:

```css
:root {
  --primary: oklch(0.55 0.22 264);   /* indigo brand */
  --radius: 0.5rem;
}
.dark {
  --primary: oklch(0.65 0.19 264);
}
```

Or apply a full shadcn theme by copying its `:root` / `.dark` blocks (any
`ui.shadcn.com` theme works — the variable names are identical).

---

## Adding Custom Colors

If you need a token that doesn't exist (e.g. `warning`), add it in **two** places:
the variable definition and the Tailwind `@theme inline` map. Edit your app's
global CSS (the file you feed to the Tailwind CLI) — never create a separate file.

```css
/* 1. Define the variable (light + dark). */
:root {
  --warning: oklch(0.84 0.16 84);
  --warning-foreground: oklch(0.28 0.07 46);
}
.dark {
  --warning: oklch(0.41 0.11 46);
  --warning-foreground: oklch(0.99 0.02 95);
}

/* 2. Register it as a Tailwind token (v4, CSS-first). */
@theme inline {
  --color-warning: var(--warning);
  --color-warning-foreground: var(--warning-foreground);
}
```

```razor
@* 3. Use it. *@
<div class="bg-warning text-warning-foreground rounded-md p-3">Heads up</div>
```

> BlazorCN targets **Tailwind v4** (CSS-first `@theme inline`). If you wired up a
> v3 `tailwind.config.js` instead, register the color under `theme.extend.colors`
> with `oklch(var(--warning) / <alpha-value>)`.

---

## Border Radius

`--radius` controls rounding globally. The demo's `@theme inline` derives a scale
from it, so changing `--radius` rescales every component:

```css
--radius-sm: calc(var(--radius) * 0.6);
--radius-md: calc(var(--radius) * 0.8);
--radius-lg: var(--radius);
--radius-xl: calc(var(--radius) * 1.4);
```

---

## Customizing a Component

Prefer these approaches, in order:

### 1. Built-in variants (best)

```razor
<ButtonCn Variant="ButtonVariant.Outline" Size="ButtonSize.Sm">Click</ButtonCn>
<BadgeCn Variant="BadgeVariant.Secondary">Beta</BadgeCn>
```

### 2. `Class` for layout

`Class` is merged into the root element via `Cn.Merge` (last-wins per utility
group). Use it for **layout** (`max-w-md`, `mx-auto`, `mt-4`), not for restyling
colors/typography.

```razor
<CardCn Class="mx-auto max-w-md">...</CardCn>
```

### 3. A new variant via `Cva`

The components are thin. Components build their class string with the `Cva<…>`
helper (a C# port of class-variance-authority). To add a variant you'd add an
enum value + a class entry. In your own components, reuse the same helper:

```csharp
private static readonly Cva<MyVariant> Variants = new(
    baseClasses: "inline-flex items-center rounded-md px-3 py-1.5 text-sm",
    variants: new() {
        [MyVariant.Default] = "bg-primary text-primary-foreground",
        [MyVariant.Warning] = "bg-warning text-warning-foreground",
    });

// In markup: class="@Variants.Apply(Variant, Class)"
```

`Cva<TVariant>.Apply(variant, additionalClasses)` and
`Cva<TVariant, TSize>.Apply(variant, size, additionalClasses)` both run the
result through `Cn.Merge`, so a consumer's `Class` overrides conflicting base
utilities.

### 4. Wrapper components (compose, don't fork)

Build higher-level components from the primitives instead of copying source:

```razor
@* ConfirmDialog.razor *@
<AlertDialogCn @bind-Open="_open">
    <AlertDialogContentCn>
        <AlertDialogHeaderCn>
            <AlertDialogTitleCn>@Title</AlertDialogTitleCn>
            <AlertDialogDescriptionCn>@Description</AlertDialogDescriptionCn>
        </AlertDialogHeaderCn>
        <AlertDialogFooterCn>
            <AlertDialogCancelCn>Cancel</AlertDialogCancelCn>
            <AlertDialogActionCn OnClick="OnConfirm">Confirm</AlertDialogActionCn>
        </AlertDialogFooterCn>
    </AlertDialogContentCn>
</AlertDialogCn>
```

---

## The Cascade-Layer Caveat

BlazorCN's component classes (`cn-button`, `cn-card`, …) are defined in
`blazorcn-components.css`. **Most** of these rules are emitted *unlayered*, while
Tailwind utilities live in the `utilities` cascade layer. Because an unlayered
rule beats a layered one at equal specificity, a `Class` you pass to **override a
color** can be silently ignored — the component's own `cn-*` color wins.

The library wraps **variant color rules in `@layer components`** for the
components that have been fixed (so consumer `Class` colors win there), but not
all components are covered yet. Practical guidance:

- For **layout** overrides (`max-w-*`, `m*`, `flex`, `grid`, …) `Class` works
  everywhere — those utilities don't collide with `cn-*` rules.
- For **color/typography** overrides, prefer **variants** and **semantic
  tokens** / **CSS-variable retheming** over a `Class` like `bg-blue-50`. If you
  must force it, an arbitrary `!`-important utility (`Class="bg-blue-50!"`) will
  win, but treat that as a smell — retheme the variable instead.

See [rules/styling.md](./rules/styling.md) for Incorrect/Correct pairs.
