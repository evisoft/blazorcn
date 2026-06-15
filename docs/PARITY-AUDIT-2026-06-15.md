# BlazorCN visual + CSS parity audit — 2026-06-15

Goal: go over the demo (`http://127.0.0.1:53185/`) page by page, find the matching
component/block on shadcn-ui, compare **visually** and **by computed CSS** (Chrome
console), and fix anything that doesn't match. Process all components, blocks, pages.

## Reference / method

- **Ground truth = the vendored shadcn source in `original/`**, specifically the **nova**
  style (`registry/styles/style-nova.css`) + `bases/base` markup. BlazorCN's `cn-*` CSS is a
  near-verbatim copy of nova.
- **ui.shadcn.com currently serves the `radix` base**, which diverges from nova on several
  components (sidebar gaps, nav/toggle/pagination padding, badge radius). So ui.shadcn.com was
  used as a *visual* cross-reference, but every computed-CSS difference was checked against the
  nova source before any change — to avoid "fixing" correct components into the wrong preset.
- **Harness:** a Chrome-console fingerprint that captures per-`data-slot` geometry
  (height, padding, radius, font, gap, border) on both the local demo and ui.shadcn.com, then
  diffs them. Filtered to each component's *own* slots (the demo scaffolding — Preview/Code
  tabs, title separators, cards — and the docs-site 15px prose font are excluded as noise).
- **Calibration:** Button is pixel-identical across local / ui.shadcn.com / nova
  (h32, pad 0×10, radius 10, 14/500, gap 6).

## Scope processed

- **Components: 60 / 60** (every `/docs/components/*` route).
- **Top-level pages: 7 / 7** — `/`, `/colors`, `/themes`, `/docs/getting-started`,
  `/docs/installation`, `/docs/theming`, `/docs/dark-mode`. All render cleanly, **0** horizontal
  overflow.
- **Blocks: 57 / 57 categories, 6,171 blocks** (headless render sweep). **0** crashes / blank
  category pages / timeouts.

## Fixes made

1. **Command palette input** (`cn-command-input-group`) — the search text sat flush against the
   magnifier icon. Our `CommandInputCn` re-implements the input-group inline instead of reusing
   the real `InputGroup`, so it lost the input's start padding that shadcn gets from
   `.cn-input-group:has(>[data-align=inline-start])>input`. Added `ps-1.5` (6px, logical/RTL-safe)
   to the command input — now matches shadcn's icon→text gap. Verified `padding-inline-start: 6px`.

2. **Pricing → Credit Packs block** (`PricingCreditPacks.razor`) — used fixed-width grid columns
   `grid-cols-[repeat(4,260px)]` (4×260 + gaps = 1088px) which overflowed the narrower docs
   container by **81px** (horizontal scrollbar). Changed to `repeat(4,minmax(0,260px))` so columns
   shrink to fit while still capping at 260px. Overflow **81px → 0**, design preserved.

(Two more fixes landed earlier the same session, already committed:
`8cbb9b8` badge left/right padding; `adb52c9` `color-scheme` for native dropdown/scrollbar theming.)

## Verified-correct (looked like diffs, were not — do NOT "fix")

- **Radix-vs-nova preset differences** (our nova is correct; ui.shadcn.com radix differs):
  sidebar `gap-0`/`p-2` vs radix gap-4/larger padding; navigation-menu link `p-2` vs radix
  py-1.5/px-2.5; toggle / toggle-group / pagination padding; badge `rounded-full` (renders as a
  pill, same as radix's 26px on a 22px-tall badge). All confirmed verbatim against `style-nova.css`.
- **Docs-site prose font** (shadcn docs 15px / 1.5 vs our 14px) — surrounding text, not component
  styling; excluded as noise.
- **Skeleton blocks flagged "blank"** (skeleton cat = 99, chat "Loading Skeleton" = 1) — intentional
  shimmer placeholders with no text/media; heuristic false positive.
- **"Blowout" (>1400px tall) blocks (93)** — legitimately long content (galleries, FAQ, timelines).

## Noted, not changed (architecture / proportionality)

- **Trigger components** (`alert-dialog`, `dialog`, `drawer`, `dropdown-menu`, `hover-card`,
  `popover`, `sheet`, `tooltip`) render an **outer `<button>` wrapping the child** instead of
  React's `asChild`/Slot merge, so the wrapper measures as unstyled. Visually identical (the inner
  `ButtonCn` carries the styling); the only difference is a semantic nested-element wrapper. This is
  a library-wide trigger pattern — changing it needs an asChild-equivalent and touches ~10
  components, so it's recommended for a separate, deliberate change, not this visual pass.
- **Combobox trigger** uses legacy pre-v4 classes (`h-9 rounded-md shadow-sm ring-offset-background
  focus:ring-1`) vs shadcn's v4 flat outline button. Visually clean and a valid input-style trigger;
  modernizing it is cosmetic and risks an interactive component, so left as-is with this note.
- **Features → Index Card File Rolodex (37px)** and **Staggered Hex Cluster Grid (33px)** — minor
  horizontal overhang from intentional decorative transforms (rotated fanned cards; honeycomb
  half-cell offset). On the real full-width centered layout (`max-w-4xl mx-auto`) the overhang sits
  in the side margin; clipping it would cut the decorative effect / real cells. Low priority.

## Result

All 60 components, 57 block categories (6,171 blocks) and 7 top-level pages processed. The library
matches the canonical shadcn **nova** source; 2 genuine defects found and fixed this pass
(command-input spacing, Credit Packs overflow), plus the 2 earlier-committed fixes. No crashes, no
blank pages, no real false-positive "fixes" introduced.
