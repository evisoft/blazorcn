# Forms & Inputs

## Contents

- Form layout uses `FieldCn` / `FieldGroupCn`
- Binding controls (`@bind-Value` vs `@bind-Checked`) — value-type table
- Validation & disabled states (manual in BlazorCN)
- `EditForm` / DataAnnotations integration
- The `Form*Cn` family (presentational wrappers)
- `FieldSetCn` + `FieldLegendCn` for grouped choices
- Option sets (2–7 choices) use `ToggleGroupCn`
- Choosing a control

---

## Form layout uses `FieldCn` / `FieldGroupCn`

Use `FieldGroupCn` (stacks fields) + `FieldCn` (one field) instead of a raw `div`
with `space-y-*`:

```razor
<FieldGroupCn>
  <FieldCn>
    <FieldLabelCn For="email">Email</FieldLabelCn>
    <InputCn id="email" @bind-Value="_email" />
    <FieldDescriptionCn>We'll never share it.</FieldDescriptionCn>
  </FieldCn>
  <FieldCn>
    <FieldLabelCn For="password">Password</FieldLabelCn>
    <InputCn id="password" type="password" @bind-Value="_password" />
  </FieldCn>
</FieldGroupCn>
```

`FieldCn` takes an `Orientation` (`FieldOrientation.Vertical` default,
`Horizontal` for settings rows, `Responsive`). Field parts:
`FieldLabelCn` (`For`), `FieldTitleCn`, `FieldDescriptionCn`, `FieldErrorCn`,
`FieldContentCn`, `FieldSeparatorCn`, `FieldSetCn`, `FieldLegendCn`.

---

## Binding controls — `@bind-Value` vs `@bind-Checked`

The bindable parameter and its **type** differ by control. Getting these wrong is
the most common mistake:

| Control | Bind | Value type |
| --- | --- | --- |
| `InputCn` | `@bind-Value` | `string?` |
| `TextareaCn` | `@bind-Value` | `string?` |
| `SelectCn` | `@bind-Value` | `string?` |
| `NativeSelectCn` | `@bind-Value` | `string?` |
| `ComboboxCn` | `@bind-Value` | `string?` |
| `RadioGroupCn` | `@bind-Value` | `string?` |
| `ToggleGroupCn` | `@bind-Value` | `string?` |
| `SliderCn` | `@bind-Value` | `double` |
| `CheckboxCn` | **`@bind-Checked`** | `bool` |
| `SwitchCn` | **`@bind-Checked`** | `bool` |
| `TabsCn` | `@bind-Value` | `string?` |

```razor
<InputCn @bind-Value="_name" />
<SliderCn @bind-Value="_volume" />        @* double *@
<CheckboxCn @bind-Checked="_agree" />      @* NOT @bind-Value *@
<SwitchCn @bind-Checked="_notifications" />
```

> `CheckboxCn` and `SwitchCn` model an on/off state, so their parameter is
> `Checked`/`CheckedChanged`, not `Value`. Using `@bind-Value` on them won't
> compile.

---

## Validation & disabled states (manual)

BlazorCN does **not** auto-derive validity — *you* set the attributes from your
validation state (this differs from the impression shadcn's docs give):

- **`aria-invalid="true"` on the control** triggers the invalid styling (the
  `cn-input` CSS reacts to it). Pass it as a plain attribute.
- **`data-invalid` on `FieldCn`** styles the field's label/description as invalid.
  The component does **not** set it automatically — the consumer sets it.
- **Disabled:** `Disabled="true"` on the control; optionally `data-disabled` on the
  `FieldCn` to dim the label.

```razor
<FieldCn data-invalid="@(_emailError is not null)">
  <FieldLabelCn For="email">Email</FieldLabelCn>
  <InputCn id="email" @bind-Value="_email" aria-invalid="@(_emailError is not null)" />
  @if (_emailError is not null)
  {
    <FieldErrorCn Errors="@(new[] { _emailError })" />
  }
</FieldCn>
```

`FieldErrorCn` accepts either `ChildContent` or an `Errors`
(`IReadOnlyList<string>?`) collection (it de-dupes and renders a single message or
a bulleted list).

---

## `EditForm` / DataAnnotations integration

Control components are thin wrappers over native inputs — they are **not**
`InputBase`-derived and do **not** auto-register with an `EditContext`. To use
`EditForm` + DataAnnotations, bind each control to your model with `@bind-Value`
and drive messages/validity from validation state:

```razor
<EditForm Model="_model" OnValidSubmit="Submit">
  <DataAnnotationsValidator />
  <FieldGroupCn>
    <FieldCn>
      <FieldLabelCn For="email">Email</FieldLabelCn>
      <InputCn id="email" @bind-Value="_model.Email" />
      <ValidationMessage For="@(() => _model.Email)" />
    </FieldCn>
  </FieldGroupCn>
  <ButtonCn Type="submit">Save</ButtonCn>
</EditForm>
```

> `ButtonCn Type="submit"` is required to submit an `EditForm` (default `Type` is
> `"button"`). Standard `<ValidationMessage>` works; or use `FormMessageCn` for a
> styled message you control.

---

## The `Form*Cn` family (presentational wrappers)

`FormFieldCn`, `FormLabelCn`, `FormControlCn`, `FormDescriptionCn`,
`FormMessageCn` mirror shadcn's `FormItem`/`FormLabel`/`FormControl`/etc. They are
**layout/markup only** — no binding or validation logic (`FormFieldCn` is a
`grid gap-2` wrapper; `FormMessageCn` is a `text-destructive` `<p>`). Use them for
structure; do the binding/validation with `@bind-Value` + EditForm as above.

```razor
<FormFieldCn>
  <FormLabelCn For="username">Username</FormLabelCn>
  <FormControlCn>
    <InputCn id="username" @bind-Value="_model.Username" />
  </FormControlCn>
  <FormDescriptionCn>Your public display name.</FormDescriptionCn>
  <FormMessageCn>@_usernameError</FormMessageCn>
</FormFieldCn>
```

> `FieldCn`/`FieldGroupCn` and `FormFieldCn` overlap. Prefer the **Field** family
> for general layout; reach for the **Form** family when you want the
> shadcn `FormItem`-style structure inside an `EditForm`.

---

## `FieldSetCn` + `FieldLegendCn` for grouped choices

Group related checkboxes/radios/switches — not a `div` + heading:

```razor
<FieldSetCn>
  <FieldLegendCn Variant="FieldLegendVariant.Label">Preferences</FieldLegendCn>
  <FieldDescriptionCn>Select all that apply.</FieldDescriptionCn>
  <FieldGroupCn Class="gap-3">
    <FieldCn Orientation="FieldOrientation.Horizontal">
      <CheckboxCn id="dark" @bind-Checked="_dark" />
      <FieldLabelCn For="dark" Class="font-normal">Dark mode</FieldLabelCn>
    </FieldCn>
  </FieldGroupCn>
</FieldSetCn>
```

`FieldLegendCn` takes `Variant` (`FieldLegendVariant.Legend` | `Label`).

---

## Option sets (2–7 choices) use `ToggleGroupCn`

Don't loop `ButtonCn` with manual active state for a small choice set:

```razor
<ToggleGroupCn @bind-Value="_frequency">
  <ToggleGroupItemCn Value="daily">Daily</ToggleGroupItemCn>
  <ToggleGroupItemCn Value="weekly">Weekly</ToggleGroupItemCn>
  <ToggleGroupItemCn Value="monthly">Monthly</ToggleGroupItemCn>
</ToggleGroupCn>
```

---

## Choosing a control

- Free text → `InputCn`; multi-line → `TextareaCn`
- Dropdown of predefined options → `SelectCn`
- Searchable dropdown / autocomplete → `ComboboxCn`
- Native `<select>` (no JS, SSR-friendly) → `NativeSelectCn`
- Single choice from a few visible options → `RadioGroupCn`
- Toggle between 2–7 options inline → `ToggleGroupCn`
- Boolean: settings toggle → `SwitchCn`; form opt-in → `CheckboxCn`
- Numeric range → `SliderCn`
- OTP / verification code → `InputOtpCn`
- Input with leading/trailing addons or an inline button → `InputGroupCn`
  (`InputGroupInputCn` / `InputGroupTextareaCn` / `InputGroupAddonCn`)
