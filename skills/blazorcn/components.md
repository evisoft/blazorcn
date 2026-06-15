# Component Catalog

~200 components across ~55 groups. Every component ends in `Cn` and lives in
`@using BlazorCN`. This file is a map — for the **exact** parameter list of any
component, read its source: `src/BlazorCN/Components/<Group>/*.razor` (+ `.razor.cs`
and any `enum` `.cs`). Real usage lives in `docs/BlazorCN.Demo` and the live demo
at `/docs/components/{name}`.

> Composed sub-components are listed as suffixes — e.g. `CardCn` +
> `Header`/`Title`/… means `CardHeaderCn`, `CardTitleCn`, … Group/section wrappers
> (e.g. `SelectGroupCn`) are **optional** — see
> [rules/composition.md](./rules/composition.md#group-wrappers-are-optional-in-blazorcn).

---

## Layout & structure

| Group | Components |
| --- | --- |
| AspectRatio | `AspectRatioCn` |
| Card | `CardCn` + `Header`/`Title`/`Description`/`Action`/`Content`/`Footer` |
| Separator | `SeparatorCn` (`Orientation`) |
| ScrollArea | `ScrollAreaCn`, `ScrollBarCn` |
| Resizable | `ResizablePanelGroupCn`, `ResizablePanelCn`, `ResizableHandleCn` |
| Sidebar | `SidebarProviderCn`, `SidebarCn` + ~20 parts (`Header`/`Content`/`Footer`/`Menu`/`MenuItem`/`MenuButton`/`Group`/`Trigger`/…) |
| Item | `ItemCn` + `Group`/`Header`/`Media`/`Content`/`Title`/`Description`/`Actions`/`Footer`/`Separator` |
| Empty | `EmptyCn` + `Header`/`Media`/`Title`/`Description`/`Content` |
| Field | `FieldCn`/`FieldSetCn` + `Legend`/`Group`/`Label`/`Title`/`Description`/`Error`/`Content`/`Separator` |
| Table | `TableCn` + `Header`/`Body`/`Footer`/`Row`/`Head`/`Cell`/`Caption` |

## Forms & inputs

| Group | Components |
| --- | --- |
| Button | `ButtonCn` (`Variant`, `Size`, `Disabled`, `Type`, `Href`, `OnClick`) |
| ButtonGroup | `ButtonGroupCn`, `ButtonGroupSeparatorCn`, `ButtonGroupTextCn` |
| Input | `InputCn` (`@bind-Value` string) |
| InputGroup | `InputGroupCn` + `Addon`/`Input`/`Textarea`/`Button`/`Text` |
| InputOtp | `InputOtpCn`, `InputOtpGroupCn`, `InputOtpSlotCn`, `InputOtpSeparatorCn` |
| Textarea | `TextareaCn` (`@bind-Value` string) |
| Label | `LabelCn` (`For`) |
| Checkbox | `CheckboxCn` (`@bind-Checked` bool) |
| RadioGroup | `RadioGroupCn` (`@bind-Value`), `RadioGroupItemCn` (`Value`) |
| Switch | `SwitchCn` (`@bind-Checked` bool, `Size`) |
| Select | `SelectCn` (`@bind-Value`) + `Trigger`/`Value`/`Content`/`Group`/`Label`/`Item`/`Separator` |
| NativeSelect | `NativeSelectCn` (`@bind-Value`), `NativeSelectOptionCn`, `NativeSelectOptGroupCn` |
| Combobox | `ComboboxCn` (`@bind-Value`) + `Trigger`/`Input`/`Content`/`Group`/`Item`/`Empty`/`Separator` |
| Slider | `SliderCn` (`@bind-Value` double) |
| Form | `FormFieldCn`, `FormLabelCn`, `FormControlCn`, `FormDescriptionCn`, `FormMessageCn` (presentational) |
| Calendar | `CalendarCn` (single/multiple/range) |
| Toggle | `ToggleCn` |
| ToggleGroup | `ToggleGroupCn` (`@bind-Value`), `ToggleGroupItemCn` (`Value`) |

## Navigation

| Group | Components |
| --- | --- |
| Breadcrumb | `BreadcrumbCn` + `List`/`Item`/`Link`/`Page`/`Separator`/`Ellipsis` |
| NavigationMenu | `NavigationMenuCn` + `List`/`Item`/`Trigger`/`Content`/`Link`/`Indicator`/`Viewport` |
| Pagination | `PaginationCn` + `Content`/`Item`/`Link`/`Previous`/`Next`/`Ellipsis` |
| Menubar | `MenubarCn` + `Menu`/`Trigger`/`Content`/`Item`/`Checkbox`/`Radio`/`Sub`/`Label`/`Separator`/`Shortcut` |
| Tabs | `TabsCn` (`@bind-Value`/`DefaultValue`), `TabsListCn`, `TabsTriggerCn` (`Value`), `TabsContentCn` (`Value`) |

## Overlays & popups

| Group | Components |
| --- | --- |
| Dialog | `DialogCn` (`@bind-Open`) + `Trigger`/`Content`/`Header`/`Title`/`Description`/`Footer`/`Overlay`/`Close` |
| AlertDialog | `AlertDialogCn` (`@bind-Open`) + `Trigger`/`Content`/`Header`/`Title`/`Description`/`Footer`/`Action`/`Cancel`/`Overlay` |
| Sheet | `SheetCn` (`@bind-Open`) + `Trigger`/`Content` (`Side`)/`Header`/`Title`/`Description`/`Footer`/`Overlay`/`Close` |
| Drawer | `DrawerCn` (`@bind-Open`) + `Trigger`/`Content`/`Header`/`Title`/`Description`/`Footer`/`Overlay`/`Close` |
| Popover | `PopoverCn` (`@bind-Open`) + `Trigger`/`Anchor`/`Content` (`Side`/`Align`)/`Header`/`Title`/`Description` |
| HoverCard | `HoverCardCn` (`OpenDelay`/`CloseDelay`), `HoverCardTriggerCn`, `HoverCardContentCn` |
| Tooltip | `TooltipCn` (`OpenDelay`/`CloseDelay`), `TooltipTriggerCn`, `TooltipContentCn` |
| DropdownMenu | `DropdownMenuCn` (`@bind-Open`) + `Trigger`/`Content` (`Align`)/`Item`/`CheckboxItem`/`RadioGroup`/`RadioItem`/`Label`/`Separator`/`Shortcut`/`Group`/`Sub`/`SubTrigger`/`SubContent` |
| ContextMenu | `ContextMenuCn` + 13 parts (mirror of DropdownMenu) |
| Command | `CommandCn` + `Input`/`List`/`Group`/`Item`/`Empty`/`Separator` |

## Feedback & status

| Group | Components |
| --- | --- |
| Alert | `AlertCn`, `AlertTitleCn`, `AlertDescriptionCn` |
| Badge | `BadgeCn` (`Variant`) |
| Progress | `ProgressCn` (`Value`) |
| Skeleton | `SkeletonCn` |
| Spinner | `SpinnerCn` |
| Toast | `ToasterCn` + inject `ToastService` (`Success`/`Error`/`Info`/`Warning`) |
| Kbd | `KbdCn` |

## Data display

| Group | Components |
| --- | --- |
| Avatar | `AvatarCn` + `Image`/`Fallback`/`Badge`/`Group`/`GroupCount` |
| Accordion | `AccordionCn`, `AccordionItemCn` (`Value`), `AccordionTriggerCn`, `AccordionContentCn` |
| Collapsible | `CollapsibleCn` (`@bind-Open`), `CollapsibleTriggerCn`, `CollapsibleContentCn` |
| Carousel | `CarouselCn` + `Content`/`Item`/`Previous`/`Next` |
| Chart | `ChartCn`, `ChartContainerCn` |

## Icons

`Lucide{Name}Cn` (1,702 components) + `LucideIconCn` (`Name` dispatcher). See
[rules/icons.md](./rules/icons.md).

---

## Quick API — high-frequency components

Verified against source. For anything not listed, read the component.

### `ButtonCn`
- `Variant`: `ButtonVariant` = `Default` | `Destructive` | `Outline` | `Secondary` | `Ghost` | `Link`
- `Size`: `ButtonSize` = `Default` | `Xs` | `Sm` | `Lg` | `Icon` | `IconXs` | `IconSm` | `IconLg`
- `Disabled` (bool), `Type` (string, `"button"`; use `"submit"` in forms),
  `Href` (string? → renders `<a>`, and `OnClick` is then ignored),
  `OnClick` (`EventCallback<MouseEventArgs>`).
- No `IsLoading` — compose a `SpinnerCn`. Icon-only: `Size="ButtonSize.Icon"` + `aria-label`.

### `BadgeCn`
- `Variant`: `BadgeVariant` = `Default` | `Secondary` | `Destructive` | `Outline` | `Ghost` | `Link`.
- No `Size`, no `Href` (wrap in your own `<a>` to link).

### `InputCn` / `TextareaCn`
- `@bind-Value` (`string?`), `Disabled`. Other attributes (`type`, `placeholder`,
  `aria-invalid`, `id`, …) pass through to the native element. Placeholder may be
  `Placeholder="..."` or the raw `placeholder` attribute.

### `CheckboxCn` / `SwitchCn`
- `@bind-Checked` (`bool`), `Disabled`. `SwitchCn` adds `Size` (`SwitchSize`).

### `SliderCn`
- `@bind-Value` (`double`), plus `Min`/`Max`/`Step` (read source for exact names).

### `SelectCn`
- `@bind-Value` (`string?`). Structure: `SelectTriggerCn` → `SelectValueCn`;
  `SelectContentCn` → `SelectItemCn` (`Value` required). Groups optional.

### `DialogCn` (and Sheet/Drawer/AlertDialog)
- `@bind-Open` (`bool`, optional — trigger opens it). Always include a `*TitleCn`.
  AlertDialog/Sheet need an explicit `*OverlayCn`. See
  [composition.md](./rules/composition.md#overlays-open-state-triggers-overlays-titles).

### `TabsCn`
- `@bind-Value` or `DefaultValue` (`string?`), `Orientation`. `TabsTriggerCn`/
  `TabsContentCn` keyed by `Value`. Triggers go inside `TabsListCn`.

### `AvatarCn`
- Compose `AvatarImageCn` (`Src`/`Alt`) + `AvatarFallbackCn` (always include the fallback).

### `FieldCn`
- `Orientation`: `FieldOrientation` = `Vertical` | `Horizontal` | `Responsive`.
  Set `data-invalid` / `data-disabled` yourself from validation state.

### `LucideXxxCn` / `LucideIconCn`
- `Size` (int=24), `Fill`, `Stroke`, `StrokeWidth`, `Class`, `Style`;
  `LucideIconCn` adds `Name`. Don't size icons nested in components.

---

## Common enums

| Enum | Values |
| --- | --- |
| `ButtonVariant` | Default, Destructive, Outline, Secondary, Ghost, Link |
| `ButtonSize` | Default, Xs, Sm, Lg, Icon, IconXs, IconSm, IconLg |
| `BadgeVariant` | Default, Secondary, Destructive, Outline, Ghost, Link |
| `FieldOrientation` | Vertical, Horizontal, Responsive |
| `FieldLegendVariant` | Legend, Label |
| `Orientation` | Horizontal, Vertical (shared; used by ButtonGroup, Separator, Tabs, …) |
| `SwitchSize` | Default, Sm |
| `FloatingSide` | Top, Right, Bottom, Left (overlay `Side`) |
| `SheetSide` | Top, Right, Bottom, Left (`SheetContentCn` `Side`) |
| `FloatingAlign` | Start, Center, End (overlay `Align`) |

> Enum names/values are the source of truth in the component's `.cs` file. When in
> doubt, read it — don't guess a value.
