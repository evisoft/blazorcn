# BlazorCN Setup & Project Configuration

BlazorCN has **no CLI and no registry**. Unlike shadcn/ui (which copies component
source into your project), BlazorCN ships every component in **one NuGet package**
that you reference. "Adding a component" = just using `<SomethingCn />` — there is
nothing to install per component, no `components.json`, no `add` command.

Setup is four things: **(1)** reference the package, **(2)** register services,
**(3)** load the theme CSS, **(4)** configure Tailwind to see the classes.

> Working reference: `docs/BlazorCN.Demo` is a complete, working consumer app.
> When in doubt about wiring, read its `Program.cs`, `_Imports.razor`, `App.razor`,
> and `Styles/app.css`. **Caveat:** the demo lives *inside* this repo, so its CSS
> uses relative paths (`../../../src/BlazorCN/...`). A real consumer uses the
> published static-web-asset paths (`_content/BlazorCN/...`) shown below.

---

## 1. Reference the package

```bash
dotnet add package BlazorCN
```

Requirements: **.NET 10**. Works across all Blazor render modes — Server,
WebAssembly, Auto, and Static SSR. The package is trimmable (`IsTrimmable`,
`TrimMode=link`); see [AOT & trimming](#aot--trimming).

## 2. Register services

```csharp
// Program.cs
using BlazorCN;

builder.Services.AddBlazorCN();
```

`AddBlazorCN()` registers exactly two **scoped** services (idempotent via
`TryAddScoped`):

- `JsInteropCn` — typed wrapper over `_content/BlazorCN/blazorcn.js` (Floating-UI
  positioning, focus trap, scroll lock). The JS module is imported **lazily** on
  first use — no `<script>` tag required.
- `ToastService` — inject it to raise toasts (`ToastService.Success(...)`); pair
  with a single `<ToasterCn />` in your layout.

## 3. Load the theme CSS

Add the CSS-variable theme to your host page (`index.html` for WASM,
`App.razor`/`_Host.cshtml` for Server), **before** your compiled Tailwind output:

```html
<link rel="stylesheet" href="_content/BlazorCN/blazorcn.css" />
<link rel="stylesheet" href="app.css" /> <!-- your compiled Tailwind output -->
```

`blazorcn.css` defines the `:root` / `.dark` CSS variables and sets `color-scheme`
so native controls follow the theme. It also `@import`s `blazorcn-components.css`
(the `cn-*` component classes).

## 4. Configure Tailwind (v4, CSS-first)

BlazorCN emits Tailwind utility classes, so the consumer app must run Tailwind.
Create a source CSS (e.g. `Styles/app.css`):

```css
@import "tailwindcss";
@import "tw-animate-css";

/* The cn-* component classes (use @apply, so Tailwind must process them). */
@import "../_content/BlazorCN/blazorcn-components.css";

/* Tailwind only generates utilities it can SEE in scanned source. It scans your
   app automatically, but NOT the BlazorCN .razor sources inside the NuGet
   package — so utilities only the library emits get purged unless you point
   @source at the package's component sources you reference. */
@source "../Components/**/*.razor";   /* your own components */
@source "../../**/_content/BlazorCN/**/*";  /* BlazorCN razor/runtime assets, if exposed */

@custom-variant dark (&:is(.dark *));

/* Map the shadcn CSS variables to Tailwind tokens. */
@theme inline {
  --color-background: var(--background);
  --color-foreground: var(--foreground);
  --color-primary: var(--primary);
  --color-primary-foreground: var(--primary-foreground);
  /* …repeat for secondary, muted, accent, destructive, border, input, ring,
     card, popover, chart-1..5, sidebar-*. Full list in
     docs/BlazorCN.Demo/Styles/app.css. */
}

/* Component data-attribute variants (data-state=open, data-checked, etc.). */
@custom-variant data-open   { &:where([data-state="open"]),   &:where([data-open]:not([data-open="false"]))     { @slot; } }
@custom-variant data-closed { &:where([data-state="closed"]), &:where([data-closed]:not([data-closed="false"])) { @slot; } }
/* …data-checked, data-unchecked, data-selected, data-disabled, data-active,
   data-horizontal, data-vertical — see the demo app.css for the full set. */

@layer base {
  * { @apply border-border outline-ring/50; }
  body { @apply bg-background text-foreground; }
}
```

> **The `@source` gotcha is the #1 setup failure.** If components render with no
> styling (or specific things like the accordion chevron flip / a variant color
> are missing), Tailwind purged classes it never saw. Fix it by adding a
> `@source` line covering the BlazorCN `.razor` sources you use. The demo points
> `@source` at the library's `src/BlazorCN/Components/**/*.razor` (and `*.cs`)
> because it builds from source; a NuGet consumer points it at the static assets
> exposed under `_content/BlazorCN/`.

Build the CSS:

```bash
npx @tailwindcss/cli -i ./Styles/app.css -o ./wwwroot/app.css --minify
# during development, add --watch
```

> **JS-config (Tailwind v3-style) alternative:** a ready-made preset ships at
> `_content/BlazorCN/tailwind-preset.js`. Reference it from a `tailwind.config.js`
> `presets: [...]`. Prefer the v4 CSS-first setup above.

## 5. Import the namespace

BlazorCN uses one flat namespace:

```razor
@* _Imports.razor *@
@using BlazorCN
```

---

## Verifying the setup

Before building UI, confirm the wiring (there is no `info` command — check files):

1. **Package referenced?** `dotnet list package | grep -i BlazorCN`.
2. **Services registered?** `AddBlazorCN()` present in `Program.cs`.
3. **Theme CSS linked?** `_content/BlazorCN/blazorcn.css` in the host page.
4. **Tailwind seeing classes?** A `@source` line covering the BlazorCN `.razor`
   you use, and the `@theme inline` token map present. If a `<ButtonCn>` renders
   unstyled, this is almost always the cause.
5. **Namespace imported?** `@using BlazorCN` in `_Imports.razor`.
6. **Toaster mounted?** One `<ToasterCn />` in the layout if you use toasts.

---

## Render modes

Works under **Server, WebAssembly, Auto, and Static SSR**. JS interop (Floating-UI
positioning, focus trap, scroll lock) loads lazily and degrades gracefully during
prerender — interactive components (Dialog, Select, Popover, DropdownMenu, …) need
an interactive render mode to position/animate; under Static SSR they render but
their JS-driven behavior is inert until hydrated.

## AOT & trimming

The package is trimmable. Two rules for Native-AOT / aggressively-trimmed WASM:

1. **Icons:** prefer concrete `Lucide{Name}Cn` components over the reflection-based
   `<LucideIconCn Name="..." />` dispatcher, so the icon type is statically
   referenced and not trimmed. See [rules/icons.md](./rules/icons.md).
2. **JS-interop option objects** must be **plain classes** with a parameterless
   constructor and `[JsonPropertyName]` init-only properties — **never** records
   or anonymous types. Trimmed builds strip constructor parameter names, and
   `System.Text.Json` then throws `ConstructorContainsNullParameterNames`, which
   the floating components' `try/catch` swallows — so the symptom is a
   Select/Popover/DropdownMenu that opens at the off-screen sentinel
   (`top:-9999px; left:-9999px`) and never moves. The library's
   `FloatingJsOptions` / `KeyboardNavJsOptions` are already correct; if you add
   your own interop payloads, follow the same shape.

> `TrimMode=full` is known to break the app (constructor-not-located at runtime).
> Stay on `TrimMode=link`.

---

## What this skill replaces from the shadcn CLI

| shadcn/ui (React) | BlazorCN (Blazor) |
| --- | --- |
| `npx shadcn add button` (copies source) | nothing — just use `<ButtonCn>` (it's in the package) |
| `components.json` project context | `AddBlazorCN()` + CSS link + Tailwind `@source`/`@theme` |
| `npx shadcn docs <component>` | read `src/BlazorCN/Components/<Group>/*.razor` or the demo at `/docs/components/{name}` |
| registry / MCP / presets | one NuGet package; theming via CSS variables (see [customization.md](./customization.md)) |
| `base` vs `radix` API split | single API — see [rules/base-vs-radix equivalents in composition.md](./rules/composition.md) |
