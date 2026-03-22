# BlazorCN — Design Document

**Date:** 2026-03-22
**Goal:** Production-ready Blazor component library that replicates shadcn-ui one-to-one, built as a NuGet package.

---

## Decisions

| Decision | Choice |
|----------|--------|
| Library name | BlazorCN |
| .NET version | .NET 10 only |
| CSS approach | Tailwind CSS (consumer configures) |
| Naming convention | `Cn` suffix: `ButtonCn`, `CardCn`, `DialogCn` |
| Rendering modes | All (Server, WASM, Auto, Static SSR) |
| Architecture | Thin component wrappers, minimal base class |
| Component parity | Full shadcn-ui (~50 groups, ~200 components) |
| Testing | bUnit + xUnit |

---

## Project Structure

```
blazorcn/
├── src/
│   └── BlazorCN/
│       ├── BlazorCN.csproj
│       ├── ComponentBaseCn.cs
│       ├── Utilities/
│       │   ├── Cn.cs
│       │   └── Cva.cs
│       ├── Components/
│       │   ├── Button/
│       │   │   ├── ButtonCn.razor
│       │   │   └── ButtonCn.razor.cs
│       │   ├── Card/
│       │   │   ├── CardCn.razor
│       │   │   ├── CardHeaderCn.razor
│       │   │   ├── CardContentCn.razor
│       │   │   └── CardFooterCn.razor
│       │   ├── Dialog/
│       │   │   ├── DialogCn.razor
│       │   │   ├── DialogCn.razor.cs
│       │   │   └── ...
│       │   └── ... (all component folders)
│       ├── wwwroot/
│       │   ├── blazorcn.js
│       │   └── blazorcn.css
│       └── _Imports.razor
├── tests/
│   └── BlazorCN.Tests/
│       └── BlazorCN.Tests.csproj
├── docs/plans/
└── BlazorCN.slnx
```

**Namespace:** `BlazorCN` — flat, single `@using BlazorCN`.

---

## Base Class & Utilities

### ComponentBaseCn

```csharp
public abstract class ComponentBaseCn : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object?>? AdditionalAttributes { get; set; }
}
```

### Cn.Merge()

C# port of shadcn's `cn()` — merges Tailwind classes intelligently (last wins for conflicting utilities).

### Cva (Class Variance Authority)

Defines variant maps. Components call `ButtonVariants.Apply(Variant, Size, Class)` to resolve CSS classes.

---

## Component Patterns

### Pattern 1 — Simple (ButtonCn, BadgeCn, InputCn, LabelCn, SeparatorCn, SkeletonCn)

Single `.razor` file with variant parameters. Output semantic HTML with Tailwind classes.

### Pattern 2 — Composed (CardCn, AlertCn, TableCn, BreadcrumbCn)

Parent + child components. Children are thin markup wrappers. `CascadingValue` only when children need parent context.

### Pattern 3 — Interactive (DialogCn, SelectCn, PopoverCn, DropdownMenuCn, TooltipCn)

`.razor` + `.razor.cs`. JS interop for focus trap, outside click, floating positioning, keyboard navigation.

### Pattern 4 — Form (InputCn, CheckboxCn, RadioGroupCn, SelectCn, SwitchCn, SliderCn)

Support `@bind-Value`, `EditForm` integration, validation messages.

**Enums for variants/sizes** — `ButtonVariant`, `ButtonSize`, etc. Type-safe, IntelliSense-friendly.

---

## Theming

CSS variables in `blazorcn.css`, matching shadcn-ui exactly:

```css
:root {
    --background: 0 0% 100%;
    --foreground: 240 10% 3.9%;
    --primary: 240 5.9% 10%;
    --primary-foreground: 0 0% 98%;
    --secondary: 240 4.8% 95.9%;
    --secondary-foreground: 240 5.9% 10%;
    --muted: 240 4.8% 95.9%;
    --muted-foreground: 240 3.8% 46.1%;
    --accent: 240 4.8% 95.9%;
    --accent-foreground: 240 5.9% 10%;
    --destructive: 0 84.2% 60.2%;
    --destructive-foreground: 0 0% 98%;
    --border: 240 5.9% 90%;
    --input: 240 5.9% 90%;
    --ring: 240 5.9% 10%;
    --radius: 0.5rem;
    --card: 0 0% 100%;
    --card-foreground: 240 10% 3.9%;
    --popover: 0 0% 100%;
    --popover-foreground: 240 10% 3.9%;
}

.dark { /* dark mode overrides */ }
```

Dark mode via `class="dark"` on `<html>`. Consumers override variables in their own CSS.

---

## JS Interop

**Minimal.** Only for behaviors CSS can't handle:

- Focus trap (Dialog, Sheet, AlertDialog)
- Outside click detection (Popover, DropdownMenu, Select, Command)
- Floating positioning via Floating UI (Popover, Tooltip, DropdownMenu, Select, HoverCard)
- Scroll lock (Dialog, Sheet)
- Keyboard navigation (Command, Menu)

**~20 of 50 component groups need JS.** Rest are pure Blazor + CSS.

Wrapped in `JsInteropCn` class, registered as scoped service via `AddBlazorCN()`.

---

## Full Component List

| # | Component Group | BlazorCN Components | JS |
|---|----------------|--------------------|----|
| 1 | Accordion | AccordionCn, AccordionItemCn, AccordionTriggerCn, AccordionContentCn | No |
| 2 | Alert | AlertCn, AlertTitleCn, AlertDescriptionCn | No |
| 3 | Alert Dialog | AlertDialogCn, AlertDialogTriggerCn, AlertDialogContentCn, AlertDialogHeaderCn, AlertDialogFooterCn, AlertDialogTitleCn, AlertDialogDescriptionCn, AlertDialogActionCn, AlertDialogCancelCn | Yes |
| 4 | Aspect Ratio | AspectRatioCn | No |
| 5 | Avatar | AvatarCn, AvatarImageCn, AvatarFallbackCn | No |
| 6 | Badge | BadgeCn | No |
| 7 | Breadcrumb | BreadcrumbCn, BreadcrumbListCn, BreadcrumbItemCn, BreadcrumbLinkCn, BreadcrumbSeparatorCn, BreadcrumbEllipsisCn, BreadcrumbPageCn | No |
| 8 | Button | ButtonCn | No |
| 9 | Calendar | CalendarCn | No |
| 10 | Card | CardCn, CardHeaderCn, CardTitleCn, CardDescriptionCn, CardActionCn, CardContentCn, CardFooterCn | No |
| 11 | Carousel | CarouselCn, CarouselContentCn, CarouselItemCn, CarouselPreviousCn, CarouselNextCn | Yes |
| 12 | Checkbox | CheckboxCn | No |
| 13 | Collapsible | CollapsibleCn, CollapsibleTriggerCn, CollapsibleContentCn | No |
| 14 | Combobox | ComboboxCn | Yes |
| 15 | Command | CommandCn, CommandInputCn, CommandListCn, CommandEmptyCn, CommandGroupCn, CommandItemCn, CommandSeparatorCn | Yes |
| 16 | Context Menu | ContextMenuCn + sub-components | Yes |
| 17 | Dialog | DialogCn, DialogTriggerCn, DialogContentCn, DialogHeaderCn, DialogFooterCn, DialogTitleCn, DialogDescriptionCn, DialogCloseCn | Yes |
| 18 | Drawer | DrawerCn + sub-components | Yes |
| 19 | Dropdown Menu | DropdownMenuCn + sub-components | Yes |
| 20 | Form | FormCn, FormFieldCn, FormLabelCn, FormControlCn, FormDescriptionCn, FormMessageCn | No |
| 21 | Hover Card | HoverCardCn, HoverCardTriggerCn, HoverCardContentCn | Yes |
| 22 | Input | InputCn | No |
| 23 | Input OTP | InputOtpCn, InputOtpGroupCn, InputOtpSlotCn, InputOtpSeparatorCn | Yes |
| 24 | Label | LabelCn | No |
| 25 | Menubar | MenubarCn + sub-components | Yes |
| 26 | Navigation Menu | NavigationMenuCn + sub-components | Yes |
| 27 | Pagination | PaginationCn + sub-components | No |
| 28 | Popover | PopoverCn, PopoverTriggerCn, PopoverContentCn | Yes |
| 29 | Progress | ProgressCn | No |
| 30 | Radio Group | RadioGroupCn, RadioGroupItemCn | No |
| 31 | Resizable | ResizablePanelCn, ResizablePanelGroupCn, ResizableHandleCn | Yes |
| 32 | Scroll Area | ScrollAreaCn, ScrollBarCn | Yes |
| 33 | Select | SelectCn, SelectTriggerCn, SelectValueCn, SelectContentCn, SelectGroupCn, SelectLabelCn, SelectItemCn, SelectSeparatorCn | Yes |
| 34 | Separator | SeparatorCn | No |
| 35 | Sheet | SheetCn + sub-components | Yes |
| 36 | Skeleton | SkeletonCn | No |
| 37 | Slider | SliderCn | No |
| 38 | Sonner/Toast | ToasterCn, ToastCn | Yes |
| 39 | Switch | SwitchCn | No |
| 40 | Table | TableCn, TableHeaderCn, TableBodyCn, TableFooterCn, TableRowCn, TableHeadCn, TableCellCn, TableCaptionCn | No |
| 41 | Tabs | TabsCn, TabsListCn, TabsTriggerCn, TabsContentCn | No |
| 42 | Textarea | TextareaCn | No |
| 43 | Toggle | ToggleCn | No |
| 44 | Toggle Group | ToggleGroupCn, ToggleGroupItemCn | No |
| 45 | Tooltip | TooltipCn, TooltipTriggerCn, TooltipContentCn | Yes |
| 46 | Chart | ChartCn | Yes |
| 47 | Sidebar | SidebarCn + sub-components | No |
| 48 | Kbd | KbdCn | No |
| 49 | Spinner | SpinnerCn | No |
| 50 | Empty | EmptyCn | No |

---

## Consumer Experience

### Installation

```bash
dotnet add package BlazorCN
```

### Program.cs

```csharp
builder.Services.AddBlazorCN();
```

### _Imports.razor

```razor
@using BlazorCN
```

### App.razor / Layout

```html
<link rel="stylesheet" href="_content/BlazorCN/blazorcn.css" />
<script src="_content/BlazorCN/blazorcn.js" type="module"></script>
```

### Tailwind config

Extend with BlazorCN color preset (documented + optionally shipped as preset file).

### Usage

```razor
<CardCn>
    <CardHeaderCn>
        <CardTitleCn>Create project</CardTitleCn>
        <CardDescriptionCn>Deploy your new project in one-click.</CardDescriptionCn>
    </CardHeaderCn>
    <CardContentCn>
        <InputCn Placeholder="Project name" @bind-Value="name" />
    </CardContentCn>
    <CardFooterCn class="flex justify-between">
        <ButtonCn Variant="ButtonVariant.Outline">Cancel</ButtonCn>
        <ButtonCn OnClick="Deploy">Deploy</ButtonCn>
    </CardFooterCn>
</CardCn>
```

---

## Testing

- **Framework:** bUnit + xUnit
- Every component tested per variant
- Interactive components test open/close lifecycle
- Form components test `@bind-Value` and `EditForm` integration
- All components test `Class` passthrough and `AdditionalAttributes`
