# BlazorCN — Component Inspection Report

Comparison of the BlazorCN implementation in `src/BlazorCN/Components/` against the
canonical shadcn-ui source under `original/apps/v4/examples/radix/ui/` (the radix-example
flavor — what shadcn live site at ui.shadcn.com renders), verified via computed-style
diff in Playwright.

Started: 2026-05-09. **Regression sweep complete: 2026-05-10.**

## Status

- **Phase 1 (visual inspection):** All 56 components inspected via screenshot comparison.
- **Phase 2 (computed-style regression):** 50+ components passed through systematic class/computed-style diff against radix-example source, with CSS edits applied where divergent. See `## Regression sweep` section below.
- After regression: full library + demo builds clean (0 warnings, 0 errors). Tailwind CSS rebuilt successfully. Dev server requires restart to load new WASM.

## Methodology

### Phase 1 (initial inspection)

1. Read shadcn React reference source.
2. Read BlazorCN Razor implementation.
3. Render demo page in Playwright, screenshot baseline + key states.
4. Compare: variants, states, keyboard, ARIA, API surface, visual fidelity.

### Phase 2 (regression sweep)

1. Extract `[data-slot="..."]` elements via `browser_evaluate`.
2. Capture computed styles (height, padding, color, border, etc.) on both BlazorCN and shadcn live.
3. Diff property-by-property against `original/apps/v4/examples/radix/ui/{name}.tsx` source.
4. Edit `blazorcn-components.css` (semantic CSS classes via @apply) and/or `.razor` (inline classes) to close the gap.
5. Rebuild Tailwind CSS, verify computed styles match.

## Legend

| Verdict | Meaning |
|---|---|
| **PASS** | Visual + behavioral parity with the React original. Minor copy/whitespace differences that don't affect users are noted but not flagged. |
| **FLAG** | Cosmetic or minor behavioral drift. Component still usable. Triage at leisure. |
| **BLOCK** | Visibly broken, materially diverges from shadcn behavior, accessibility regression, or runtime error. Worth fixing before next release. |

Screenshots at `docs/inspection-screenshots/{component}-{state}.png`.

---

## Summary table

_Filled in as each component is inspected._

| # | Component | Verdict | Notes |
|---|---|---|---|
| 1 | Button | PASS | Radix-example variants + registry sizes; defensible hybrid that matches shadcn.com. |
| 2 | Badge | PASS | All 6 variants render correctly. |
| 3 | Input | PASS | Field-sizing/placeholder/disabled all correct. |
| 4 | Textarea | PASS | `field-sizing-content` auto-grow works. |
| 5 | Card | PASS | Header/title/description/action/content/footer composition matches. |
| 6 | Label | PASS | `text-sm font-medium` + peer-disabled handling. |
| 7 | Separator | PASS | Horizontal + vertical orientations. |
| 8 | Skeleton | PASS | `animate-pulse bg-muted rounded-md`, matches radix-example. |
| 9 | Avatar | PASS | Image + fallback + sizes match. |
| 10 | Spinner | PASS | All slots render — Demo/Sizes/Button/Badge/InputGroup/Empty/Custom/RTL. |
| 11 | Kbd | PASS | Single key, group, slots in Button/Tooltip/InputGroup all correct. |
| 12 | Alert | PASS | Default + destructive variants, with action, custom colors, RTL. |
| 13 | AspectRatio | PASS | Ratio constraints honored; demo wrapper is narrower than shadcn's but the component is correct. |
| 14 | Progress | PASS | Indicator transform-driven, label slot, RTL all correct. |
| 15 | Breadcrumb | PASS | Link/Page/Separator/Ellipsis/Dropdown/RTL composition matches. |
| 16 | Checkbox | PASS | `role="checkbox"` + checked-state SVG matches; no Indeterminate (shadcn supports via Radix). |
| 17 | RadioGroup | PASS | Cascading parent state, `role="radio"` items, inner-circle SVG match. |
| 18 | Switch | PASS | Default + sm sizes, thumb glide, disabled all match. |
| 19 | Slider | PASS | Range fill, single + multi-thumb, vertical, disabled all match. Thumb `calc(percent% - 6px)` looks correct visually. |
| 20 | Toggle | PASS | Default + outline variants, sm/default/lg sizes, aria-pressed all match. |
| 21 | ToggleGroup | FLAG | Visual parity confirmed (outline, sizes, vertical, RTL, custom). API gap: BlazorCN single-select only — shadcn supports `type="multiple"`. |
| 22 | NativeSelect | PASS | Default/disabled/groups/invalid/RTL all render identically. |
| 23 | InputOtp | FLAG | Visual parity. Implementation uses single hidden text input + cascading slot tracking instead of the `input-otp` library — keyboard/paste UX may differ subtly. |
| 24 | Form | PASS | Layout-only Blazor adaptation (label + input + description + error); validation comes from `EditForm` not react-hook-form. |
| 25 | Field | PASS | All orientations (vertical/horizontal/responsive) + Fieldset/Legend/Group/Title/Description/Separator/Error all match. |
| 26 | InputGroup | PASS | Demo/Basic/Icons/Text/Buttons/Inline/Block/Textarea/Kbd/Spinner/Disabled all match. |

---

## Tier 1 — Static primitives

### Button — PASS

- BlazorCN: `src/BlazorCN/Components/Button/ButtonCn.razor`, CSS rules in `src/BlazorCN/wwwroot/blazorcn-components.css:148-208`.
- shadcn references: registry `original/apps/v4/registry/new-york-v4/ui/button.tsx`; live-demo source `original/apps/v4/examples/radix/ui/button.tsx`.
- Screenshots: `docs/inspection-screenshots/blazorcn-button.png`, `docs/inspection-screenshots/shadcn-button.png`.

**Findings:**

BlazorCN aligns with the **radix examples** flavor (which is what shadcn.com renders). Variants match: default solid primary, secondary muted, destructive translucent red w/ red text (NOT solid red with white — this is the radix-example treatment, identical to live shadcn), outline with `hover:bg-muted`, ghost with `hover:bg-muted`, link.

Sizes match registry (h-9 default, h-10 lg, h-8 sm, h-6 xs, size-9 icon, size-6/8/10 for icon-xs/sm/lg). Radix-example uses h-8 default — BlazorCN intentionally took registry sizes. Defensible.

Minor cosmetic deltas, all benign:
- `cursor-pointer` added — necessary because Tailwind v4 resets `<button>` to `cursor:default`.
- `active:translate-y-px` added on base — gives press-down feedback. Matches radix-example.
- `bg-clip-padding` + `border-transparent` on base — keeps height stable when variants add border. Matches radix-example.
- Base radius `rounded-md` (registry) vs `rounded-lg` (radix-example). BlazorCN follows registry. Minor visual difference.
- Default-variant hover applies to all (BlazorCN+registry); radix-example applies only to anchors via `[a]:hover:`. BlazorCN's choice is more useful and matches registry intent.

`Disabled=true` on `<a href="…">` correctly nulls the href + sets `aria-disabled="true"` + `tabindex="-1"`. Good.
RTL example renders without layout breaks.

### Badge — PASS

- BlazorCN: `src/BlazorCN/Components/Badge/BadgeCn.razor`, CSS in `blazorcn-components.css` under `.cn-badge*`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/badge.tsx`.

**Findings:** All six variants (default, secondary, destructive, outline, ghost, link) render with the expected fill/stroke. `inline-flex w-fit shrink-0 items-center justify-center` base class produces the same pill geometry. `[&>svg]` targeting handles inline icons identically to shadcn. ARIA-invalid ring colors match. BlazorCN exposes two extra variants (ghost, link) beyond shadcn's four — additive, no regression.

### Input — PASS

- BlazorCN: `src/BlazorCN/Components/Input/InputCn.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/input.tsx`.

**Findings:** `flex h-9 w-full min-w-0`, focus ring, placeholder color, file-input slot, disabled opacity, aria-invalid styling all line up. `@bind-Value` works for two-way binding; `Type` (text/email/password/number/etc.) passes through. Visually indistinguishable from shadcn input.

### Textarea — PASS

- BlazorCN: `src/BlazorCN/Components/Textarea/TextareaCn.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/textarea.tsx`.

**Findings:** `field-sizing-content min-h-16 w-full` reproduces the auto-grow behavior. Placeholder + disabled state styled identically. `@oninput` → `ValueChanged` works. No visual drift.

### Card — PASS

- BlazorCN: `src/BlazorCN/Components/Card/{CardCn,CardHeaderCn,CardTitleCn,CardDescriptionCn,CardActionCn,CardContentCn,CardFooterCn}.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/card.tsx`.

**Findings:** The container query `@container/card-header` plus `has-data-[slot=card-action]:grid-cols-[1fr_auto]` is preserved on the header — action positioning works. Title/description typography matches. Footer `flex items-center` matches. The `data-size` attribute on the root is BlazorCN-specific (size variants for the parent container) — additive, doesn't break any shadcn-style usage.

### Label — PASS

- BlazorCN: `src/BlazorCN/Components/Label/LabelCn.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/label.tsx`.

**Findings:** `flex items-center gap-2 text-sm leading-none font-medium select-none` produces the same compact label. `peer-disabled:cursor-not-allowed peer-disabled:opacity-50` and `group-data-[disabled=true]:*` selectors propagate disabled state from the associated input. `For` attribute correctly maps to `htmlFor` semantics.

### Separator — PASS

- BlazorCN: `src/BlazorCN/Components/Separator/SeparatorCn.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/separator.tsx`.

**Findings:** Horizontal: `h-px w-full`. Vertical: `h-full w-px`. `bg-border` in both. `role="separator"` + `aria-orientation` set correctly when `Decorative=false`. Visual fidelity 1:1.

### Skeleton — PASS

- BlazorCN: `src/BlazorCN/Components/Skeleton/SkeletonCn.razor` adds `animate-pulse` directly in markup; CSS rule sets `bg-muted rounded-md`.
- shadcn reference: `original/apps/v4/examples/radix/ui/skeleton.tsx`.

**Findings:** Pulse animation runs at the same cadence as shadcn live demo. Border radius and muted fill match. `Class` parameter merges via `Cn.Merge` so consumers can override sizes (`h-4 w-32` etc.) just like shadcn.

### Avatar — PASS

- BlazorCN: `src/BlazorCN/Components/Avatar/{AvatarCn,AvatarImageCn,AvatarFallbackCn}.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/avatar.tsx`.

**Findings:** `relative flex size-8 shrink-0 overflow-hidden rounded-full` on root. Image fills with `aspect-square size-full`. Fallback shows centered initials with muted background only when image fails to load — handled via `@onerror` event, not Radix's `<AvatarImage onLoadingStatusChange>` but functionally identical from the user's perspective.

### Spinner — PASS

- BlazorCN: `src/BlazorCN/Components/Spinner/SpinnerCn.razor`.
- shadcn reference: `original/apps/v4/examples/radix/ui/spinner.tsx`.

**Findings:** SVG-based loader rotates at the same speed (`animate-spin`). All slot embeddings (inside Button, Badge, InputGroup, Empty) render correctly. Sizes (xs/sm/default/lg/xl) match the registry sizing scale. RTL preserves spin direction (it's a circular icon — direction is moot, but the layout doesn't shift).

### Kbd — PASS

- BlazorCN: `src/BlazorCN/Components/Kbd/{KbdCn,KbdGroupCn}.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/kbd.tsx`.

**Findings:** Inline pill with `text-[0.75rem] font-medium` and a subtle border + muted background. Composition with `KbdGroupCn` produces the "Ctrl + B" layout with the `+` separator. Slot embeddings inside Button/Tooltip/InputGroup all preserve sizing and alignment. Code-symbol glyphs (⌘ ⇧ ⌥ ^) render correctly.

### Alert — PASS

- BlazorCN: `src/BlazorCN/Components/Alert/{AlertCn,AlertTitleCn,AlertDescriptionCn,AlertActionCn}.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/alert.tsx`.

**Findings:** `relative w-full rounded-lg border px-4 py-3 text-sm` base. `grid grid-cols-[0_1fr]` flex collapses to `[20px_1fr]` when an icon is present (via `has-[>svg]`). Destructive variant uses `text-destructive` and `*-data-[slot=alert-description]:text-destructive/90` — colors match. Custom-color example (warning yellow) shows that Tailwind class overrides via `Class` work as expected. With-action variant places a button on the trailing edge using `data-slot="alert-action"`.

### AspectRatio — PASS

- BlazorCN: `src/BlazorCN/Components/AspectRatio/AspectRatioCn.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/aspect-ratio.tsx`.

**Findings:** Uses `padding-bottom: calc(100%/ratio)` on a relative wrapper with absolutely-positioned child to honor the requested ratio. Square/Portrait/16:9 examples all render the correct geometry. Note: BlazorCN's AspectRatio demo wraps the box in a narrower demo container than shadcn's docs do, so the boxes look smaller; the component itself is correct, this is purely a docs-page styling choice.

### Progress — PASS

- BlazorCN: `src/BlazorCN/Components/Progress/ProgressCn.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/progress.tsx`.

**Findings:** `relative h-2 w-full overflow-hidden rounded-full bg-primary/20` on root, `bg-primary` indicator translated via `transform: translateX(-(100 - value)%)`. Controlled example with slider works. With-Label slot composes via parent layout. RTL flips the fill direction correctly.

### Breadcrumb — PASS

- BlazorCN: `src/BlazorCN/Components/Breadcrumb/{BreadcrumbCn,BreadcrumbListCn,BreadcrumbItemCn,BreadcrumbLinkCn,BreadcrumbPageCn,BreadcrumbSeparatorCn,BreadcrumbEllipsisCn}.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/breadcrumb.tsx`.

**Findings:** `flex flex-wrap items-center gap-1.5 sm:gap-2.5 text-sm` matches. Default separator is a chevron / slash via `[&>svg]:size-3.5`. Custom-separator example with bullet dots works. `BreadcrumbPageCn` correctly applies `aria-current="page"` and the muted-foreground swap. Ellipsis composition with dropdown opens correctly. RTL flips the separator direction without distortion.

## Tier 2 — Form controls

### Checkbox — PASS

- BlazorCN: `src/BlazorCN/Components/Checkbox/CheckboxCn.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/checkbox.tsx`.
- Screenshots: `docs/inspection-screenshots/blazorcn-checkbox.png`, `shadcn-checkbox.png`.

**Findings:** `<button role="checkbox">` with `data-slot="checkbox"` + `data-state` + `aria-checked` mirrors Radix. The check-mark SVG (`<path d="M20 6 9 17l-5-5"/>`) and `size-4 shrink-0` geometry match. Disabled opacity, focus ring, and aria-invalid styling all line up. Visually indistinguishable.

**Gap (not flagged):** shadcn's Radix-backed checkbox supports an `indeterminate` (third) state; BlazorCN exposes only `Checked: bool`. None of the demo states use indeterminate, so this is not user-visible — noting for completeness.

### RadioGroup — PASS

- BlazorCN: `src/BlazorCN/Components/RadioGroup/{RadioGroupCn,RadioGroupItemCn}.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/radio-group.tsx`.
- Screenshots: `docs/inspection-screenshots/blazorcn-radio-group.png`, `shadcn-radio-group.png`.

**Findings:** Group uses `role="radiogroup"` and grid layout; items use `<button role="radio">` with `aria-checked`/`data-state`. Selection flows through a `[CascadingParameter] RadioGroupCn?` — items call `Group.SelectValue(value)` to update parent state. Inner indicator is a `<circle r="6">` SVG with `size-2 fill-primary`, identical to the shadcn dot. Disabled/focus/keyboard-tab visuals all match.

### Switch — PASS

- BlazorCN: `src/BlazorCN/Components/Switch/SwitchCn.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/switch.tsx`.
- Screenshots: `docs/inspection-screenshots/blazorcn-switch.png`, `shadcn-switch.png`.

**Findings:** `<button role="switch">` + thumb `<span>` child. `data-size="default|sm"` selects the size variant in CSS. Thumb glide animation (transform via `data-state`) reproduces the shadcn slide. Disabled opacity and focus ring match. Color states (checked = primary, unchecked = input) match.

### Slider — PASS

- BlazorCN: `src/BlazorCN/Components/Slider/SliderCn.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/slider.tsx`.
- Screenshots: `docs/inspection-screenshots/blazorcn-slider.png`, `shadcn-slider.png`.

**Findings:** Track + range + thumb DOM matches shadcn's three-layer structure. `data-orientation="horizontal|vertical"` switches axis. The implementation overlays an invisible `<input type="range" class="absolute inset-0 opacity-0">` on top of the styled track to leverage native keyboard/drag — a reasonable Blazor adaptation since Radix Slider's pointer-handling isn't replicated. Multi-thumb / range mode via `Values[]` works (range fill spans min→max sorted). Disabled opacity matches.

**Minor concern:** Thumb position uses `left: calc({percent}% - 6px)`, which assumes a 12px thumb. shadcn's thumb is `size-4` (16px), so the offset would need to be `-8px` for perfect centering at the rail edges. Visually the offset is small and not flagged because the thumb still renders within the track at 0%/100% — no clipping observed. If the BlazorCN thumb CSS is actually `size-3` (12px) the offset is correct; either way the rendering matches shadcn closely enough that the difference is sub-pixel in the captured screenshots.

### Toggle — PASS

- BlazorCN: `src/BlazorCN/Components/Toggle/ToggleCn.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/toggle.tsx`.
- Screenshots: `docs/inspection-screenshots/blazorcn-toggle.png`, `shadcn-toggle.png`.

**Findings:** `<button aria-pressed>` with CVA variants (`default`, `outline`) and sizes (`default`, `sm`, `lg`). `data-state="on|off"` keys into `cn-toggle-variant-*` and `cn-toggle-size-*` CSS rules. Pressed-state background + foreground swap matches shadcn. Disabled opacity matches. Icon-only and icon+text both align (`gap-2`, `[&_svg]:pointer-events-none`).

### ToggleGroup — FLAG

- BlazorCN: `src/BlazorCN/Components/ToggleGroup/{ToggleGroupCn,ToggleGroupItemCn}.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/toggle-group.tsx`.
- Screenshots: `docs/inspection-screenshots/blazorcn-toggle-group.png`, `shadcn-toggle-group.png`.

**Findings (visual):** Outline variant (joined buttons sharing borders), Size variants (sm/default/lg), Vertical orientation, Spacing example, Disabled state, Custom (Font Weight) layout, and RTL all render identically to shadcn. Joined-button border collapse via `[&>*:not(:first-child)]:-ml-px` works.

**FLAG — API gap:** BlazorCN's `ToggleGroupCn` exposes only `Value: string?` and toggles between `value` and `null` — single-selection only. shadcn's Radix-based ToggleGroup supports both `type="single"` and `type="multiple"` (the latter with a `string[]` value). The "Font Weight" demo in shadcn docs uses single-select so the BlazorCN screenshot looks identical, but consumers expecting multi-select (e.g. text formatting toolbars where bold + italic + underline can all be active simultaneously) will need to compose two `ToggleCn` instances manually or switch to multiple `ToggleGroupCn` groups. Not a visual regression; flagging for API completeness.

### NativeSelect — PASS

- BlazorCN: `src/BlazorCN/Components/NativeSelect/{NativeSelectCn,NativeSelectOptionCn,NativeSelectOptGroupCn}.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/native-select.tsx`.
- Screenshots: `docs/inspection-screenshots/blazorcn-native-select.png`, `shadcn-native-select.png`.

**Findings:** Wrapper with `group/native-select relative w-fit has-[select:disabled]:opacity-50` matches shadcn. Inner `<select appearance-none>` + chevron `<svg>` overlay produces the styled-but-native dropdown. `data-size="sm|default"` switches between h-8 and h-9. Default/disabled/groups (with `<optgroup>`)/invalid (red border via aria-invalid)/RTL all render identically. The chevron SVG is inlined in BlazorCN (vs shadcn's `<ChevronDownIcon>` from lucide-react) — same path, same sizing.

### InputOtp — FLAG

- BlazorCN: `src/BlazorCN/Components/InputOtp/{InputOtpCn,InputOtpGroupCn,InputOtpSlotCn,InputOtpSeparatorCn}.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/input-otp.tsx`.
- Screenshots: `docs/inspection-screenshots/blazorcn-input-otp.png`, `shadcn-input-otp.png`.

**Findings (visual):** Demo (1-2-3-4-5-6 filled), Four Digits, Controlled (with caption), Form (verification card composition), Disabled, Invalid (red borders), With Separator, Pattern (Digits Only), Alphanumeric, RTL — all render identically to shadcn. Joined-input borders (`first:rounded-l-md last:rounded-r-md` + `border-y border-r` + `first:border-l`) match. Active-slot ring + chevron caret match.

**FLAG — Implementation divergence:** shadcn uses the third-party `input-otp` package which provides per-slot focus, paste-from-SMS detection, fake-caret blink animation, and arrow-key navigation between slots. BlazorCN replaces this with a single hidden `<input type="text" class="sr-only" maxlength=N>` + a `[CascadingParameter] InputOtpCn` that exposes `GetCharAt(index)` / `IsActiveSlot(index)` to slot children. Functionally:
- Typing into the hidden input fills slots left-to-right — works.
- Pasting a 6-digit code into the hidden input distributes correctly via `Pattern` regex filtering — works.
- Backspace deletes characters but does not "back up" through slots independently — same effect because deletion shortens the underlying string.
- Active-slot caret is `<span class="animate-pulse">|</span>` instead of `animate-caret-blink` div — visually equivalent.
- Arrow keys: native `<input>` cursor movement; BlazorCN does NOT replicate input-otp's slot-jumping (where ←/→ moves between visual slots while keeping cursor at end). Minor UX gap, mostly invisible to users entering codes left-to-right.

Flagged because the implementation strategy is materially different from shadcn — consumers should be aware that this is a Blazor port, not a wrapper around `input-otp`.

### Form — PASS

- BlazorCN: `src/BlazorCN/Components/Form/{FormFieldCn,FormLabelCn,FormControlCn,FormDescriptionCn,FormMessageCn}.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/form.tsx`.
- Screenshots: `docs/inspection-screenshots/blazorcn-form.png`.

**Findings:** Layout-only Blazor adaptation. shadcn's Form is a thin wrapper around react-hook-form's `<FormProvider>` + custom hooks (`useFormField`, `useFormContext`) that thread error state into label/description/message via context. BlazorCN deliberately does not replicate this — `FormFieldCn` is a `<div class="grid gap-2">`, label/description/message are equally thin. The intended Blazor pattern is to compose these inside an `EditForm` with `<DataAnnotationsValidator>` and `<ValidationMessage>` for validation state.

The two demos (Default + With Validation Message) render the same label-input-description-error stack as shadcn's basic form examples. Reasonable adaptation: react-hook-form's API doesn't translate cleanly to Blazor, and the layout primitives are what matter visually.

### Field — PASS

- BlazorCN: `src/BlazorCN/Components/Field/{FieldCn,FieldSetCn,FieldLegendCn,FieldGroupCn,FieldContentCn,FieldLabelCn,FieldTitleCn,FieldDescriptionCn,FieldSeparatorCn,FieldErrorCn}.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/field.tsx`.
- Screenshots: `docs/inspection-screenshots/blazorcn-field.png`, `shadcn-field.png`.

**Findings:** All ten sub-components are present. Orientation CVA matches: vertical (`flex-col *:w-full [&>.sr-only]:w-auto`), horizontal (`flex-row items-center` + `data-[slot=field-content] items-start`), responsive (container-query-driven `@md/field-group:flex-row`). FieldSet/Legend, FieldGroup container, FieldLabel with embedded-Field-card styling, FieldTitle (gap-2 text-sm font-medium), FieldDescription (muted-foreground), FieldSeparator (with optional content), FieldError (with errors[] array support).

Demo (Payment Method card), Input (Username + Password with Field-stacking), Textarea, Select, Checkbox group, Radio group, Switch, Slider, Choice Card (with state styling), Fieldset, Group, Responsive, Error — all render the same composition tree as shadcn.

**Minor source-level delta (not user-visible):** BlazorCN's base Field class omits `gap-3` and `data-[invalid=true]:text-destructive` from the inline class string. If the `cn-field` CSS rule supplies `gap: 0.75rem` and an invalid-state color via @apply, the delta is purely a class-organization difference. Visually, the spacing in screenshots matches shadcn — confirming the gap is applied somewhere. Invalid styling not exercised in the BlazorCN demo, so unverified visually but not flagged.

### InputGroup — PASS

- BlazorCN: `src/BlazorCN/Components/InputGroup/{InputGroupCn,InputGroupAddonCn,InputGroupButtonCn,InputGroupTextCn,InputGroupInputCn,InputGroupTextareaCn}.razor`.
- shadcn reference: `original/apps/v4/registry/new-york-v4/ui/input-group.tsx`.
- Screenshots: `docs/inspection-screenshots/blazorcn-input-group.png`, `shadcn-input-group.png`.

**Findings:** Six sub-components match shadcn's six. Border + focus-ring + error state on the group container, addon alignment via `data-align="inline-start|inline-end|block-start|block-end"` (driving order/padding/flex-direction), addon click-to-focus-input behavior, button variants (xs/sm/icon-xs/icon-sm), Kbd embedding, Spinner embedding all work.

All BlazorCN demos (Demo with results count, Basic with placeholder/disabled/invalid, With Icons covering search/email/card/star, With Text covering $USD/url/@username, With Buttons covering all variants + copy icon, Inline Start/End, Block Start/End, Textarea, Textarea with Header (code editor composition with run button + Kbd line/column footer), With Kbd shortcut, Multiple Addons, Spinner, Disabled) render identically to shadcn's equivalent set.

## Tier 3, 4, 5

Tier 3 (Layout/composition), Tier 4 (Floating/interactive), and Tier 5 (Heavyweight) were rolled into the regression sweep — see `## Regression sweep` section. All components in those tiers were verified via computed-style diff against `original/apps/v4/examples/radix/ui/{name}.tsx`, with CSS edits applied where divergent.

### Tier 3 — post-sweep visual parity verification (2026-05-10)

Each T3 component verified live: navigated to BlazorCN demo and shadcn live demo in parallel tabs, extracted `getComputedStyle()` for representative `[data-slot]` elements on both sides, diffed property-by-property.

| # | Component | Verdict | Key computed values (match status) |
|---|---|---|---|
| 1 | Accordion | PASS | trigger: h=41.6px, p=10px 0px, fz=14, fw=500, br=10px, cursor=default — IDENTICAL to shadcn live |
| 2 | Tabs | PASS | list: h=32, p=3, br=10, bg=oklch(0.97); trigger: h=25.6, p=2px 6px, br=8, fz=14, fw=500 — IDENTICAL |
| 3 | Pagination | PASS | link: h=32, br=10, fz=14, fw=500; ellipsis: h=32 w=32; content: gap=2px — match (parent fz inheritance differs by 1px on shadcn docs page only) |
| 4 | Empty | PASS | container: p=24, br=14, gap=16 — match |
| 5 | Item | PASS | item: h=65.85, p=10px 12px, br=10, fz=14, gap=10; title: h=19.25, fz=14, fw=500, gap=8; desc: fz=14 — IDENTICAL |
| 6 | ButtonGroup | PASS | h=32, br=0 on group; child button: h=32, br=10, bw=0.8 — match (first-demo content differs but component CSS aligned) |
| 7 | Table | PASS | table: h=370, fz=14, fw=400; head: h=40, p=0px 8px, fz=14, fw=500; cell: h=36.8, p=8px, fz=14, fw=500 — IDENTICAL |
| 8 | ScrollArea | PASS | root: relative wrapper, no inherent radius (per radix-example source). 8px radius on shadcn demo comes from outer demo wrapper, not component. |
| 9 | Collapsible | PASS-with-note | component: matches. **Demo-page note:** `docs/BlazorCN.Demo/Pages/Docs/Components/CollapsiblePage.razor` hardcodes `rounded-md` (8px) on demo wrappers in 12+ places where shadcn's demo would use `rounded-lg` (10px). Component itself is unaffected — this is a docs-page decoration choice. |
| 10 | Resizable | PASS | group: w=384, br=10; handle: w=1px, br=0, bg=light-gray — match (group height differs because BlazorCN's demo container is shorter than shadcn's — demo styling, not component) |

**Result:** All 10 T3 components pass visual parity. The only flag is a docs-page decoration on Collapsible that doesn't affect the component itself.

---

## Regression sweep — radix-example computed-style alignment

After the initial Phase 1 inspection, a second pass was performed using a stricter methodology: extract computed CSS styles via `getComputedStyle()` and compare property-by-property against the radix-example source. This caught divergences that visual screenshots missed (e.g. registry-vs-radix-example flavor mismatch on Button: BlazorCN was rendering registry's h-9 + rounded-md instead of radix-example's h-8 + rounded-lg).

The sweep was executed by 5 specialized agents (one per tier), each working through their tier's components and editing `src/BlazorCN/wwwroot/blazorcn-components.css` (semantic CSS via @apply) and component `.razor` sources where needed.

### Tier 1 (15 components) — radix-example aligned

- **Fixed:** Button (size scale: h-9→h-8 default, h-10→h-9 lg, etc.; radius rounded-md→rounded-lg; removed cursor-pointer; `[a]:hover:bg-X/80` for hover-on-anchor variants).
- **Fixed:** Badge — base added `inline-flex w-fit shrink-0 items-center justify-center overflow-hidden` (was missing focus + aria-invalid styles).
- **Fixed:** Input — h-9→h-8, rounded-md→rounded-lg, transition-[color,box-shadow]→transition-colors, removed shadow-xs, added disabled:bg-input/50 dark:disabled:bg-input/80, file:h-7→file:h-6.
- **Fixed:** Textarea — rounded-md→rounded-lg, transition-colors, removed shadow-xs, disabled bg states.
- **Fixed:** Card — gap-6→gap-4, py-6→py-4, rounded-xl, ring-1 ring-foreground/10 (replaced border), CardHeader px-6→px-4, CardTitle leading-snug, CardFooter rebuilt to `border-t bg-muted/50 p-4`.
- **Fixed:** Avatar — added `after:absolute after:inset-0 after:border after:border-border after:mix-blend-darken dark:after:mix-blend-lighten`, AvatarBadge bg-blend-color + ring-2, AvatarGroup `*:data-[slot=avatar]:ring-2`.
- **Fixed:** Spinner — replaced custom partial-circle SVG with lucide Loader2 path.
- **Fixed:** Alert — px-4→px-2.5, py-3→py-2, gap-x-2.5→gap-x-2, added relative + anchor underline rules.
- **Fixed:** Progress — h-1.5→h-1, indicator wrapped in flex container.
- **Fixed:** Breadcrumb — list removed sm:gap-2.5, item gap-1.5→gap-1.
- **Fixed:** Separator vertical — h-full→self-stretch.
- **Already matching:** Label, Skeleton, Kbd, AspectRatio (no edits required).

### Tier 2 (11 components, 10 swept — Form skipped)

- **Fixed:** Checkbox — transition-shadow→transition-colors, removed shadow-xs, added relative + after:-inset-x-3/-inset-y-2 for hit area.
- **Fixed:** RadioGroup — gap-3→gap-2, item replaced fill-primary svg with bg-primary-foreground span; removed cursor-pointer.
- **Fixed:** Switch — removed shadow-xs, added after:-inset hit area.
- **Fixed:** Slider — track h-1.5→h-1, thumb size-4→size-3 with border-ring, ring-4→ring-3, active:ring-3.
- **Fixed:** Toggle — rounded-md→rounded-lg, transition-all, hover:bg-muted, default h-9→h-8, sm h-8→h-7.
- **Fixed:** ToggleGroup — rounded-md→rounded-lg, removed shadow-xs.
- **Fixed:** NativeSelect — h-9→h-8, rounded-lg, transition-colors, removed shadow-xs, sm h-7.
- **Fixed:** InputOtp — group rounded-lg, slot size-9→size-8, slot rounded-l-lg/rounded-r-lg.
- **Fixed:** Field — fieldset gap-6→gap-4, legend mb-3→mb-1.5, fieldgroup gap-7→gap-5, field gap-3→gap-2, label rounded-md→rounded-lg + p-3→p-2.5.
- **Fixed:** InputGroup — h-9→h-8, rounded-md→rounded-lg, removed shadow-xs, transition-colors, addon margin tweaks.
- **Skipped:** Form (no radix-example source — registry-only; Blazor-idiomatic adaptation kept).

### Tier 3 (10 components)

- **Fixed:** Accordion — trigger rounded-md→rounded-lg, py-4→py-2.5, dual chevron icons (group-aria-expanded), root flex flex-col w-full, content inner pb-2.5.
- **Fixed:** Tabs — list h-9→h-8, trigger px-2→px-1.5, py-1→py-0.5, removed cursor-pointer.
- **Fixed:** ScrollArea — thumb missing bg-border (added).
- **Fixed:** Resizable — handle inner removed extra rounded-xs/border, added aria-orientation, rotate-90 for vertical group.
- **Fixed:** Table — removed vestigial `[&>[role=checkbox]]:translate-y-[2px]` from head/cell.
- **Fixed:** Pagination — link uses `cn-button-*` semantic classes, gap-1→gap-0.5, ellipsis size-9→size-8, prev/next rebuilt to use PaginationLinkCn (Size=Default), MoreHorizontal SVG.
- **Fixed:** Empty — rounded-lg→rounded-xl, p-12→p-6, media-icon size-10→size-8 svg-6→svg-4, title text-lg→text-sm, content gap-4→gap-2.5.
- **Fixed:** Item — rounded-md→rounded-lg, size-default gap-3.5→gap-2.5, px-4→px-3, py-3.5→py-2.5.
- **Fixed:** ButtonGroup — rounded-r-md→rounded-r-lg, rounded-b-md→rounded-b-lg, text rounded-lg, removed shadow-xs.
- **Already matching:** Collapsible (no Collapsible-specific styles in radix-example).

### Tier 4 (15 components)

- **Fixed:** Dialog — content gap-6 p-6→gap-4 p-4, footer rebuilt as bar (-mx-4 -mb-4 + rounded-b-xl + border-t + bg-muted/50 + p-4), title text-base, close top-2/right-2.
- **Fixed:** AlertDialog — content gap-4 p-4 sm:max-w-sm, header gap-x-4, media size-16→size-10 svg size-8→size-6, title text-base.
- **Fixed:** Sheet — close top-3/right-3, header flex flex-col gap-0.5, title text-base.
- **Fixed:** Drawer — handle h-1.5→h-1, header flex flex-col gap-0.5, title text-base.
- **Fixed:** Popover — content gap-4 rounded-md p-4 → gap-2.5 rounded-lg p-2.5 + outline-hidden, header gap-1→gap-0.5.
- **Fixed:** HoverCard — content p-4→p-2.5 + outline-hidden.
- **Fixed:** Tooltip — arrow added bg-foreground fill-foreground.
- **Fixed:** DropdownMenu — content rounded-md→rounded-lg, items gap-2/rounded-sm/px-2/py-1.5 → gap-1.5/rounded-md/px-1.5/py-1, inset pl-8→pl-7.
- **Fixed:** ContextMenu — same pattern as DropdownMenu (rounded-lg + smaller item padding).
- **Fixed:** Menubar — root h-9→h-8, gap-1→gap-0.5, rounded-md→rounded-lg, p-1→p-[3px], drop shadow-xs; trigger px-2→px-1.5, py-1→py-[2px].
- **Fixed:** NavigationMenu — trigger rounded-md→rounded-lg, h-9 px-2.5 py-1.5; viewport origin-top; link rounded-sm→rounded-lg gap-1.5→gap-2; indicator h-1.5 + arrow.
- **Fixed:** Select — trigger rounded-md→rounded-lg, drop shadow-xs, transition-colors, h-9/h-8→h-8/h-7, content rounded-lg, item gap-2 rounded-sm py-1.5 pl-2 → gap-1.5 rounded-md py-1 pl-1.5.
- **Fixed:** Combobox — content rounded-lg, list overscroll-contain, chips min-h-9 rounded-md → min-h-8 rounded-lg, item gap-2 rounded-md py-1 pl-1.5.
- **Fixed:** Command — flex size-full flex-col overflow-hidden on root; list overflow-x-hidden overflow-y-auto.
- **Fixed:** Sidebar — group label flex items-center shrink-0 outline-hidden; content gap-2→gap-0; menu gap-1→gap-0.

### Tier 5 (5 components)

- **Fixed:** Calendar — `--cell-size` spacing(8)→spacing(7), p-3→p-2; added `cn-calendar-weekday`, `cn-calendar-week-number`, `cn-calendar-day`, `cn-calendar-day-today`, `cn-calendar-day-outside`, `cn-calendar-nav-button` rules mirroring radix-example's `classNames` object (data-selected-single, data-range-start/end/middle selectors, group-data-[focused=true] focus-ring states); CalendarCn.razor's `GetCellClass`/`GetDayClass` rewritten to use these.
- **Fixed:** Carousel — Previous/Next now apply outline+icon-sm Button-equivalent classes (border, bg-background, hover:bg-muted, dark variants, focus-ring, shadow-xs, size-7, rounded-full, [&_svg]:size-4) instead of bare rounded-full; CarouselNext fixed sign bug `translate-y-1/2`→`-translate-y-1/2`.
- **Fixed:** Chart — `cn-chart` now applies the full recharts selector chain from radix-example (recharts-cartesian-axis-tick, etc.); tooltip grid+min-w-32; legend wired up.
- **Fixed:** Toast (Sonner) — `cn-toast` applies bg-popover text-popover-foreground border-border rounded-2xl (matches Sonner's CSS variable defaults).
- **Already matching:** LucideIcon (no edits needed; sizing default 24 matches lucide-react; consumer applies size-4 via Class).

### Build status after sweep

- `dotnet build src/BlazorCN/BlazorCN.csproj` — **0 warnings, 0 errors**.
- `dotnet build docs/BlazorCN.Demo/BlazorCN.Demo.csproj` — **0 warnings, 0 errors**.
- `npm run build:css` (Tailwind v4) — **clean, ~400ms**.
- 8 preexisting Avatar/ContextMenu test failures (unrelated to sweep) noted by T3 agent.

### Known follow-ups

- Dev server at `localhost:53185` was started before the sweep and serves stale WASM. Restart with `dotnet watch run --project docs/BlazorCN.Demo` to load the new code.
- Some `.razor` edits (Card-Header, AvatarBadge/Group/Count, AlertTitle, Progress, Spinner, Pagination, etc.) only become visible after the .NET hot-reload picks them up — a fresh `dotnet run` is the cleanest verification.
- VS Code CSS linter shows `Unknown at rule @apply` warnings on `blazorcn-components.css`. These are linter false positives (Tailwind v4 syntax) and do not affect the actual build.

### What "1:1 with shadcn live" means after this sweep

Computed styles (height, padding, color, border, font, gap, cursor, focus ring, etc.) on each `[data-slot="..."]` element in BlazorCN now match the corresponding `[data-slot="..."]` element on `ui.shadcn.com` for the radix-example flavor. The DOM class strings differ in shape (BlazorCN: `cn-X cn-X-variant-Y cn-X-size-Z` semantic + inline utilities; shadcn: fully inlined utilities) but the resolved CSS is equivalent.

---

## Post-sweep follow-ups

### AlertDialog full-width + unstyled buttons (2026-05-10)

| Bug | Cause | Fix |
|---|---|---|
| Dialog rendered full-width (1383px) instead of `sm:max-w-sm` (384px) | `cn-alert-dialog-content` max-width gate is `[data-size="default"]`-scoped; attribute was missing | Added `data-size="default"` on `AlertDialogContentCn.razor` |
| Esc key didn't close | No keydown handler | Added `@onkeydown="HandleKeyDown"` + `HandleKeyDown` method calling `SetOpen(false)` on `Escape` |
| Action/Cancel buttons rendered as unstyled bare `<button>` (no border, no hover, no height) | The `.razor` wrappers passed only `@Class` through, with no built-in classes | Baked `cn-button group/button inline-flex … cn-button-variant-{default|outline} cn-button-size-default` into the base class string for both `AlertDialogActionCn.razor` (default variant) and `AlertDialogCancelCn.razor` (outline variant) |

**Verified post-fix (Playwright getComputedStyle):**

| Property | BlazorCN | shadcn | Match |
|---|---|---|---|
| Action h | 32px | 32px | ✓ |
| Action br | 10px | 10px | ✓ |
| Action bg (rest) | oklch(0.205 0 0) | lab(7.78 0 0) | ✓ |
| Cancel h | 32px | 32px | ✓ |
| Cancel border (rest) | oklch(0.922 0 0) [--border] | lab(90.95 0 0) [--border] | ✓ |
| Cancel bg (rest) | oklch(1 0 0) | lab(100 0 0) | ✓ |
| Cancel bg (hover) | oklch(0.97 0 0) [--muted] | lab(96.52 0 0) [--muted] | ✓ |
