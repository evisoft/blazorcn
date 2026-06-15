# shadcn (React) → BlazorCN translation

shadcn/ui has a `base` vs `radix` API split. BlazorCN has a **single** API, so the
relevant "differences" reference is how shadcn **React** patterns map to BlazorCN
**Razor**. Use this when porting shadcn snippets, docs, or AI output to BlazorCN —
a near-literal translation usually compiles but is often subtly wrong without these.

## Cheat sheet

| shadcn / React | BlazorCN / Razor |
| --- | --- |
| `<Button>` … `<Card>` | `<ButtonCn>` … `<CardCn>` (every component gets a `Cn` suffix) |
| `className="..."` | `Class="..."` |
| `variant="outline"` (string) | `Variant="ButtonVariant.Outline"` (enum) |
| `size="sm"` | `Size="ButtonSize.Sm"` |
| `onClick={fn}` | `OnClick="Fn"` (or `@onclick="Fn"`) |
| `useState` + `value`/`onValueChange` | `@bind-Value` (or `@bind-Checked`, `@bind-Open`) |
| `checked`/`onCheckedChange` | `@bind-Checked` |
| `open`/`onOpenChange` | `@bind-Open` |
| `cn(...)` | `Cn.Merge(...)` |
| `import { X } from "lucide-react"` | `<LucideXCn />` component (no import) |
| `toast()` from `sonner` | inject `ToastService`; `Toast.Success(...)` + one `<ToasterCn />` |
| `"use client"` directive | n/a — pick an interactive render mode (see [setup.md](../setup.md#render-modes)) |

## Behavioral differences (these bite)

### 1. No `asChild` — triggers ARE buttons

React merges behavior onto a child via `asChild`. BlazorCN has no slot-merging;
`*TriggerCn` renders its **own** `<button>`.

```tsx
// React
<DialogTrigger asChild><Button>Open</Button></DialogTrigger>
```
```razor
@* BlazorCN — the trigger is the button. Nesting a ButtonCn works but nests
   buttons; for valid markup style the trigger or use @bind-Open + a plain button. *@
<DialogTriggerCn Class="...">Open</DialogTriggerCn>
```

See [composition.md](./composition.md#why-triggers-render-their-own-button-no-aschild).

### 2. Items do NOT need a Group wrapper

React requires `SelectItem` inside `SelectGroup`. BlazorCN does **not** — items
cascade from the root and sit directly in the content. Groups are for labeled
sections only.

```razor
<SelectContentCn>
  <SelectItemCn Value="a">A</SelectItemCn>   @* no SelectGroupCn needed *@
</SelectContentCn>
```

### 3. No `data-icon`; icons auto-size

React uses `<SearchIcon data-icon="inline-start" />` inside buttons. BlazorCN has
no `data-icon` — just nest the icon; the component sizes and spaces it.

```razor
<ButtonCn><LucideSearchCn /> Search</ButtonCn>
```

See [icons.md](./icons.md).

### 4. `@bind-Checked` for Checkbox/Switch (not `@bind-Value`)

`CheckboxCn` and `SwitchCn` bind `Checked` (bool), not `Value`. Everything else
(`Input`, `Select`, `Slider`, `RadioGroup`, `ToggleGroup`, `Tabs`) binds `Value`.
See the [value-type table](./forms.md#binding-controls--bind-value-vs-bind-checked).

### 5. Forms: validation is manual

React's shadcn `Form` wires into `react-hook-form`. BlazorCN's `Form*Cn` are
**presentational only**. Use `EditForm` + `DataAnnotationsValidator` + `@bind-Value`,
and set `aria-invalid` / `data-invalid` yourself. See [forms.md](./forms.md).

### 6. `Select` is always inline JSX-style

There is no `base`-style `items` prop. Always compose `SelectItemCn` children
inside `SelectContentCn`. `ToggleGroup`/`Slider`/`Accordion` use plain scalar
binds (no React `type="single"`/array quirks).

### 7. Button link & loading

- `<Button asChild><a/></Button>` → `<ButtonCn Href="/x">` (renders an `<a>`;
  `OnClick` is ignored on the anchor branch).
- No `isLoading`/`isPending` prop → compose a `<SpinnerCn />` and set `Disabled`.

### 8. Overlays: explicit overlay for AlertDialog/Sheet

`DialogCn` renders its own backdrop; `AlertDialogCn`/`SheetCn` need an explicit
`<AlertDialogOverlayCn />` / `<SheetOverlayCn />`. Always include a `*TitleCn`.
