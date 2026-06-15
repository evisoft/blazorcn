# Icons

BlazorCN bundles all **1,702 Lucide icons** as Blazor components. There is **no
separate icon package to install** and — importantly — **no `data-icon`
attribute convention** (that is a shadcn/React thing; it does nothing here).

## Two ways to use an icon

```razor
@* 1. Concrete component (preferred). Name = Lucide + PascalCase of the icon. *@
<LucideCheckCn Size="16" Class="text-green-600" />
<LucideTriangleAlertCn />

@* 2. By-name dispatcher (runtime string; handles lucide v1→v2 renames). *@
<LucideIconCn Name="circle-check-big" Size="20" Class="text-primary" />
```

Both expose the same parameters (from `LucideIconBaseCn`):

| Parameter | Type | Default | Notes |
| --- | --- | --- | --- |
| `Size` | `int` | `24` | Sets the SVG `width`/`height` **attributes** (px) |
| `Fill` | `string` | `"none"` | SVG `fill` |
| `Stroke` | `string` | `"currentColor"` | Inherits parent text color by default |
| `StrokeWidth` | `int` | `2` | SVG `stroke-width` |
| `Class`, `Style` | — | — | Standard; `Class` merges onto the `<svg>` |

`LucideIconCn` additionally takes `Name` (`string`, `EditorRequired`) — kebab-case,
e.g. `"trash-2"`, `"circle-alert"`.

---

## No sizing classes (or `Size`) on icons inside components

Components auto-size nested icons via CSS — `[&_svg:not([class*='size-'])]:size-4`
on the component's base class. So an icon inside `ButtonCn`, `DropdownMenuItemCn`,
`AlertCn`, `Sidebar*`, etc. renders at the component's intended size **regardless
of the icon's `Size` parameter** — CSS `size-*` overrides the SVG width/height
attributes.

**Incorrect** (redundant/ignored sizing, and the `data-icon` does nothing):

```razor
<ButtonCn>
  <LucideSearchCn Size="16" Class="mr-2 size-4" data-icon="inline-start" />
  Search
</ButtonCn>
```

**Correct** — just nest the icon; the button handles size *and* spacing (its base
class includes `gap-2`):

```razor
<ButtonCn>
  <LucideSearchCn />
  Search
</ButtonCn>

<ButtonCn>
  Next
  <LucideArrowRightCn />
</ButtonCn>
```

To **deliberately** change an icon's size inside a component, add a `size-*`
`Class` — the `:not([class*='size-'])` guard means your class opts out of the
auto-size and wins:

```razor
<ButtonCn Size="ButtonSize.Lg">
  <LucideSparklesCn Class="size-5" />
  Upgrade
</ButtonCn>
```

For a **standalone** icon (not inside a sizing component), use `Size`:

```razor
<LucideLoaderCn Size="32" Class="animate-spin text-muted-foreground" />
```

---

## Icon-only buttons

Use `ButtonSize.Icon` (or `IconSm`/`IconLg`/`IconXs`) and add an accessible label:

```razor
<ButtonCn Variant="ButtonVariant.Ghost" Size="ButtonSize.Icon" aria-label="Settings">
  <LucideSettingsCn />
</ButtonCn>
```

---

## Don't guess icon names — Lucide renamed many

There are ~1,700 icons and Lucide renamed a lot of classic ones, so a plausible
guess often doesn't exist. A wrong **concrete** component (`LucideFilterCn`) won't
compile; a wrong **dispatcher** name (`<LucideIconCn Name="filter" />`) silently
renders nothing. Common renames that bite:

| You might reach for | The component that exists |
| --- | --- |
| `LucideFilterCn` | `LucideFunnelCn` (or `LucideListFilterCn`) |
| `LucideHomeCn` | `LucideHouseCn` |
| `LucideTrashCn` | `LucideTrash2Cn` |
| `LucideSettingsCn` | `LucideSettingsCn` ✓ (this one's fine) |
| `LucideMoreHorizontalCn` | `LucideEllipsisCn` |
| `LucideEditCn` | `LucidePenCn` / `LucidePenLineCn` |

When you're not certain a name exists, **verify it** rather than guessing — check
[lucide.dev/icons](https://lucide.dev/icons) for the current name, or list the
shipped set (the `Lucide*Cn` components under the package's icon assets). The
`LucideIconCn` dispatcher carries an alias map for *some* v1→v2 renames, but it's
not exhaustive (e.g. `filter` is not aliased), so don't lean on it to rescue a
wrong name.

---

## AOT / trimming

`LucideIconCn` resolves the concrete type by **reflection** (`Type.GetType`). Under
Native-AOT or aggressive trimming, a trimmer rooting only from
`<LucideIconCn Name="..."/>` usages may strip the concrete icon types. For
trim-safe apps, reference the concrete `Lucide{Name}Cn` component directly so the
type is statically rooted. Use the dispatcher only when the icon is genuinely
chosen at runtime.
