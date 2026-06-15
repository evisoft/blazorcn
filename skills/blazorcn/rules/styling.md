# Styling & Tailwind

See [customization.md](../customization.md) for theming, CSS variables, and adding
custom colors. This file is the Incorrect/Correct rule set.

## Contents

- Semantic colors (not raw Tailwind colors)
- No raw colors for status/state
- Built-in variants first
- `Class` for layout, not restyling
- No `space-x-*` / `space-y-*` — use `gap-*`
- `size-*` over `w-* h-*` when equal
- `truncate` shorthand
- No manual `dark:` color overrides
- Conditional classes via `Cn.Merge`
- No manual `z-index` on overlay components
- Cascade-layer caveat

---

## Semantic colors

Use semantic tokens that follow the theme, not raw palette values.

**Incorrect:**

```razor
<div class="bg-blue-500 text-white">
  <p class="text-gray-600">Secondary text</p>
</div>
```

**Correct:**

```razor
<div class="bg-primary text-primary-foreground">
  <p class="text-muted-foreground">Secondary text</p>
</div>
```

---

## No raw color values for status/state

For positive/negative/status indicators use a `BadgeCn` variant or a semantic
token like `text-destructive` — don't reach for raw Tailwind colors.

**Incorrect:**

```razor
<span class="text-emerald-600">+20.1%</span>
<span class="text-red-600">-3.2%</span>
```

**Correct:**

```razor
<BadgeCn Variant="BadgeVariant.Secondary">+20.1%</BadgeCn>
<span class="text-destructive">-3.2%</span>
```

If you need a success/positive color that isn't a token, add a CSS variable to the
theme (see [customization.md](../customization.md)) rather than hardcoding a hue.

---

## Built-in variants first

Use enum variant/size parameters before hand-rolling utilities.

**Incorrect:**

```razor
<ButtonCn Class="border border-input bg-transparent hover:bg-accent">Click</ButtonCn>
```

**Correct:**

```razor
<ButtonCn Variant="ButtonVariant.Outline">Click</ButtonCn>
```

---

## `Class` for layout, not restyling

`Class` is for **layout** (`max-w-md`, `mx-auto`, `mt-4`, `w-full`, grid/flex) —
not for overriding a component's colors or typography. To change appearance, use
variants, semantic tokens, or retheme the CSS variables.

**Incorrect:**

```razor
<CardCn Class="bg-blue-100 text-blue-900 font-bold">
  <CardContentCn>Dashboard</CardContentCn>
</CardCn>
```

**Correct:**

```razor
<CardCn Class="mx-auto max-w-md">
  <CardContentCn>Dashboard</CardContentCn>
</CardCn>
```

> See the [cascade-layer caveat](#cascade-layer-caveat) — a color override via
> `Class` may be silently ignored, which is another reason to prefer variants and
> tokens.

---

## No `space-x-*` / `space-y-*` — use `gap-*`

`space-y-4` → `flex flex-col gap-4`. `space-x-2` → `flex gap-2`.

```razor
<div class="flex flex-col gap-4">
  <InputCn @bind-Value="_a" />
  <InputCn @bind-Value="_b" />
  <ButtonCn>Submit</ButtonCn>
</div>
```

---

## `size-*` over `w-* h-*` when equal

`size-10` not `w-10 h-10`. Applies to avatars, skeletons, icon wrappers, etc.

```razor
<AvatarCn Class="size-10">...</AvatarCn>   @* correct *@
<AvatarCn Class="w-10 h-10">...</AvatarCn> @* wrong *@
```

---

## `truncate` shorthand

`truncate`, not `overflow-hidden text-ellipsis whitespace-nowrap`.

---

## No manual `dark:` color overrides

Semantic tokens already flip with the `.dark` class. Use `bg-background
text-foreground`, never `bg-white dark:bg-gray-950`.

---

## Conditional classes via `Cn.Merge`

There is no `className` ternary in Razor. Build the class string with
`Cn.Merge(...)` (last-wins per utility group) instead of string-concatenating
ternaries — it also de-duplicates conflicting utilities.

**Incorrect:**

```razor
<div class="flex items-center @(isActive ? "bg-primary text-primary-foreground" : "bg-muted")">
```

**Correct:**

```razor
<div class="@Cn.Merge("flex items-center", isActive ? "bg-primary text-primary-foreground" : "bg-muted")">
```

For components, pass extra classes through `Class` — they're already merged via
`Cn.Merge` onto the root element.

---

## Use `Class=`, not lowercase `class=`, on components

Pass extra classes to a BlazorCN component with the **`Class`** parameter (capital
C). Components render `class="@Class"` *and* splat `AdditionalAttributes`, so a
lowercase `class="..."` lands in the splat and can produce a **duplicate `class`
attribute** in the HTML (with unreliable last-wins behavior). `Class=` is merged
through `Cn.Merge` and is the only correct way.

**Incorrect:**

```razor
<CardHeaderCn class="flex items-center justify-between">…</CardHeaderCn>
<SkeletonCn class="h-4 w-24" />
```

**Correct:**

```razor
<CardHeaderCn Class="flex items-center justify-between">…</CardHeaderCn>
<SkeletonCn Class="h-4 w-24" />
```

> Lowercase `class` is fine on **plain HTML elements** (`<div class="...">`). The
> rule is only about BlazorCN `*Cn` components, where `Class` is a real parameter.

---

## No manual `z-index` on overlay components

`DialogCn`, `SheetCn`, `DrawerCn`, `AlertDialogCn`, `DropdownMenuCn`, `PopoverCn`,
`TooltipCn`, `HoverCardCn` manage their own stacking (overlay + JS positioning).
Never add `z-50` / `z-[999]`.

---

## Cascade-layer caveat

BlazorCN's `cn-*` component classes are mostly emitted **unlayered**, while
Tailwind utilities live in the `utilities` cascade layer — so at equal
specificity an unlayered `cn-*` color rule can beat a `Class` utility you pass in.
The library wraps variant **color** rules in `@layer components` for components
that have been fixed, but coverage is partial.

Consequence:

- **Layout** overrides via `Class` always work (they don't collide with `cn-*`).
- **Color/typography** overrides via `Class` may be ignored — prefer **variants**,
  **semantic tokens**, or **retheming the variable**. As a last resort an
  important utility (`Class="bg-blue-50!"`) will win, but treat it as a smell.

Full detail in [customization.md](../customization.md#the-cascade-layer-caveat).
