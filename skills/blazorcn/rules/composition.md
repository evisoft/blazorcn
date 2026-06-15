# Component Composition

## Contents

- Overlays: open state, triggers, overlays, titles
- Why triggers render their own `<button>` (no `asChild`)
- Group wrappers are OPTIONAL in BlazorCN
- Choosing between overlay components
- Card structure
- `TabsTriggerCn` inside `TabsListCn`
- `AvatarCn` needs `AvatarFallbackCn`
- Button has no `IsLoading` — compose a `SpinnerCn`
- Use existing components instead of custom markup
- Toasts via `ToastService`

---

## Overlays: open state, triggers, overlays, titles

Overlay families — `DialogCn`, `AlertDialogCn`, `SheetCn`, `DrawerCn`,
`PopoverCn`, `DropdownMenuCn`, `ContextMenuCn`, `MenubarCn` — share a pattern:

- **Open state** is `Open` + `OpenChanged` → `@bind-Open` (optional). The
  `*TriggerCn` opens it on its own via a cascading parameter, so `@bind-Open` is
  only needed when you want to open/close it programmatically.
- **`*TriggerCn` renders a real `<button>`** that opens the overlay. (Hover
  overlays `TooltipCn`/`HoverCardCn` open on hover instead, via `OpenDelay`/
  `CloseDelay` — no trigger button.)
- **`*ContentCn`** is the floating panel, rendered only when open and positioned
  by JS interop (`Side`, `Align` via `FloatingAlign`, offsets).
- **Overlay/backdrop:** `DialogCn` renders its own overlay internally.
  `AlertDialogCn` and `SheetCn` do **not** — add `<AlertDialogOverlayCn />` /
  `<SheetOverlayCn />` explicitly before the content.
- **Title is required for accessibility.** Include `DialogTitleCn` / `SheetTitleCn`
  / `DrawerTitleCn` / `AlertDialogTitleCn`. Without it the content's
  `aria-labelledby` is dropped and the label falls back to a generic "Dialog".
  Use `Class="sr-only"` to hide it visually.

**Correct (Dialog):**

```razor
<DialogCn>
  <DialogTriggerCn>
    <ButtonCn Variant="ButtonVariant.Outline">Edit profile</ButtonCn>
  </DialogTriggerCn>
  <DialogContentCn>
    <DialogHeaderCn>
      <DialogTitleCn>Edit profile</DialogTitleCn>
      <DialogDescriptionCn>Make changes, then save.</DialogDescriptionCn>
    </DialogHeaderCn>
    @* …fields… *@
    <DialogFooterCn>
      <DialogCloseCn><ButtonCn Variant="ButtonVariant.Outline">Cancel</ButtonCn></DialogCloseCn>
      <ButtonCn>Save</ButtonCn>
    </DialogFooterCn>
  </DialogContentCn>
</DialogCn>
```

**Correct (AlertDialog — note the explicit overlay):**

```razor
<AlertDialogCn>
  <AlertDialogTriggerCn>
    <ButtonCn Variant="ButtonVariant.Outline">Delete</ButtonCn>
  </AlertDialogTriggerCn>
  <AlertDialogOverlayCn />
  <AlertDialogContentCn>
    <AlertDialogHeaderCn>
      <AlertDialogTitleCn>Are you sure?</AlertDialogTitleCn>
      <AlertDialogDescriptionCn>This can't be undone.</AlertDialogDescriptionCn>
    </AlertDialogHeaderCn>
    <AlertDialogFooterCn>
      <AlertDialogCancelCn>Cancel</AlertDialogCancelCn>
      <AlertDialogActionCn OnClick="Delete">Continue</AlertDialogActionCn>
    </AlertDialogFooterCn>
  </AlertDialogContentCn>
</AlertDialogCn>
```

---

## Why triggers render their own `<button>` (no `asChild`)

shadcn/Radix uses `<DialogTrigger asChild>` to merge trigger behavior onto a child
element. Blazor has no `asChild`/slot-merging primitive, so **BlazorCN triggers
*are* real `<button>` elements** that wire to the parent overlay via a cascading
parameter (`SetOpen(true)`). This keeps the components thin and the trigger always
keyboard-accessible.

Consequence: nesting `<ButtonCn>` inside a `*TriggerCn` (the demo's common idiom)
produces a `<button>` inside a `<button>` — technically invalid HTML, though it
renders and works. Two clean alternatives when you care about valid markup:

```razor
@* A) Style the trigger directly (it already is a button). *@
<DialogTriggerCn Class="inline-flex h-9 items-center rounded-md border px-4 text-sm">
  Open
</DialogTriggerCn>

@* B) Use @bind-Open with a standalone ButtonCn — fully valid, full styling. *@
<ButtonCn OnClick="() => _open = true">Open</ButtonCn>
<DialogCn @bind-Open="_open">
  <DialogContentCn>
    <DialogHeaderCn><DialogTitleCn>…</DialogTitleCn></DialogHeaderCn>
  </DialogContentCn>
</DialogCn>
```

---

## Group wrappers are OPTIONAL in BlazorCN

**This is the opposite of shadcn/React.** Items cascade from the root component, so
they sit directly inside the content container. Use a `*GroupCn` only to create a
**labeled section**, not as a required wrapper.

**Correct (no group needed):**

```razor
<SelectContentCn>
  <SelectItemCn Value="apple">Apple</SelectItemCn>
  <SelectItemCn Value="banana">Banana</SelectItemCn>
</SelectContentCn>

<DropdownMenuContentCn>
  <DropdownMenuItemCn>Edit</DropdownMenuItemCn>
  <DropdownMenuSeparatorCn />
  <DropdownMenuItemCn Class="text-destructive">Delete</DropdownMenuItemCn>
</DropdownMenuContentCn>
```

**Also correct (grouped + labeled, when you want sections):**

```razor
<SelectContentCn>
  <SelectGroupCn>
    <SelectLabelCn>Fruits</SelectLabelCn>
    <SelectItemCn Value="apple">Apple</SelectItemCn>
  </SelectGroupCn>
</SelectContentCn>
```

Applies to `SelectItemCn`, `DropdownMenuItemCn`, `ContextMenuItemCn`,
`MenubarItemCn`, `CommandItemCn`.

---

## Choosing between overlay components

| Use case | Component |
| --- | --- |
| Focused task requiring input | `DialogCn` |
| Destructive-action confirmation | `AlertDialogCn` |
| Side panel (details/filters) | `SheetCn` |
| Mobile-first bottom panel | `DrawerCn` |
| Quick info on hover | `HoverCardCn` |
| Small contextual content on click | `PopoverCn` |
| Action menu on click | `DropdownMenuCn` |
| Right-click menu | `ContextMenuCn` |

---

## Card structure

Use full composition — don't dump everything into `CardContentCn`:

```razor
<CardCn>
  <CardHeaderCn>
    <CardTitleCn>Team Members</CardTitleCn>
    <CardDescriptionCn>Manage your team.</CardDescriptionCn>
  </CardHeaderCn>
  <CardContentCn>…</CardContentCn>
  <CardFooterCn>
    <ButtonCn>Invite</ButtonCn>
  </CardFooterCn>
</CardCn>
```

`CardActionCn` (in the header) holds a header-aligned action.

---

## `TabsTriggerCn` inside `TabsListCn`

Never put triggers directly in `TabsCn`. Bind the active tab with `@bind-Value`
(or set `DefaultValue` for uncontrolled):

```razor
<TabsCn DefaultValue="account">
  <TabsListCn>
    <TabsTriggerCn Value="account">Account</TabsTriggerCn>
    <TabsTriggerCn Value="password">Password</TabsTriggerCn>
  </TabsListCn>
  <TabsContentCn Value="account">…</TabsContentCn>
  <TabsContentCn Value="password">…</TabsContentCn>
</TabsCn>
```

---

## `AvatarCn` needs `AvatarFallbackCn`

Always include a fallback for when the image fails:

```razor
<AvatarCn>
  <AvatarImageCn Src="/avatar.png" Alt="User" />
  <AvatarFallbackCn>JD</AvatarFallbackCn>
</AvatarCn>
```

---

## Button has no `IsLoading` — compose a `SpinnerCn`

```razor
<ButtonCn Disabled="_saving">
  @if (_saving) { <SpinnerCn Class="size-4" /> }
  Save
</ButtonCn>
```

---

## Use existing components instead of custom markup

| Instead of | Use |
| --- | --- |
| `<hr>` / `<div class="border-t">` | `<SeparatorCn />` |
| `<div class="animate-pulse">…</div>` | `<SkeletonCn Class="h-4 w-3/4" />` |
| a styled status `<span>` | `<BadgeCn Variant="BadgeVariant.Secondary">` |
| a custom callout `<div>` | `<AlertCn>` + `AlertTitleCn` / `AlertDescriptionCn` |
| a hand-built empty state | `<EmptyCn>` + `EmptyHeaderCn`/`EmptyMediaCn`/`EmptyTitleCn`/`EmptyDescriptionCn` |
| a hand-rolled spinner | `<SpinnerCn />` |

---

## Toasts via `ToastService`

Inject the service and render one `<ToasterCn />` in your layout:

```razor
@inject ToastService Toast

<ButtonCn OnClick="@(() => Toast.Success("Saved."))">Save</ButtonCn>
```

```razor
@* In MainLayout.razor *@
<ToasterCn />
```

Don't build custom toast markup — use `ToastService` (`Success`/`Error`/`Info`/…).
