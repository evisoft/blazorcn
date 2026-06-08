# BlazorCN

A production-ready **Blazor component library that replicates [shadcn/ui](https://ui.shadcn.com) one‑to‑one.** Thin component wrappers, Tailwind CSS styling, CSS‑variable theming, and minimal JavaScript interop — across **all Blazor render modes** (Server, WebAssembly, Auto, Static SSR).

[![NuGet](https://img.shields.io/badge/NuGet-BlazorCN-blue)](https://www.nuget.org/packages/BlazorCN) ![.NET](https://img.shields.io/badge/.NET-10.0-512BD4) ![License](https://img.shields.io/badge/license-MIT-green)

- **~200 components across 55 groups** — full shadcn/ui parity
- **1,702 Lucide icons** as components, plus a by‑name `LucideIconCn` dispatcher
- **CVA + `Cn.Merge()`** — C# ports of `class-variance-authority` and `cn()` (tailwind‑merge)
- **Exact shadcn theming** — CSS variables, `.dark` class dark mode
- **Trimmable / AOT‑friendly**, XML‑documented public API, MIT licensed

---

## Table of contents
- [Installation](#installation)
- [Quick start](#quick-start)
- [Theming & dark mode](#theming--dark-mode)
- [Utilities (`Cn.Merge`, `Cva`)](#utilities)
- [Component catalog](#component-catalog)
- [Lucide icons](#lucide-icons)
- [Render modes & AOT](#render-modes--aot)
- [Project structure](#project-structure)
- [Development](#development)
- [License](#license)

---

## Installation

### 1. Add the package
```bash
dotnet add package BlazorCN
```

### 2. Register services
BlazorCN ships one DI helper that wires up the JS interop and toast service:
```csharp
// Program.cs
using BlazorCN;

builder.Services.AddBlazorCN(); // registers JsInteropCn + ToastService (scoped)
```

### 3. Reference the theme stylesheet
Add the CSS‑variable theme (provides `--background`, `--primary`, `.dark`, etc.) to your host page (`index.html`, `App.razor`, or `_Host.cshtml`):
```html
<link rel="stylesheet" href="_content/BlazorCN/blazorcn.css" />
<link rel="stylesheet" href="app.css" /> <!-- your compiled Tailwind output -->
```
> The JS interop module (`_content/BlazorCN/blazorcn.js`) is imported **on demand** — no `<script>` tag required.

### 4. Configure Tailwind (v4, CSS‑first)
BlazorCN outputs Tailwind utility classes, so your app needs Tailwind. Create a `Styles/app.css`:
```css
@import "tailwindcss";
@import "tw-animate-css";

/* BlazorCN component classes (must be processed by Tailwind for @apply) */
@import "../_content/BlazorCN/blazorcn-components.css";

/* Tailwind must "see" the utility classes the components emit. Scan your own
   markup, and point @source at the BlazorCN component sources you reference. */
@source "../Components/**/*.razor";   /* your app's components */

@custom-variant dark (&:is(.dark *));

/* Map the shadcn CSS variables to Tailwind tokens */
@theme inline {
  --color-background: var(--background);
  --color-foreground: var(--foreground);
  --color-primary: var(--primary);
  --color-primary-foreground: var(--primary-foreground);
  /* ...full token list in docs/BlazorCN.Demo/Styles/app.css... */
}
```
Build it:
```bash
npx @tailwindcss/cli -i ./Styles/app.css -o ./wwwroot/app.css --minify
```
> A ready‑made Tailwind preset is shipped at `_content/BlazorCN/tailwind-preset.js` for JS‑config setups. The complete, working reference setup lives in **`docs/BlazorCN.Demo`**.

### 5. Import the namespace
BlazorCN uses a single flat namespace:
```razor
@* _Imports.razor *@
@using BlazorCN
```

---

## Quick start

```razor
@using BlazorCN

<CardCn Class="w-[380px]">
    <CardHeaderCn>
        <CardTitleCn>Create project</CardTitleCn>
        <CardDescriptionCn>Deploy your new project in one click.</CardDescriptionCn>
    </CardHeaderCn>
    <CardContentCn>
        <div class="flex flex-col gap-2">
            <LabelCn For="name">Name</LabelCn>
            <InputCn id="name" Placeholder="Name of your project" @bind-Value="_name" />
        </div>
    </CardContentCn>
    <CardFooterCn Class="justify-between">
        <ButtonCn Variant="ButtonVariant.Outline">Cancel</ButtonCn>
        <ButtonCn OnClick="Deploy">Deploy</ButtonCn>
    </CardFooterCn>
</CardCn>

@code {
    private string? _name;
    private void Deploy() { /* ... */ }
}
```

Every component is suffixed with **`Cn`** (e.g. `ButtonCn`, `DialogCn`) and accepts:
- `Class` — extra Tailwind classes, intelligently merged via `Cn.Merge()`
- `Style` — inline styles
- arbitrary attributes (`id`, `data-*`, `aria-*`, …) — captured and forwarded

Interactive components expose typed callbacks — e.g. `ButtonCn` has `OnClick` as `EventCallback<MouseEventArgs>`, so both `OnClick="Handler"` and `@onclick="Handler"` work.

---

## Theming & dark mode

Theming is the exact shadcn/ui system: semantic CSS variables defined on `:root` and `.dark` in `_content/BlazorCN/blazorcn.css`. Override them anywhere to retheme:

```css
:root {
  --primary: oklch(0.205 0 0);
  --radius: 0.65rem;
}
```

Toggle dark mode by adding/removing the `dark` class on `<html>`:
```csharp
await JS.InvokeVoidAsync("document.documentElement.classList.toggle", "dark");
```

---

## Utilities

### `Cn.Merge(...)` — tailwind‑merge for C#
Resolves conflicting Tailwind utilities (last one wins per group) so consumer `Class` overrides behave intuitively:
```csharp
Cn.Merge("px-2 py-1 bg-red-500", "bg-blue-500"); // → "px-2 py-1 bg-blue-500"
```

### `Cva<TVariant, TSize>` — class‑variance‑authority for C#
Type‑safe variant → class maps, used internally by components and available to yours:
```csharp
private static readonly Cva<ButtonVariant, ButtonSize> Variants = new(
    baseClasses: "inline-flex items-center justify-center rounded-lg ...",
    variants: new() { [ButtonVariant.Default] = "bg-primary text-primary-foreground", ... },
    sizes:    new() { [ButtonSize.Default] = "h-8 px-4", ... });
```

---

## Component catalog

> ~200 components in 55 groups. Browse them live with `docs/BlazorCN.Demo` at `/docs/components/{name}`.

### Layout & structure
| Group | Components | Description |
|---|---|---|
| **AspectRatio** | `AspectRatioCn` | Constrain content to a width/height ratio |
| **Card** | `CardCn` + `Header`/`Title`/`Description`/`Action`/`Content`/`Footer` | Content container |
| **Separator** | `SeparatorCn` | Horizontal/vertical divider |
| **ScrollArea** | `ScrollAreaCn`, `ScrollBarCn` | Scroll container |
| **Resizable** | `ResizablePanelGroupCn`, `ResizablePanelCn`, `ResizableHandleCn` | Resizable split panes |
| **Sidebar** | `SidebarProviderCn`, `SidebarCn` + 20 parts | Collapsible app sidebar |
| **Item** | `ItemCn` + `Group`/`Header`/`Media`/`Content`/`Title`/`Description`/`Actions`/`Footer`/`Separator` | List‑item primitive |
| **Empty** | `EmptyCn` + `Header`/`Media`/`Title`/`Description`/`Content` | Empty‑state placeholder |
| **Field** | `FieldCn`/`FieldSetCn` + `Legend`/`Group`/`Label`/`Title`/`Description`/`Error`/`Content`/`Separator` | Form field layout |
| **Table** | `TableCn` + `Header`/`Body`/`Footer`/`Row`/`Head`/`Cell`/`Caption` | Data table primitives |

### Forms & inputs
| Group | Components | Description |
|---|---|---|
| **Button** | `ButtonCn` | 6 variants × 8 sizes; `Href` renders an anchor |
| **ButtonGroup** | `ButtonGroupCn`, `ButtonGroupSeparatorCn`, `ButtonGroupTextCn` | Connected button row/column |
| **Input** | `InputCn` | Text input, `@bind-Value` |
| **InputGroup** | `InputGroupCn` + `Addon`/`Input`/`Textarea`/`Button`/`Text` | Input with addons |
| **InputOtp** | `InputOtpCn`, `InputOtpGroupCn`, `InputOtpSlotCn`, `InputOtpSeparatorCn` | One‑time‑password input |
| **Textarea** | `TextareaCn` | Multi‑line input |
| **Label** | `LabelCn` | Accessible form label |
| **Checkbox** | `CheckboxCn` | `@bind-Value` checkbox |
| **RadioGroup** | `RadioGroupCn`, `RadioGroupItemCn` | Single‑select radios |
| **Switch** | `SwitchCn` | Toggle switch |
| **Select** | `SelectCn` + `Trigger`/`Value`/`Content`/`Group`/`Label`/`Item`/`Separator` | Custom dropdown select |
| **NativeSelect** | `NativeSelectCn`, `NativeSelectOptionCn`, `NativeSelectOptGroupCn` | Native `<select>` |
| **Combobox** | `ComboboxCn` + `Trigger`/`Input`/`Content`/`Group`/`Item`/`Empty`/`Separator` | Autocomplete select |
| **Slider** | `SliderCn` | Range slider |
| **Form** | `FormFieldCn`, `FormLabelCn`, `FormControlCn`, `FormDescriptionCn`, `FormMessageCn` | `EditForm` integration |
| **Calendar** | `CalendarCn` | Date picker calendar (single/multiple/range) |
| **Toggle** | `ToggleCn` | Two‑state toggle button |
| **ToggleGroup** | `ToggleGroupCn`, `ToggleGroupItemCn` | Grouped toggles |

### Navigation
| Group | Components | Description |
|---|---|---|
| **Breadcrumb** | `BreadcrumbCn` + `List`/`Item`/`Link`/`Page`/`Separator`/`Ellipsis` | Breadcrumb trail |
| **NavigationMenu** | `NavigationMenuCn` + `List`/`Item`/`Trigger`/`Content`/`Link`/`Indicator`/`Viewport` | Top‑level nav with dropdowns |
| **Pagination** | `PaginationCn` + `Content`/`Item`/`Link`/`Previous`/`Next`/`Ellipsis` | Page navigation |
| **Menubar** | `MenubarCn` + `Menu`/`Trigger`/`Content`/`Item`/`Checkbox`/`Radio`/`Sub`/`Label`/`Separator`/`Shortcut` | Desktop‑style menu bar |
| **Tabs** | `TabsCn`, `TabsListCn`, `TabsTriggerCn`, `TabsContentCn` | Tabbed panels (default & line variants) |

### Overlays & popups
| Group | Components | Description |
|---|---|---|
| **Dialog** | `DialogCn` + `Trigger`/`Content`/`Header`/`Title`/`Description`/`Footer`/`Overlay`/`Close` | Modal dialog |
| **AlertDialog** | `AlertDialogCn` + `Trigger`/`Content`/`Header`/`Title`/`Description`/`Footer`/`Action`/`Cancel`/`Overlay` | Confirmation dialog |
| **Sheet** | `SheetCn` + `Trigger`/`Content`/`Header`/`Title`/`Description`/`Footer`/`Overlay`/`Close` | Side panel |
| **Drawer** | `DrawerCn` + `Trigger`/`Content`/`Header`/`Title`/`Description`/`Footer`/`Overlay`/`Close` | Bottom drawer |
| **Popover** | `PopoverCn` + `Trigger`/`Anchor`/`Content`/`Header`/`Title`/`Description` | Floating popover |
| **HoverCard** | `HoverCardCn`, `HoverCardTriggerCn`, `HoverCardContentCn` | Hover‑to‑reveal card |
| **Tooltip** | `TooltipCn`, `TooltipTriggerCn`, `TooltipContentCn` | Tooltip |
| **DropdownMenu** | `DropdownMenuCn` + 13 parts (items, checkbox/radio, sub‑menus, shortcuts) | Action menu |
| **ContextMenu** | `ContextMenuCn` + 13 parts | Right‑click menu |
| **Command** | `CommandCn` + `Input`/`List`/`Group`/`Item`/`Empty`/`Separator` | Command palette |

### Feedback & status
| Group | Components | Description |
|---|---|---|
| **Alert** | `AlertCn`, `AlertTitleCn`, `AlertDescriptionCn` | Inline alert |
| **Badge** | `BadgeCn` | Status badge |
| **Progress** | `ProgressCn` | Progress bar |
| **Skeleton** | `SkeletonCn` | Loading placeholder |
| **Spinner** | `SpinnerCn` | Loading spinner |
| **Toast** | `ToasterCn`, `ToastCn` + `ToastService` | Toast notifications (call `ToastService.Success(...)`) |
| **Kbd** | `KbdCn` | Keyboard shortcut hint |

### Data display
| Group | Components | Description |
|---|---|---|
| **Avatar** | `AvatarCn` + `Image`/`Fallback`/`Badge`/`Group`/`GroupCount` | User avatar & stacks |
| **Accordion** | `AccordionCn`, `AccordionItemCn`, `AccordionTriggerCn`, `AccordionContentCn` | Collapsible sections |
| **Collapsible** | `CollapsibleCn`, `CollapsibleTriggerCn`, `CollapsibleContentCn` | Single collapsible region |
| **Carousel** | `CarouselCn` + `Content`/`Item`/`Previous`/`Next` | Horizontal/vertical carousel |
| **Chart** | `ChartCn`, `ChartContainerCn` | Chart container/theming |

> **Composed recipes** (built from the primitives above, see the demo): **Date Picker** = `Calendar` + `Popover`; **Data Table** = `Table` + sorting/filtering; **File Upload** = `InputGroup` + `Item`.

---

## Lucide icons

All **1,702 [Lucide](https://lucide.dev) icons** ship as components, e.g.:
```razor
<LucideCheckCn Size="16" Class="text-green-600" />
<LucideTriangleAlertCn />
```
Each accepts `Size`, `Fill`, `Stroke`, `StrokeWidth`, `Class`, `Style`.

Or pick an icon **by name** with the dispatcher (handles lucide v1→v2 renames):
```razor
<LucideIconCn Name="circle-check-big" Size="20" Class="text-primary" />
```
> The dispatcher resolves icons via reflection. For Native‑AOT / aggressive trimming, prefer the concrete `Lucide{Name}Cn` component so the icon type is statically referenced.

---

## Render modes & AOT

Works under **Server, WebAssembly, Auto, and Static SSR**. JS interop (Floating‑UI positioning, focus trap, scroll lock) is loaded lazily and degrades gracefully during prerender.

For Native‑AOT/trimmed WASM: JS‑interop option objects use plain classes with init‑only properties (not records) for source‑generated serialization. Reference concrete icon components rather than the by‑name dispatcher.

---

## Project structure

```
blazorcn/
├── src/BlazorCN/
│   ├── ComponentBaseCn.cs                 # base: Class, Style, AdditionalAttributes
│   ├── ServiceCollectionExtensions.cs     # AddBlazorCN()
│   ├── JsInteropCn.cs                      # typed JS interop wrapper
│   ├── Utilities/{Cn.cs, Cva.cs}          # tailwind-merge + CVA ports
│   ├── Components/                         # 55 groups + LucideIcon/Icons (1702)
│   └── wwwroot/{blazorcn.css, blazorcn-components.css, blazorcn.js, tailwind-preset.js}
├── tests/BlazorCN.Tests/                  # bUnit + xUnit
└── docs/BlazorCN.Demo/                    # reference consumer app + live docs
```

---

## Development

```bash
dotnet build                                   # build library
dotnet test                                    # run bUnit/xUnit suite
dotnet pack src/BlazorCN/BlazorCN.csproj -c Release   # produce NuGet package

# Run the docs/demo app
cd docs/BlazorCN.Demo
npm install && npm run dev:css                 # Tailwind watch (separate terminal)
dotnet run                                     # serves the component gallery
```

---

## License

[MIT](LICENSE) © BlazorCN Contributors. Lucide icons are [ISC licensed](https://lucide.dev/license).
Design and component APIs mirror [shadcn/ui](https://ui.shadcn.com) (MIT).
