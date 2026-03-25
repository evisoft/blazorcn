# BlazorCN Demo App — Design Document

**Date:** 2026-03-22
**Status:** Approved

## Overview

A Blazor WebAssembly standalone demo app that serves as both documentation and interactive showcase for the BlazorCN component library. Modeled after shadcn-ui's v4 site, it includes component docs with live previews, full-page example applications, an interactive theme customizer, and a color palette viewer.

**Location:** `docs/BlazorCN.Demo/`
**Technology:** Blazor WASM standalone, .NET 10
**Reference:** BlazorCN library via project reference

## Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Hosting model | Blazor WASM standalone | Simple deployment (static hosting), no server needed, proves WASM compatibility |
| Doc pages | Razor files with live components + code tabs | Live demos are the value; no markdown build pipeline needed |
| Layout | Match v4 closely | Sticky header, docs sidebar, familiar shadcn layout builds credibility |
| Examples | Dashboard, Tasks, Authentication | Covers widest range of components without niche demos |
| Themes page | Yes | High-impact showcase of CSS variable theming |
| Colors page | Yes | Low effort, useful reference |
| Charts section | No | Library has ChartContainerCn but no full chart section needed |

## Project Structure

```
docs/BlazorCN.Demo/
├── BlazorCN.Demo.csproj
├── Program.cs
├── wwwroot/
│   ├── index.html
│   ├── css/
│   └── js/                          # Prism.js for code highlighting
├── Layout/
│   ├── MainLayout.razor              # Header + content + footer
│   ├── DocsLayout.razor              # Sidebar + content
│   ├── ExampleLayout.razor           # Full-page layout for examples
│   ├── SiteHeader.razor              # Sticky header (nav, search, theme toggle)
│   ├── SiteFooter.razor              # Footer
│   ├── Sidebar.razor                 # Docs sidebar navigation
│   └── MobileNav.razor               # Sheet-based mobile nav
├── Pages/
│   ├── Index.razor                   # Landing page
│   ├── Docs/
│   │   ├── GettingStartedPage.razor
│   │   ├── InstallationPage.razor
│   │   ├── ThemingPage.razor
│   │   ├── DarkModePage.razor
│   │   └── Components/
│   │       ├── ButtonPage.razor      # One per component group (48 total)
│   │       ├── CardPage.razor
│   │       └── ...
│   ├── Examples/
│   │   ├── DashboardPage.razor
│   │   ├── TasksPage.razor
│   │   └── AuthenticationPage.razor
│   ├── ThemesPage.razor
│   └── ColorsPage.razor
├── Components/
│   ├── ComponentPreview.razor        # Live preview + code tab wrapper
│   ├── CodeBlock.razor               # Syntax-highlighted code with copy button
│   ├── CommandMenu.razor             # Ctrl+K search dialog
│   └── ThemeCustomizer.razor         # CSS variable editor
├── Data/
│   └── MockData.cs                   # Static mock data for examples
└── _Imports.razor
```

## Layout & Navigation

### SiteHeader
- Logo/brand → home
- Top nav tabs: Docs, Components, Examples, Themes, Colors
- Command menu trigger (Ctrl+K) via CommandCn/DialogCn
- Theme toggle (light/dark) via ButtonCn + JS interop for `.dark` class
- GitHub link
- Mobile: hamburger → SheetCn with full nav
- All built with BlazorCN components (dogfooding)

### Docs Sidebar
- Getting Started section (Introduction, Installation, Theming, Dark Mode)
- Components section — alphabetical list of all 48 groups
- Active item highlighted, scrollable via ScrollAreaCn
- Hidden on mobile, toggled via button

### Routing

| Route | Layout | Page |
|-------|--------|------|
| `/` | MainLayout | Landing page |
| `/docs/getting-started` | DocsLayout | Introduction |
| `/docs/installation` | DocsLayout | Installation guide |
| `/docs/theming` | DocsLayout | Theming guide |
| `/docs/dark-mode` | DocsLayout | Dark mode guide |
| `/docs/components/{name}` | DocsLayout | Component doc page |
| `/examples/dashboard` | ExampleLayout | Dashboard example |
| `/examples/tasks` | ExampleLayout | Tasks example |
| `/examples/authentication` | ExampleLayout | Auth example |
| `/themes` | MainLayout | Theme customizer |
| `/colors` | MainLayout | Color palette |

## Component Documentation Pages

48 pages, one per component group. Each follows the same template:

1. Title + one-line description
2. One or more ComponentPreview sections with:
   - **Preview tab** — live rendered BlazorCN components
   - **Code tab** — syntax-highlighted Razor markup
3. Sections grouped by feature (Variants, Sizes, With Icon, etc.)

### ComponentPreview.razor
- TabsCn for Preview/Code toggle
- Preview renders a RenderFragment (live components)
- Code displays a string parameter via CodeBlock
- Bordered card with padding matching v4 look

### CodeBlock.razor
- `<pre><code>` with Prism.js syntax highlighting
- Copy-to-clipboard button (top-right)
- Language indicator (razor/html/css)

## Example Pages

### Dashboard (`/examples/dashboard`)
- Sidebar nav using SidebarCn
- Top bar: InputCn search + AvatarCn user menu via DropdownMenuCn
- 4 stat cards (CardCn) — revenue, subscriptions, sales, active
- Area chart section (ChartContainerCn or SVG placeholder)
- Recent sales list with AvatarCn + names + amounts
- Mock data in static class

### Tasks (`/examples/tasks`)
- Header with title + description
- Filter bar: InputCn search + DropdownMenuCn for status/priority
- Data table: TableCn with columns — ID, title, status (BadgeCn), priority (BadgeCn), actions (DropdownMenuCn)
- Pagination with PaginationCn
- ~20 mock tasks

### Authentication (`/examples/authentication`)
- Split layout: left image/branding, right form
- Login: InputCn email + password, ButtonCn submit, CheckboxCn remember me
- Social login buttons (GitHub, Google)
- Tab switch to signup form
- CardCn, LabelCn, TabsCn for form container
- Purely visual, no real auth

All examples use ExampleLayout (no docs sidebar, minimal header with back link).

## Themes Page (`/themes`)

- Left panel: ThemeCustomizer controls
  - Base color picker (zinc, slate, stone, gray, neutral) — radio swatches
  - Border radius slider (SliderCn) — 0 to 1rem
  - Mode toggle (light/dark)
- Right panel: Live preview (CardsDemo composite)
  - CardCn with form inputs
  - CardCn with stats
  - Buttons in various variants
  - Mini data table, badges, avatars, toggles
- Updates CSS variables on `<html>` in real-time via JS interop

## Colors Page (`/colors`)

- Grid of all CSS variable colors from BlazorCN theme
- Each: swatch + variable name + HSL value
- Light and dark mode columns side by side

## Landing Page (`/`)

- Hero: "BlazorCN" title + tagline + CTA buttons (Get Started, GitHub)
- Component showcase strip: highlighted component previews
- Feature highlights: 3-4 cards (48 Components, Tailwind CSS, Dark Mode, Accessible)
- Footer with links

## Dependencies

- `BlazorCN` (project reference)
- Tailwind CSS (for the demo app's own styling)
- Prism.js (syntax highlighting via JS interop)
- No other heavy dependencies

## Non-Goals

- No MDX/Markdown build pipeline
- No charts section
- No RTL or Playground examples
- No real authentication
- No server-side rendering
- No v0/AI integration
