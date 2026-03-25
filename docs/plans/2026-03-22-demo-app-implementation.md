# BlazorCN Demo App — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a Blazor WASM standalone demo app at `docs/BlazorCN.Demo/` that documents and showcases all 48 BlazorCN component groups with live previews, 3 example apps, an interactive theme customizer, and a color palette viewer.

**Architecture:** Blazor WASM standalone app referencing BlazorCN via project reference. Razor pages for docs (live components + code tabs). Tailwind CSS via CDN for demo app styling. Prism.js via CDN for syntax highlighting. No markdown pipeline — all content is in .razor files.

**Tech Stack:** .NET 10, Blazor WebAssembly standalone, BlazorCN (project ref), Tailwind CSS v4 (CDN), Prism.js (CDN)

**Important Note:** BlazorCN does NOT have a SidebarCn component. The Dashboard example must use a custom sidebar built from existing components (nav, links, Collapsible, ScrollArea, etc.).

---

## Phase 1: Project Scaffolding & App Shell

### Task 1: Create the WASM project

**Files:**
- Create: `docs/BlazorCN.Demo/BlazorCN.Demo.csproj`
- Create: `docs/BlazorCN.Demo/Program.cs`
- Create: `docs/BlazorCN.Demo/wwwroot/index.html`
- Create: `docs/BlazorCN.Demo/_Imports.razor`
- Modify: `BlazorCN.slnx`

**Step 1: Create .csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>BlazorCN.Demo</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\BlazorCN\BlazorCN.csproj" />
  </ItemGroup>

</Project>
```

**Step 2: Create Program.cs**

```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorCN;
using BlazorCN.Demo;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<JsInteropCn>();

await builder.Build().RunAsync();
```

**Step 3: Create wwwroot/index.html**

```html
<!DOCTYPE html>
<html lang="en" class="scroll-smooth">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>BlazorCN — Blazor Components</title>
    <base href="/" />

    <!-- BlazorCN CSS (theme variables) -->
    <link rel="stylesheet" href="_content/BlazorCN/blazorcn.css" />

    <!-- Tailwind CSS v4 CDN -->
    <script src="https://cdn.tailwindcss.com"></script>
    <script>
        tailwind.config = {
            darkMode: 'class',
            theme: {
                extend: {
                    colors: {
                        background: 'var(--background)',
                        foreground: 'var(--foreground)',
                        card: { DEFAULT: 'var(--card)', foreground: 'var(--card-foreground)' },
                        popover: { DEFAULT: 'var(--popover)', foreground: 'var(--popover-foreground)' },
                        primary: { DEFAULT: 'var(--primary)', foreground: 'var(--primary-foreground)' },
                        secondary: { DEFAULT: 'var(--secondary)', foreground: 'var(--secondary-foreground)' },
                        muted: { DEFAULT: 'var(--muted)', foreground: 'var(--muted-foreground)' },
                        accent: { DEFAULT: 'var(--accent)', foreground: 'var(--accent-foreground)' },
                        destructive: { DEFAULT: 'var(--destructive)', foreground: 'var(--destructive-foreground)' },
                        border: 'var(--border)',
                        input: 'var(--input)',
                        ring: 'var(--ring)',
                        chart: {
                            1: 'var(--chart-1)', 2: 'var(--chart-2)', 3: 'var(--chart-3)',
                            4: 'var(--chart-4)', 5: 'var(--chart-5)',
                        },
                        sidebar: {
                            DEFAULT: 'var(--sidebar)',
                            foreground: 'var(--sidebar-foreground)',
                            primary: 'var(--sidebar-primary)',
                            'primary-foreground': 'var(--sidebar-primary-foreground)',
                            accent: 'var(--sidebar-accent)',
                            'accent-foreground': 'var(--sidebar-accent-foreground)',
                            border: 'var(--sidebar-border)',
                            ring: 'var(--sidebar-ring)',
                        },
                    },
                    borderRadius: {
                        lg: 'var(--radius)',
                        md: 'calc(var(--radius) - 2px)',
                        sm: 'calc(var(--radius) - 4px)',
                    },
                },
            },
        }
    </script>

    <!-- Prism.js for syntax highlighting -->
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/prismjs@1.29.0/themes/prism-tomorrow.min.css" />

    <!-- Dark mode init (prevent flash) -->
    <script>
        if (localStorage.getItem('theme') === 'dark' ||
            (!localStorage.getItem('theme') && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
            document.documentElement.classList.add('dark');
        }
    </script>
</head>
<body class="min-h-screen bg-background font-sans antialiased">
    <div id="app">
        <div class="flex min-h-screen items-center justify-center">
            <svg class="size-8 animate-spin text-muted-foreground" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
            </svg>
        </div>
    </div>

    <script src="_framework/blazor.webassembly.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/prismjs@1.29.0/prism.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/prismjs@1.29.0/components/prism-markup.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/prismjs@1.29.0/components/prism-csharp.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/prismjs@1.29.0/components/prism-cshtml.min.js"></script>
</body>
</html>
```

**Step 4: Create _Imports.razor**

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.AspNetCore.Components.WebAssembly.Http
@using Microsoft.JSInterop
@using BlazorCN
@using BlazorCN.Demo
@using BlazorCN.Demo.Layout
@using BlazorCN.Demo.Components
```

**Step 5: Create App.razor**

Create `docs/BlazorCN.Demo/App.razor`:

```razor
<Router AppAssembly="typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(MainLayout)" />
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
    <NotFound>
        <PageTitle>Not Found</PageTitle>
        <LayoutView Layout="typeof(MainLayout)">
            <div class="flex min-h-[50vh] flex-col items-center justify-center gap-4">
                <h1 class="text-4xl font-bold">404</h1>
                <p class="text-muted-foreground">Page not found.</p>
                <ButtonCn Href="/">Go Home</ButtonCn>
            </div>
        </LayoutView>
    </NotFound>
</Router>
```

**Step 6: Update solution file**

Modify `BlazorCN.slnx` to add the demo project:

```xml
<Solution>
  <Folder Name="/src/">
    <Project Path="src/BlazorCN/BlazorCN.csproj" />
  </Folder>
  <Folder Name="/tests/">
    <Project Path="tests/BlazorCN.Tests/BlazorCN.Tests.csproj" />
  </Folder>
  <Folder Name="/docs/">
    <Project Path="docs/BlazorCN.Demo/BlazorCN.Demo.csproj" />
  </Folder>
</Solution>
```

**Step 7: Verify it builds**

Run: `dotnet build docs/BlazorCN.Demo/BlazorCN.Demo.csproj`
Expected: BUILD SUCCEEDED, 0 errors

**Step 8: Commit**

```bash
git add docs/BlazorCN.Demo/ BlazorCN.slnx
git commit -m "feat: scaffold BlazorCN.Demo WASM project"
```

---

### Task 2: Create MainLayout with SiteHeader and SiteFooter

**Files:**
- Create: `docs/BlazorCN.Demo/Layout/MainLayout.razor`
- Create: `docs/BlazorCN.Demo/Layout/SiteHeader.razor`
- Create: `docs/BlazorCN.Demo/Layout/SiteFooter.razor`
- Create: `docs/BlazorCN.Demo/wwwroot/demo.js` (theme toggle + clipboard + Prism highlight)

**Step 1: Create demo.js**

```javascript
export function toggleTheme() {
    const html = document.documentElement;
    const isDark = html.classList.toggle('dark');
    localStorage.setItem('theme', isDark ? 'dark' : 'light');
    return isDark;
}

export function getTheme() {
    return document.documentElement.classList.contains('dark') ? 'dark' : 'light';
}

export function copyToClipboard(text) {
    return navigator.clipboard.writeText(text);
}

export function highlightAll() {
    if (window.Prism) {
        Prism.highlightAll();
    }
}

export function setThemeColor(name, value) {
    document.documentElement.style.setProperty(name, value);
}

export function removeThemeColor(name) {
    document.documentElement.style.removeProperty(name);
}
```

**Step 2: Create SiteHeader.razor**

```razor
@inject NavigationManager Nav
@inject IJSRuntime JS

<header class="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
    <div class="container flex h-14 items-center px-4 md:px-8">
        <a href="/" class="mr-6 flex items-center space-x-2 font-bold">
            BlazorCN
        </a>

        <!-- Desktop nav -->
        <nav class="hidden md:flex items-center gap-6 text-sm">
            <a href="/docs/getting-started" class="@NavLinkClass("/docs")">Docs</a>
            <a href="/docs/components/button" class="@NavLinkClass("/docs/components")">Components</a>
            <a href="/examples/dashboard" class="@NavLinkClass("/examples")">Examples</a>
            <a href="/themes" class="@NavLinkClass("/themes")">Themes</a>
            <a href="/colors" class="@NavLinkClass("/colors")">Colors</a>
        </nav>

        <div class="flex flex-1 items-center justify-end gap-2">
            <!-- Search trigger -->
            <ButtonCn Variant="ButtonVariant.Outline" Size="ButtonSize.Sm"
                      Class="hidden md:inline-flex w-64 justify-start text-muted-foreground"
                      OnClick="@(() => OnSearchRequested.InvokeAsync())">
                <span>Search components...</span>
                <KbdCn Class="ml-auto">Ctrl+K</KbdCn>
            </ButtonCn>

            <!-- Theme toggle -->
            <ButtonCn Variant="ButtonVariant.Ghost" Size="ButtonSize.Icon" OnClick="ToggleTheme"
                      aria-label="Toggle theme">
                @if (_isDark)
                {
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="4"/><path d="M12 2v2"/><path d="M12 20v2"/><path d="m4.93 4.93 1.41 1.41"/><path d="m17.66 17.66 1.41 1.41"/><path d="M2 12h2"/><path d="M20 12h2"/><path d="m6.34 17.66-1.41 1.41"/><path d="m19.07 4.93-1.41 1.41"/></svg>
                }
                else
                {
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 3a6 6 0 0 0 9 9 9 9 0 1 1-9-9Z"/></svg>
                }
            </ButtonCn>

            <!-- GitHub -->
            <ButtonCn Variant="ButtonVariant.Ghost" Size="ButtonSize.Icon"
                      Href="https://github.com/nickvdyck/blazorcn" aria-label="GitHub">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="currentColor"><path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"/></svg>
            </ButtonCn>

            <!-- Mobile menu toggle -->
            <ButtonCn Variant="ButtonVariant.Ghost" Size="ButtonSize.Icon"
                      Class="md:hidden" OnClick="@(() => OnMobileMenuRequested.InvokeAsync())"
                      aria-label="Menu">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="4" x2="20" y1="12" y2="12"/><line x1="4" x2="20" y1="6" y2="6"/><line x1="4" x2="20" y1="18" y2="18"/></svg>
            </ButtonCn>
        </div>
    </div>
</header>

@code {
    [Parameter] public EventCallback OnSearchRequested { get; set; }
    [Parameter] public EventCallback OnMobileMenuRequested { get; set; }

    private IJSObjectReference? _module;
    private bool _isDark;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>("import", "./demo.js");
            var theme = await _module.InvokeAsync<string>("getTheme");
            _isDark = theme == "dark";
            StateHasChanged();
        }
    }

    private async Task ToggleTheme()
    {
        if (_module is not null)
        {
            _isDark = await _module.InvokeAsync<bool>("toggleTheme");
        }
    }

    private string NavLinkClass(string prefix)
    {
        var uri = Nav.ToBaseRelativePath(Nav.Uri);
        var isActive = uri.StartsWith(prefix.TrimStart('/'), StringComparison.OrdinalIgnoreCase);
        return isActive
            ? "text-foreground font-medium transition-colors"
            : "text-muted-foreground transition-colors hover:text-foreground";
    }
}
```

**Step 3: Create SiteFooter.razor**

```razor
<footer class="border-t py-6 md:py-0">
    <div class="container flex flex-col items-center justify-between gap-4 px-4 md:h-16 md:flex-row md:px-8">
        <p class="text-sm text-muted-foreground">
            Built with <a href="https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor" class="font-medium underline underline-offset-4" target="_blank">Blazor</a>
            and <a href="https://tailwindcss.com" class="font-medium underline underline-offset-4" target="_blank">Tailwind CSS</a>.
            Inspired by <a href="https://ui.shadcn.com" class="font-medium underline underline-offset-4" target="_blank">shadcn/ui</a>.
        </p>
    </div>
</footer>
```

**Step 4: Create MainLayout.razor**

```razor
@inherits LayoutComponentBase

<div class="relative flex min-h-screen flex-col">
    <SiteHeader OnSearchRequested="OpenSearch" OnMobileMenuRequested="OpenMobileMenu" />
    <main class="flex-1">
        @Body
    </main>
    <SiteFooter />
</div>

@* Mobile nav sheet *@
<SheetCn @bind-Open="_mobileMenuOpen">
    <SheetContentCn Side="SheetSide.Left" Class="w-72 p-0">
        <SheetHeaderCn Class="border-b p-4">
            <SheetTitleCn>BlazorCN</SheetTitleCn>
        </SheetHeaderCn>
        <ScrollAreaCn Class="h-[calc(100vh-5rem)]">
            <div class="flex flex-col gap-1 p-4">
                <a href="/docs/getting-started" class="block rounded-md px-3 py-2 text-sm hover:bg-accent">Getting Started</a>
                <a href="/docs/components/button" class="block rounded-md px-3 py-2 text-sm hover:bg-accent">Components</a>
                <a href="/examples/dashboard" class="block rounded-md px-3 py-2 text-sm hover:bg-accent">Examples</a>
                <a href="/themes" class="block rounded-md px-3 py-2 text-sm hover:bg-accent">Themes</a>
                <a href="/colors" class="block rounded-md px-3 py-2 text-sm hover:bg-accent">Colors</a>
            </div>
        </ScrollAreaCn>
    </SheetContentCn>
</SheetCn>

@* Command menu (Ctrl+K search) — placeholder for Task 4 *@

@code {
    private bool _mobileMenuOpen;
    private bool _searchOpen;

    private void OpenMobileMenu() => _mobileMenuOpen = true;
    private void OpenSearch() => _searchOpen = true;
}
```

**Step 5: Verify it builds**

Run: `dotnet build docs/BlazorCN.Demo/BlazorCN.Demo.csproj`
Expected: BUILD SUCCEEDED

**Step 6: Commit**

```bash
git add docs/BlazorCN.Demo/
git commit -m "feat: add MainLayout with SiteHeader, SiteFooter, mobile nav"
```

---

### Task 3: Create DocsLayout with sidebar navigation

**Files:**
- Create: `docs/BlazorCN.Demo/Layout/DocsLayout.razor`
- Create: `docs/BlazorCN.Demo/Layout/DocsSidebar.razor`
- Create: `docs/BlazorCN.Demo/Data/NavData.cs`

**Step 1: Create NavData.cs**

This provides the sidebar navigation structure — a single source of truth for all component names and routes.

```csharp
namespace BlazorCN.Demo.Data;

public static class NavData
{
    public static readonly NavSection[] GettingStarted =
    [
        new("Getting Started", [
            new("Introduction", "/docs/getting-started"),
            new("Installation", "/docs/installation"),
            new("Theming", "/docs/theming"),
            new("Dark Mode", "/docs/dark-mode"),
        ]),
    ];

    public static readonly NavSection[] Components =
    [
        new("Components", [
            new("Accordion", "/docs/components/accordion"),
            new("Alert", "/docs/components/alert"),
            new("Alert Dialog", "/docs/components/alert-dialog"),
            new("Aspect Ratio", "/docs/components/aspect-ratio"),
            new("Avatar", "/docs/components/avatar"),
            new("Badge", "/docs/components/badge"),
            new("Breadcrumb", "/docs/components/breadcrumb"),
            new("Button", "/docs/components/button"),
            new("Calendar", "/docs/components/calendar"),
            new("Card", "/docs/components/card"),
            new("Carousel", "/docs/components/carousel"),
            new("Chart", "/docs/components/chart"),
            new("Checkbox", "/docs/components/checkbox"),
            new("Collapsible", "/docs/components/collapsible"),
            new("Combobox", "/docs/components/combobox"),
            new("Command", "/docs/components/command"),
            new("Context Menu", "/docs/components/context-menu"),
            new("Dialog", "/docs/components/dialog"),
            new("Drawer", "/docs/components/drawer"),
            new("Dropdown Menu", "/docs/components/dropdown-menu"),
            new("Empty", "/docs/components/empty"),
            new("Form", "/docs/components/form"),
            new("Hover Card", "/docs/components/hover-card"),
            new("Input", "/docs/components/input"),
            new("Input OTP", "/docs/components/input-otp"),
            new("Kbd", "/docs/components/kbd"),
            new("Label", "/docs/components/label"),
            new("Menubar", "/docs/components/menubar"),
            new("Navigation Menu", "/docs/components/navigation-menu"),
            new("Pagination", "/docs/components/pagination"),
            new("Popover", "/docs/components/popover"),
            new("Progress", "/docs/components/progress"),
            new("Radio Group", "/docs/components/radio-group"),
            new("Resizable", "/docs/components/resizable"),
            new("Scroll Area", "/docs/components/scroll-area"),
            new("Select", "/docs/components/select"),
            new("Separator", "/docs/components/separator"),
            new("Sheet", "/docs/components/sheet"),
            new("Skeleton", "/docs/components/skeleton"),
            new("Slider", "/docs/components/slider"),
            new("Spinner", "/docs/components/spinner"),
            new("Switch", "/docs/components/switch"),
            new("Table", "/docs/components/table"),
            new("Tabs", "/docs/components/tabs"),
            new("Textarea", "/docs/components/textarea"),
            new("Toast", "/docs/components/toast"),
            new("Toggle", "/docs/components/toggle"),
            new("Toggle Group", "/docs/components/toggle-group"),
            new("Tooltip", "/docs/components/tooltip"),
        ]),
    ];
}

public record NavSection(string Title, NavItem[] Items);
public record NavItem(string Label, string Href);
```

**Step 2: Create DocsSidebar.razor**

```razor
@inject NavigationManager Nav

<aside class="hidden md:block w-64 shrink-0">
    <div class="sticky top-14 h-[calc(100vh-3.5rem)]">
        <ScrollAreaCn Class="h-full py-6 pr-4">
            @foreach (var section in Data.NavData.GettingStarted)
            {
                <div class="mb-4">
                    <h4 class="mb-1 rounded-md px-2 py-1 text-sm font-semibold">@section.Title</h4>
                    @foreach (var item in section.Items)
                    {
                        <a href="@item.Href"
                           class="@ItemClass(item.Href)">
                            @item.Label
                        </a>
                    }
                </div>
            }
            @foreach (var section in Data.NavData.Components)
            {
                <div class="mb-4">
                    <h4 class="mb-1 rounded-md px-2 py-1 text-sm font-semibold">@section.Title</h4>
                    @foreach (var item in section.Items)
                    {
                        <a href="@item.Href"
                           class="@ItemClass(item.Href)">
                            @item.Label
                        </a>
                    }
                </div>
            }
        </ScrollAreaCn>
    </div>
</aside>

@code {
    private string ItemClass(string href)
    {
        var uri = "/" + Nav.ToBaseRelativePath(Nav.Uri);
        var isActive = uri.Equals(href, StringComparison.OrdinalIgnoreCase);
        var baseClass = "block rounded-md px-2 py-1.5 text-sm transition-colors";
        return isActive
            ? $"{baseClass} bg-accent text-accent-foreground font-medium"
            : $"{baseClass} text-muted-foreground hover:text-foreground hover:bg-accent/50";
    }
}
```

**Step 3: Create DocsLayout.razor**

```razor
@inherits LayoutComponentBase
@layout MainLayout

<div class="container flex gap-8 px-4 md:px-8">
    <DocsSidebar />
    <div class="flex-1 min-w-0 py-6">
        @Body
    </div>
</div>
```

**Step 4: Verify it builds**

Run: `dotnet build docs/BlazorCN.Demo/BlazorCN.Demo.csproj`
Expected: BUILD SUCCEEDED

**Step 5: Commit**

```bash
git add docs/BlazorCN.Demo/
git commit -m "feat: add DocsLayout with sidebar navigation"
```

---

### Task 4: Create ComponentPreview and CodeBlock components

**Files:**
- Create: `docs/BlazorCN.Demo/Components/ComponentPreview.razor`
- Create: `docs/BlazorCN.Demo/Components/CodeBlock.razor`

**Step 1: Create CodeBlock.razor**

```razor
@inject IJSRuntime JS

<div class="relative">
    <pre class="overflow-x-auto rounded-lg border bg-zinc-950 p-4 dark:bg-zinc-900"><code class="language-@Language text-sm">@Code</code></pre>
    <ButtonCn Variant="ButtonVariant.Ghost" Size="ButtonSize.IconSm"
              Class="absolute top-2 right-2 text-zinc-400 hover:text-zinc-100"
              OnClick="CopyCode" aria-label="Copy code">
        @if (_copied)
        {
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6 9 17l-5-5"/></svg>
        }
        else
        {
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect width="14" height="14" x="8" y="8" rx="2" ry="2"/><path d="M4 16c-1.1 0-2-.9-2-2V4c0-1.1.9-2 2-2h10c1.1 0 2 .9 2 2"/></svg>
        }
    </ButtonCn>
</div>

@code {
    [Parameter, EditorRequired] public string Code { get; set; } = "";
    [Parameter] public string Language { get; set; } = "cshtml";

    private IJSObjectReference? _module;
    private bool _copied;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _module ??= await JS.InvokeAsync<IJSObjectReference>("import", "./demo.js");
        await _module.InvokeVoidAsync("highlightAll");
    }

    private async Task CopyCode()
    {
        if (_module is not null)
        {
            await _module.InvokeVoidAsync("copyToClipboard", Code);
            _copied = true;
            StateHasChanged();
            await Task.Delay(2000);
            _copied = false;
            StateHasChanged();
        }
    }
}
```

**Step 2: Create ComponentPreview.razor**

```razor
<div class="mb-8">
    @if (Title is not null)
    {
        <h3 class="mb-3 text-lg font-semibold">@Title</h3>
    }
    @if (Description is not null)
    {
        <p class="mb-3 text-sm text-muted-foreground">@Description</p>
    }

    <TabsCn DefaultValue="preview">
        <TabsListCn>
            <TabsTriggerCn Value="preview">Preview</TabsTriggerCn>
            <TabsTriggerCn Value="code">Code</TabsTriggerCn>
        </TabsListCn>
        <TabsContentCn Value="preview">
            <div class="flex min-h-[200px] items-center justify-center rounded-lg border p-8">
                @ChildContent
            </div>
        </TabsContentCn>
        <TabsContentCn Value="code">
            <CodeBlock Code="@Code" />
        </TabsContentCn>
    </TabsCn>
</div>

@code {
    [Parameter] public string? Title { get; set; }
    [Parameter] public string? Description { get; set; }
    [Parameter, EditorRequired] public string Code { get; set; } = "";
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

**Step 3: Verify it builds**

Run: `dotnet build docs/BlazorCN.Demo/BlazorCN.Demo.csproj`
Expected: BUILD SUCCEEDED

**Step 4: Commit**

```bash
git add docs/BlazorCN.Demo/Components/
git commit -m "feat: add ComponentPreview and CodeBlock components"
```

---

### Task 5: Create ExampleLayout

**Files:**
- Create: `docs/BlazorCN.Demo/Layout/ExampleLayout.razor`

**Step 1: Create ExampleLayout.razor**

```razor
@inherits LayoutComponentBase
@layout MainLayout

<div class="flex-1">
    @Body
</div>
```

**Step 2: Verify and commit**

Run: `dotnet build docs/BlazorCN.Demo/BlazorCN.Demo.csproj`

```bash
git add docs/BlazorCN.Demo/Layout/ExampleLayout.razor
git commit -m "feat: add ExampleLayout for full-page examples"
```

---

## Phase 2: Landing Page & Getting Started Docs

### Task 6: Create the landing page

**Files:**
- Create: `docs/BlazorCN.Demo/Pages/Index.razor`

**Step 1: Create Index.razor**

```razor
@page "/"

<PageTitle>BlazorCN — Blazor Components</PageTitle>

<div class="container px-4 md:px-8">
    <!-- Hero -->
    <section class="flex flex-col items-center gap-6 py-20 text-center md:py-32">
        <BadgeCn Variant="BadgeVariant.Secondary" Class="mb-2">Open Source</BadgeCn>
        <h1 class="max-w-3xl text-4xl font-bold tracking-tight sm:text-5xl md:text-6xl lg:text-7xl">
            Build your Blazor apps with
            <span class="text-primary">beautifully designed</span> components
        </h1>
        <p class="max-w-xl text-lg text-muted-foreground">
            Accessible and customizable components built with Tailwind CSS.
            Copy and paste into your Blazor apps. Open source. Free forever.
        </p>
        <div class="flex gap-3">
            <ButtonCn Href="/docs/getting-started" Size="ButtonSize.Lg">Get Started</ButtonCn>
            <ButtonCn Variant="ButtonVariant.Outline" Size="ButtonSize.Lg"
                      Href="https://github.com/nickvdyck/blazorcn">GitHub</ButtonCn>
        </div>
    </section>

    <!-- Feature highlights -->
    <section class="grid gap-6 pb-20 sm:grid-cols-2 lg:grid-cols-4">
        <CardCn>
            <CardHeaderCn>
                <CardTitleCn>48 Components</CardTitleCn>
            </CardHeaderCn>
            <CardContentCn>
                <p class="text-sm text-muted-foreground">
                    From buttons to data tables, dialogs to drawers. Every shadcn-ui component, ported to Blazor.
                </p>
            </CardContentCn>
        </CardCn>

        <CardCn>
            <CardHeaderCn>
                <CardTitleCn>Tailwind CSS</CardTitleCn>
            </CardHeaderCn>
            <CardContentCn>
                <p class="text-sm text-muted-foreground">
                    Styled with Tailwind utility classes. Customize everything with your existing Tailwind config.
                </p>
            </CardContentCn>
        </CardCn>

        <CardCn>
            <CardHeaderCn>
                <CardTitleCn>Dark Mode</CardTitleCn>
            </CardHeaderCn>
            <CardContentCn>
                <p class="text-sm text-muted-foreground">
                    Built-in dark mode support via CSS variables. Toggle with a single class.
                </p>
            </CardContentCn>
        </CardCn>

        <CardCn>
            <CardHeaderCn>
                <CardTitleCn>Accessible</CardTitleCn>
            </CardHeaderCn>
            <CardContentCn>
                <p class="text-sm text-muted-foreground">
                    WAI-ARIA compliant. Focus management, keyboard navigation, and screen reader support.
                </p>
            </CardContentCn>
        </CardCn>
    </section>
</div>
```

**Step 2: Verify it builds and run**

Run: `dotnet build docs/BlazorCN.Demo/BlazorCN.Demo.csproj`

**Step 3: Commit**

```bash
git add docs/BlazorCN.Demo/Pages/Index.razor
git commit -m "feat: add landing page with hero and feature cards"
```

---

### Task 7: Create Getting Started docs (4 pages)

**Files:**
- Create: `docs/BlazorCN.Demo/Pages/Docs/GettingStartedPage.razor`
- Create: `docs/BlazorCN.Demo/Pages/Docs/InstallationPage.razor`
- Create: `docs/BlazorCN.Demo/Pages/Docs/ThemingPage.razor`
- Create: `docs/BlazorCN.Demo/Pages/Docs/DarkModePage.razor`

**Step 1: Create GettingStartedPage.razor**

```razor
@page "/docs/getting-started"
@layout DocsLayout

<PageTitle>Getting Started — BlazorCN</PageTitle>

<div class="space-y-6">
    <div>
        <h1 class="text-3xl font-bold tracking-tight">Introduction</h1>
        <p class="mt-2 text-lg text-muted-foreground">
            Beautifully designed Blazor components built with Tailwind CSS.
        </p>
    </div>

    <SeparatorCn />

    <div class="space-y-4 text-sm leading-7">
        <p>
            BlazorCN is a collection of reusable Blazor components that replicate
            <a href="https://ui.shadcn.com" class="font-medium underline underline-offset-4" target="_blank">shadcn/ui</a>
            one-to-one. It ships as a NuGet package with ~200 components across 48 groups.
        </p>

        <h2 class="text-xl font-semibold tracking-tight pt-4">Features</h2>
        <ul class="list-disc pl-6 space-y-2">
            <li>48 component groups (~200 individual components)</li>
            <li>Tailwind CSS styling with CSS variables for theming</li>
            <li>Dark mode support via <code class="rounded bg-muted px-1.5 py-0.5 text-sm">.dark</code> class</li>
            <li>All Blazor render modes: Server, WASM, Auto, Static SSR</li>
            <li>WAI-ARIA accessible — focus management, keyboard navigation</li>
            <li>Minimal JavaScript interop — only for focus trap, scroll lock, and positioning</li>
        </ul>

        <h2 class="text-xl font-semibold tracking-tight pt-4">Philosophy</h2>
        <p>
            BlazorCN components are thin wrappers around semantic HTML styled with Tailwind CSS.
            They use CSS variables for theming and ship zero external CSS dependencies.
            The consumer configures Tailwind CSS in their project and uses the BlazorCN preset.
        </p>
    </div>
</div>
```

**Step 2: Create InstallationPage.razor**

```razor
@page "/docs/installation"
@layout DocsLayout

<PageTitle>Installation — BlazorCN</PageTitle>

<div class="space-y-6">
    <div>
        <h1 class="text-3xl font-bold tracking-tight">Installation</h1>
        <p class="mt-2 text-lg text-muted-foreground">
            How to install and configure BlazorCN in your Blazor project.
        </p>
    </div>

    <SeparatorCn />

    <div class="space-y-4 text-sm leading-7">
        <h2 class="text-xl font-semibold tracking-tight">1. Install the NuGet package</h2>
        <CodeBlock Language="bash" Code="dotnet add package BlazorCN" />

        <h2 class="text-xl font-semibold tracking-tight pt-4">2. Add the CSS</h2>
        <p>Reference the BlazorCN stylesheet in your <code class="rounded bg-muted px-1.5 py-0.5 text-sm">index.html</code> or <code class="rounded bg-muted px-1.5 py-0.5 text-sm">App.razor</code>:</p>
        <CodeBlock Language="markup" Code="@CssSnippet" />

        <h2 class="text-xl font-semibold tracking-tight pt-4">3. Configure Tailwind CSS</h2>
        <p>Use the BlazorCN Tailwind preset in your Tailwind config:</p>
        <CodeBlock Language="javascript" Code="@TailwindSnippet" />

        <h2 class="text-xl font-semibold tracking-tight pt-4">4. Add the namespace</h2>
        <p>Add the BlazorCN namespace to your <code class="rounded bg-muted px-1.5 py-0.5 text-sm">_Imports.razor</code>:</p>
        <CodeBlock Language="cshtml" Code="@("@using BlazorCN")" />

        <h2 class="text-xl font-semibold tracking-tight pt-4">5. Register services</h2>
        <p>Register the JS interop service in <code class="rounded bg-muted px-1.5 py-0.5 text-sm">Program.cs</code>:</p>
        <CodeBlock Language="csharp" Code="builder.Services.AddScoped<JsInteropCn>();" />
    </div>
</div>

@code {
    private const string CssSnippet = """<link rel="stylesheet" href="_content/BlazorCN/blazorcn.css" />""";
    private const string TailwindSnippet = """
import blazorcnPreset from './node_modules/blazorcn/wwwroot/tailwind-preset.js';

export default {
  presets: [blazorcnPreset],
  // your config...
}
""";
}
```

**Step 3: Create ThemingPage.razor**

```razor
@page "/docs/theming"
@layout DocsLayout

<PageTitle>Theming — BlazorCN</PageTitle>

<div class="space-y-6">
    <div>
        <h1 class="text-3xl font-bold tracking-tight">Theming</h1>
        <p class="mt-2 text-lg text-muted-foreground">
            Customize colors and styles using CSS variables.
        </p>
    </div>

    <SeparatorCn />

    <div class="space-y-4 text-sm leading-7">
        <p>
            BlazorCN uses CSS variables for theming. All color tokens are defined as
            <code class="rounded bg-muted px-1.5 py-0.5 text-sm">oklch()</code> values
            in <code class="rounded bg-muted px-1.5 py-0.5 text-sm">blazorcn.css</code>.
        </p>

        <h2 class="text-xl font-semibold tracking-tight pt-4">Color tokens</h2>
        <p>Override any of these CSS variables to customize your theme:</p>
        <CodeBlock Language="css" Code="@CssVarsSnippet" />

        <h2 class="text-xl font-semibold tracking-tight pt-4">Using the variables</h2>
        <p>
            The Tailwind preset maps these CSS variables to Tailwind color utilities.
            Use them like any other Tailwind color:
        </p>
        <CodeBlock Language="markup" Code="@UsageSnippet" />

        <AlertCn>
            <AlertTitleCn>Tip</AlertTitleCn>
            <AlertDescriptionCn>
                Visit the <a href="/themes" class="font-medium underline underline-offset-4">Themes</a> page
                to interactively customize your theme.
            </AlertDescriptionCn>
        </AlertCn>
    </div>
</div>

@code {
    private const string CssVarsSnippet = """
:root {
    --background: oklch(1 0 0);
    --foreground: oklch(0.145 0 0);
    --primary: oklch(0.205 0 0);
    --primary-foreground: oklch(0.985 0 0);
    --secondary: oklch(0.97 0 0);
    --secondary-foreground: oklch(0.205 0 0);
    --muted: oklch(0.97 0 0);
    --muted-foreground: oklch(0.556 0 0);
    --accent: oklch(0.97 0 0);
    --accent-foreground: oklch(0.205 0 0);
    --destructive: oklch(0.577 0.245 27.325);
    --border: oklch(0.922 0 0);
    --ring: oklch(0.708 0 0);
    /* ... and more */
}
""";
    private const string UsageSnippet = """
<div class="bg-primary text-primary-foreground">Primary</div>
<div class="bg-muted text-muted-foreground">Muted</div>
<div class="border-destructive text-destructive">Error</div>
""";
}
```

**Step 4: Create DarkModePage.razor**

```razor
@page "/docs/dark-mode"
@layout DocsLayout

<PageTitle>Dark Mode — BlazorCN</PageTitle>

<div class="space-y-6">
    <div>
        <h1 class="text-3xl font-bold tracking-tight">Dark Mode</h1>
        <p class="mt-2 text-lg text-muted-foreground">
            Adding dark mode to your BlazorCN application.
        </p>
    </div>

    <SeparatorCn />

    <div class="space-y-4 text-sm leading-7">
        <p>
            BlazorCN supports dark mode via the <code class="rounded bg-muted px-1.5 py-0.5 text-sm">.dark</code> class
            on the <code class="rounded bg-muted px-1.5 py-0.5 text-sm">&lt;html&gt;</code> element.
            All components automatically adapt their colors.
        </p>

        <h2 class="text-xl font-semibold tracking-tight pt-4">Setup</h2>
        <p>1. Configure Tailwind to use class-based dark mode:</p>
        <CodeBlock Language="javascript" Code="@TailwindDarkSnippet" />

        <p>2. Toggle the <code class="rounded bg-muted px-1.5 py-0.5 text-sm">.dark</code> class via JavaScript:</p>
        <CodeBlock Language="javascript" Code="@ToggleSnippet" />

        <h2 class="text-xl font-semibold tracking-tight pt-4">Respecting system preference</h2>
        <p>Add this script to your <code class="rounded bg-muted px-1.5 py-0.5 text-sm">&lt;head&gt;</code> to prevent a flash of wrong theme:</p>
        <CodeBlock Language="markup" Code="@SystemPrefSnippet" />
    </div>
</div>

@code {
    private const string TailwindDarkSnippet = """
export default {
  darkMode: 'class',
  // ...
}
""";
    private const string ToggleSnippet = """
document.documentElement.classList.toggle('dark');
""";
    private const string SystemPrefSnippet = """
<script>
  if (localStorage.getItem('theme') === 'dark' ||
      (!localStorage.getItem('theme') &&
       window.matchMedia('(prefers-color-scheme: dark)').matches)) {
    document.documentElement.classList.add('dark');
  }
</script>
""";
}
```

**Step 5: Verify and commit**

Run: `dotnet build docs/BlazorCN.Demo/BlazorCN.Demo.csproj`

```bash
git add docs/BlazorCN.Demo/Pages/Docs/
git commit -m "feat: add Getting Started documentation pages"
```

---

## Phase 3: Component Documentation Pages

### Task 8: Create first 5 component doc pages (Button, Badge, Card, Input, Label)

These establish the pattern. Each page uses ComponentPreview with live examples and code strings.

**Files:**
- Create: `docs/BlazorCN.Demo/Pages/Docs/Components/ButtonPage.razor`
- Create: `docs/BlazorCN.Demo/Pages/Docs/Components/BadgePage.razor`
- Create: `docs/BlazorCN.Demo/Pages/Docs/Components/CardPage.razor`
- Create: `docs/BlazorCN.Demo/Pages/Docs/Components/InputPage.razor`
- Create: `docs/BlazorCN.Demo/Pages/Docs/Components/LabelPage.razor`

**Step 1: Create ButtonPage.razor** (template for all doc pages)

```razor
@page "/docs/components/button"
@layout DocsLayout

<PageTitle>Button — BlazorCN</PageTitle>

<div class="space-y-6">
    <div>
        <h1 class="text-3xl font-bold tracking-tight">Button</h1>
        <p class="mt-2 text-lg text-muted-foreground">Displays a button or a component that looks like a button.</p>
    </div>

    <SeparatorCn />

    <ComponentPreview Title="Default" Code="@DefaultCode">
        <ButtonCn>Button</ButtonCn>
    </ComponentPreview>

    <ComponentPreview Title="Variants" Code="@VariantsCode">
        <div class="flex flex-wrap gap-3">
            <ButtonCn Variant="ButtonVariant.Default">Default</ButtonCn>
            <ButtonCn Variant="ButtonVariant.Secondary">Secondary</ButtonCn>
            <ButtonCn Variant="ButtonVariant.Destructive">Destructive</ButtonCn>
            <ButtonCn Variant="ButtonVariant.Outline">Outline</ButtonCn>
            <ButtonCn Variant="ButtonVariant.Ghost">Ghost</ButtonCn>
            <ButtonCn Variant="ButtonVariant.Link">Link</ButtonCn>
        </div>
    </ComponentPreview>

    <ComponentPreview Title="Sizes" Code="@SizesCode">
        <div class="flex flex-wrap items-center gap-3">
            <ButtonCn Size="ButtonSize.Xs">Extra Small</ButtonCn>
            <ButtonCn Size="ButtonSize.Sm">Small</ButtonCn>
            <ButtonCn Size="ButtonSize.Default">Default</ButtonCn>
            <ButtonCn Size="ButtonSize.Lg">Large</ButtonCn>
        </div>
    </ComponentPreview>

    <ComponentPreview Title="Icon" Code="@IconCode">
        <div class="flex flex-wrap items-center gap-3">
            <ButtonCn Size="ButtonSize.Icon" aria-label="Settings">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12.22 2h-.44a2 2 0 0 0-2 2v.18a2 2 0 0 1-1 1.73l-.43.25a2 2 0 0 1-2 0l-.15-.08a2 2 0 0 0-2.73.73l-.22.38a2 2 0 0 0 .73 2.73l.15.1a2 2 0 0 1 1 1.72v.51a2 2 0 0 1-1 1.74l-.15.09a2 2 0 0 0-.73 2.73l.22.38a2 2 0 0 0 2.73.73l.15-.08a2 2 0 0 1 2 0l.43.25a2 2 0 0 1 1 1.73V20a2 2 0 0 0 2 2h.44a2 2 0 0 0 2-2v-.18a2 2 0 0 1 1-1.73l.43-.25a2 2 0 0 1 2 0l.15.08a2 2 0 0 0 2.73-.73l.22-.39a2 2 0 0 0-.73-2.73l-.15-.08a2 2 0 0 1-1-1.74v-.5a2 2 0 0 1 1-1.74l.15-.09a2 2 0 0 0 .73-2.73l-.22-.38a2 2 0 0 0-2.73-.73l-.15.08a2 2 0 0 1-2 0l-.43-.25a2 2 0 0 1-1-1.73V4a2 2 0 0 0-2-2z"/><circle cx="12" cy="12" r="3"/></svg>
            </ButtonCn>
        </div>
    </ComponentPreview>

    <ComponentPreview Title="As Link" Code="@LinkCode">
        <ButtonCn Href="https://example.com">Visit Example</ButtonCn>
    </ComponentPreview>

    <ComponentPreview Title="Disabled" Code="@DisabledCode">
        <ButtonCn Disabled="true">Disabled</ButtonCn>
    </ComponentPreview>
</div>

@code {
    private const string DefaultCode = """<ButtonCn>Button</ButtonCn>""";
    private const string VariantsCode = """
<ButtonCn Variant="ButtonVariant.Default">Default</ButtonCn>
<ButtonCn Variant="ButtonVariant.Secondary">Secondary</ButtonCn>
<ButtonCn Variant="ButtonVariant.Destructive">Destructive</ButtonCn>
<ButtonCn Variant="ButtonVariant.Outline">Outline</ButtonCn>
<ButtonCn Variant="ButtonVariant.Ghost">Ghost</ButtonCn>
<ButtonCn Variant="ButtonVariant.Link">Link</ButtonCn>
""";
    private const string SizesCode = """
<ButtonCn Size="ButtonSize.Xs">Extra Small</ButtonCn>
<ButtonCn Size="ButtonSize.Sm">Small</ButtonCn>
<ButtonCn Size="ButtonSize.Default">Default</ButtonCn>
<ButtonCn Size="ButtonSize.Lg">Large</ButtonCn>
""";
    private const string IconCode = """
<ButtonCn Size="ButtonSize.Icon" aria-label="Settings">
    <svg>...</svg>
</ButtonCn>
""";
    private const string LinkCode = """<ButtonCn Href="https://example.com">Visit Example</ButtonCn>""";
    private const string DisabledCode = """<ButtonCn Disabled="true">Disabled</ButtonCn>""";
}
```

**Step 2: Create BadgePage.razor, CardPage.razor, InputPage.razor, LabelPage.razor**

Follow the same pattern as ButtonPage — each page shows the component in various configurations with live previews + code strings. Keep examples focused on the key variants/features of each component.

**Step 3: Verify and commit**

Run: `dotnet build docs/BlazorCN.Demo/BlazorCN.Demo.csproj`

```bash
git add docs/BlazorCN.Demo/Pages/Docs/Components/
git commit -m "feat: add first 5 component doc pages (Button, Badge, Card, Input, Label)"
```

---

### Task 9: Create remaining 43 component doc pages

**Files:**
- Create: `docs/BlazorCN.Demo/Pages/Docs/Components/{ComponentName}Page.razor` — one per remaining component group

Follow the established pattern from Task 8. For each component:

1. `@page "/docs/components/{kebab-name}"`
2. `@layout DocsLayout`
3. PageTitle
4. H1 + description
5. Separator
6. One or more ComponentPreview sections demonstrating key features
7. Code strings as `const string` fields

**Complete list of remaining pages (43):**

AccordionPage, AlertPage, AlertDialogPage, AspectRatioPage, AvatarPage, BreadcrumbPage, CalendarPage, CarouselPage, ChartPage, CheckboxPage, CollapsiblePage, ComboboxPage, CommandPage, ContextMenuPage, DialogPage, DrawerPage, DropdownMenuPage, EmptyPage, FormPage, HoverCardPage, InputOtpPage, KbdPage, MenubarPage, NavigationMenuPage, PaginationPage, PopoverPage, ProgressPage, RadioGroupPage, ResizablePage, ScrollAreaPage, SelectPage, SeparatorPage, SheetPage, SkeletonPage, SliderPage, SpinnerPage, SwitchPage, TablePage, TabsPage, TextareaPage, ToastPage, TogglePage, ToggleGroupPage, TooltipPage

**Key principle:** Each page should show 2-5 examples maximum. Don't over-document — show the default, key variants, and one composed example where applicable.

**This is the largest task — expect ~43 files. Each file is 40-120 lines following the same template.**

**Commit after every ~10 pages:**

```bash
git commit -m "feat: add component doc pages (Accordion through Collapsible)"
git commit -m "feat: add component doc pages (Combobox through Form)"
git commit -m "feat: add component doc pages (HoverCard through ScrollArea)"
git commit -m "feat: add component doc pages (Select through Tooltip)"
```

---

## Phase 4: Example Pages

### Task 10: Create mock data for examples

**Files:**
- Create: `docs/BlazorCN.Demo/Data/MockData.cs`

**Step 1: Create MockData.cs**

```csharp
namespace BlazorCN.Demo.Data;

public static class MockData
{
    // Dashboard data
    public static readonly DashboardStat[] DashboardStats =
    [
        new("Total Revenue", "$45,231.89", "+20.1% from last month"),
        new("Subscriptions", "+2,350", "+180.1% from last month"),
        new("Sales", "+12,234", "+19% from last month"),
        new("Active Now", "+573", "+201 since last hour"),
    ];

    public static readonly RecentSale[] RecentSales =
    [
        new("Olivia Martin", "olivia.martin@email.com", "+$1,999.00"),
        new("Jackson Lee", "jackson.lee@email.com", "+$39.00"),
        new("Isabella Nguyen", "isabella.nguyen@email.com", "+$299.00"),
        new("William Kim", "will@email.com", "+$99.00"),
        new("Sofia Davis", "sofia.davis@email.com", "+$39.00"),
    ];

    // Tasks data
    public static readonly TaskItem[] Tasks =
    [
        new("TASK-8782", "You can't compress the program without quantifying the open-source SSD pixel!", "In Progress", "High"),
        new("TASK-7878", "Try to calculate the EXE feed, maybe it will index the multi-byte pixel!", "Backlog", "Medium"),
        new("TASK-7839", "We need to bypass the neural TCP card!", "Todo", "High"),
        new("TASK-5562", "The SAS interface is down, bypass the open-source pixel!", "Backlog", "Medium"),
        new("TASK-8686", "I'll parse the wireless SSL protocol, that should driver the API panel!", "Canceled", "Low"),
        new("TASK-1280", "Use the digital TLS panel, then you can transmit the haptic system!", "Done", "High"),
        new("TASK-7262", "The UTF8 application is down, parse the neural bandwidth!", "Done", "High"),
        new("TASK-1138", "Generating the driver won't do anything, we need to quantify the 1080p SMTP bandwidth!", "In Progress", "Medium"),
        new("TASK-7184", "We need to program the back-end THX pixel!", "Todo", "Low"),
        new("TASK-5160", "Calculating the bus won't do anything, we need to navigate the back-end JSON protocol!", "In Progress", "High"),
        new("TASK-5618", "Generating the driver won't do anything, we need to index the online SSL application!", "Done", "Medium"),
        new("TASK-6699", "I'll transmit the wireless JBOD capacitor, that should hard drive the SSD feed!", "Backlog", "Medium"),
        new("TASK-2858", "We need to override the online UDP bus!", "Backlog", "Low"),
        new("TASK-9864", "I'll reboot the 1080p FTP panel, that should bandwidth the UTF8 bus!", "Todo", "High"),
        new("TASK-8722", "Use the virtual HDD interface, then you can parse the bluetooth alarm!", "In Progress", "Low"),
        new("TASK-3320", "Parsing the feed won't do anything, we need to copy the bluetooth DRAM circuit!", "Todo", "Medium"),
        new("TASK-9602", "Compressing the interface won't do anything, we need to compress the online SDD card!", "Done", "High"),
        new("TASK-4453", "Try to override the ASCII application, maybe it will index the multi-byte bandwidth!", "Canceled", "Medium"),
        new("TASK-3881", "We need to index the mobile PCI bus!", "In Progress", "Low"),
        new("TASK-3473", "The SQL firewall is down, input the digital port!", "Todo", "High"),
    ];
}

public record DashboardStat(string Title, string Value, string Change);
public record RecentSale(string Name, string Email, string Amount);
public record TaskItem(string Id, string Title, string Status, string Priority);
```

**Step 2: Verify and commit**

```bash
git add docs/BlazorCN.Demo/Data/MockData.cs
git commit -m "feat: add mock data for Dashboard and Tasks examples"
```

---

### Task 11: Create Dashboard example page

**Files:**
- Create: `docs/BlazorCN.Demo/Pages/Examples/DashboardPage.razor`

**Step 1: Create DashboardPage.razor**

Uses CardCn for stat cards, TableCn for recent sales, AvatarCn for user pics. No SidebarCn (doesn't exist), so the dashboard is the main content area with a top bar.

```razor
@page "/examples/dashboard"
@layout ExampleLayout

<PageTitle>Dashboard — BlazorCN</PageTitle>

<div class="flex flex-col">
    <!-- Top bar -->
    <div class="border-b">
        <div class="flex h-16 items-center px-4 md:px-8">
            <h2 class="text-lg font-semibold">Dashboard</h2>
            <div class="ml-auto flex items-center gap-4">
                <InputCn Type="search" placeholder="Search..." Class="w-48 md:w-64" />
                <DropdownMenuCn>
                    <DropdownMenuTriggerCn>
                        <ButtonCn Variant="ButtonVariant.Ghost" Size="ButtonSize.Icon" Class="rounded-full">
                            <AvatarCn Class="size-8">
                                <AvatarFallbackCn>CN</AvatarFallbackCn>
                            </AvatarCn>
                        </ButtonCn>
                    </DropdownMenuTriggerCn>
                    <DropdownMenuContentCn Align="FloatingAlign.End">
                        <DropdownMenuLabelCn>My Account</DropdownMenuLabelCn>
                        <DropdownMenuSeparatorCn />
                        <DropdownMenuItemCn>Profile</DropdownMenuItemCn>
                        <DropdownMenuItemCn>Settings</DropdownMenuItemCn>
                        <DropdownMenuSeparatorCn />
                        <DropdownMenuItemCn>Log out</DropdownMenuItemCn>
                    </DropdownMenuContentCn>
                </DropdownMenuCn>
            </div>
        </div>
    </div>

    <!-- Content -->
    <div class="flex-1 space-y-6 p-4 md:p-8">
        <div class="flex items-center justify-between">
            <h2 class="text-3xl font-bold tracking-tight">Dashboard</h2>
            <ButtonCn>Download</ButtonCn>
        </div>

        <!-- Stats -->
        <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            @foreach (var stat in Data.MockData.DashboardStats)
            {
                <CardCn>
                    <CardHeaderCn Class="flex flex-row items-center justify-between pb-2">
                        <CardTitleCn Class="text-sm font-medium">@stat.Title</CardTitleCn>
                    </CardHeaderCn>
                    <CardContentCn>
                        <div class="text-2xl font-bold">@stat.Value</div>
                        <p class="text-xs text-muted-foreground">@stat.Change</p>
                    </CardContentCn>
                </CardCn>
            }
        </div>

        <div class="grid gap-4 lg:grid-cols-7">
            <!-- Chart placeholder -->
            <CardCn Class="lg:col-span-4">
                <CardHeaderCn>
                    <CardTitleCn>Overview</CardTitleCn>
                </CardHeaderCn>
                <CardContentCn>
                    <div class="flex h-[300px] items-center justify-center rounded-md border border-dashed text-sm text-muted-foreground">
                        Chart placeholder
                    </div>
                </CardContentCn>
            </CardCn>

            <!-- Recent sales -->
            <CardCn Class="lg:col-span-3">
                <CardHeaderCn>
                    <CardTitleCn>Recent Sales</CardTitleCn>
                    <CardDescriptionCn>You made 265 sales this month.</CardDescriptionCn>
                </CardHeaderCn>
                <CardContentCn>
                    <div class="space-y-6">
                        @foreach (var sale in Data.MockData.RecentSales)
                        {
                            <div class="flex items-center">
                                <AvatarCn Class="size-9">
                                    <AvatarFallbackCn>@sale.Name[..2].ToUpper()</AvatarFallbackCn>
                                </AvatarCn>
                                <div class="ml-4 space-y-1">
                                    <p class="text-sm font-medium leading-none">@sale.Name</p>
                                    <p class="text-sm text-muted-foreground">@sale.Email</p>
                                </div>
                                <div class="ml-auto font-medium">@sale.Amount</div>
                            </div>
                        }
                    </div>
                </CardContentCn>
            </CardCn>
        </div>
    </div>
</div>
```

**Step 2: Verify and commit**

Run: `dotnet build docs/BlazorCN.Demo/BlazorCN.Demo.csproj`

```bash
git add docs/BlazorCN.Demo/Pages/Examples/DashboardPage.razor
git commit -m "feat: add Dashboard example page"
```

---

### Task 12: Create Tasks example page

**Files:**
- Create: `docs/BlazorCN.Demo/Pages/Examples/TasksPage.razor`

**Step 1: Create TasksPage.razor**

```razor
@page "/examples/tasks"
@layout ExampleLayout

<PageTitle>Tasks — BlazorCN</PageTitle>

<div class="flex-1 space-y-6 p-4 md:p-8">
    <div>
        <h2 class="text-3xl font-bold tracking-tight">Tasks</h2>
        <p class="text-muted-foreground">Here's a list of your tasks for this month!</p>
    </div>

    <!-- Toolbar -->
    <div class="flex items-center gap-2">
        <InputCn placeholder="Filter tasks..." Class="max-w-sm" @bind-Value="_filter" />
        <DropdownMenuCn>
            <DropdownMenuTriggerCn>
                <ButtonCn Variant="ButtonVariant.Outline" Size="ButtonSize.Sm">
                    Status
                    <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m6 9 6 6 6-6"/></svg>
                </ButtonCn>
            </DropdownMenuTriggerCn>
            <DropdownMenuContentCn>
                <DropdownMenuItemCn>Backlog</DropdownMenuItemCn>
                <DropdownMenuItemCn>Todo</DropdownMenuItemCn>
                <DropdownMenuItemCn>In Progress</DropdownMenuItemCn>
                <DropdownMenuItemCn>Done</DropdownMenuItemCn>
                <DropdownMenuItemCn>Canceled</DropdownMenuItemCn>
            </DropdownMenuContentCn>
        </DropdownMenuCn>
        <DropdownMenuCn>
            <DropdownMenuTriggerCn>
                <ButtonCn Variant="ButtonVariant.Outline" Size="ButtonSize.Sm">
                    Priority
                    <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m6 9 6 6 6-6"/></svg>
                </ButtonCn>
            </DropdownMenuTriggerCn>
            <DropdownMenuContentCn>
                <DropdownMenuItemCn>Low</DropdownMenuItemCn>
                <DropdownMenuItemCn>Medium</DropdownMenuItemCn>
                <DropdownMenuItemCn>High</DropdownMenuItemCn>
            </DropdownMenuContentCn>
        </DropdownMenuCn>
    </div>

    <!-- Table -->
    <div class="rounded-md border">
        <TableCn>
            <THeadCn>
                <TrCn>
                    <ThCn Class="w-24">Task</ThCn>
                    <ThCn>Title</ThCn>
                    <ThCn Class="w-28">Status</ThCn>
                    <ThCn Class="w-24">Priority</ThCn>
                </TrCn>
            </THeadCn>
            <TBodyCn>
                @foreach (var task in FilteredTasks)
                {
                    <TrCn>
                        <TdCn Class="font-medium">@task.Id</TdCn>
                        <TdCn Class="max-w-[500px] truncate">@task.Title</TdCn>
                        <TdCn>
                            <BadgeCn Variant="StatusVariant(task.Status)">@task.Status</BadgeCn>
                        </TdCn>
                        <TdCn>
                            <BadgeCn Variant="PriorityVariant(task.Priority)">@task.Priority</BadgeCn>
                        </TdCn>
                    </TrCn>
                }
            </TBodyCn>
        </TableCn>
    </div>

    <div class="flex items-center justify-between">
        <p class="text-sm text-muted-foreground">
            @FilteredTasks.Length of @Data.MockData.Tasks.Length task(s).
        </p>
    </div>
</div>

@code {
    private string? _filter;

    private Data.TaskItem[] FilteredTasks => string.IsNullOrWhiteSpace(_filter)
        ? Data.MockData.Tasks
        : Data.MockData.Tasks
            .Where(t => t.Title.Contains(_filter, StringComparison.OrdinalIgnoreCase)
                     || t.Id.Contains(_filter, StringComparison.OrdinalIgnoreCase))
            .ToArray();

    private static BadgeVariant StatusVariant(string status) => status switch
    {
        "Done" => BadgeVariant.Default,
        "In Progress" => BadgeVariant.Secondary,
        "Canceled" => BadgeVariant.Destructive,
        _ => BadgeVariant.Outline,
    };

    private static BadgeVariant PriorityVariant(string priority) => priority switch
    {
        "High" => BadgeVariant.Destructive,
        "Medium" => BadgeVariant.Secondary,
        _ => BadgeVariant.Outline,
    };
}
```

**Step 2: Verify and commit**

```bash
git add docs/BlazorCN.Demo/Pages/Examples/TasksPage.razor
git commit -m "feat: add Tasks example page"
```

---

### Task 13: Create Authentication example page

**Files:**
- Create: `docs/BlazorCN.Demo/Pages/Examples/AuthenticationPage.razor`

**Step 1: Create AuthenticationPage.razor**

```razor
@page "/examples/authentication"
@layout ExampleLayout

<PageTitle>Authentication — BlazorCN</PageTitle>

<div class="flex min-h-[calc(100vh-3.5rem)]">
    <!-- Left: branding panel -->
    <div class="hidden lg:flex lg:w-1/2 flex-col justify-between bg-zinc-900 p-10 text-white">
        <div class="flex items-center gap-2 text-lg font-medium">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 6v12a3 3 0 1 0 3-3H6a3 3 0 1 0 3 3V6a3 3 0 1 0-3 3h12a3 3 0 1 0-3-3"/></svg>
            Acme Inc
        </div>
        <blockquote class="space-y-2">
            <p class="text-lg">
                &ldquo;This library has saved me countless hours of work and
                helped me deliver stunning designs to my clients faster than ever before.&rdquo;
            </p>
            <footer class="text-sm text-zinc-400">Sofia Davis</footer>
        </blockquote>
    </div>

    <!-- Right: auth form -->
    <div class="flex flex-1 items-center justify-center p-8">
        <div class="w-full max-w-sm space-y-6">
            <div class="text-center space-y-2">
                <h1 class="text-2xl font-semibold tracking-tight">Create an account</h1>
                <p class="text-sm text-muted-foreground">Enter your email below to create your account</p>
            </div>

            <div class="space-y-4">
                <div class="space-y-2">
                    <LabelCn For="email">Email</LabelCn>
                    <InputCn id="email" Type="email" placeholder="name@example.com" />
                </div>
                <div class="space-y-2">
                    <LabelCn For="password">Password</LabelCn>
                    <InputCn id="password" Type="password" />
                </div>
                <ButtonCn Class="w-full">Create Account</ButtonCn>
            </div>

            <div class="relative">
                <div class="absolute inset-0 flex items-center">
                    <SeparatorCn />
                </div>
                <div class="relative flex justify-center text-xs uppercase">
                    <span class="bg-background px-2 text-muted-foreground">Or continue with</span>
                </div>
            </div>

            <div class="grid grid-cols-2 gap-4">
                <ButtonCn Variant="ButtonVariant.Outline">
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="currentColor"><path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"/></svg>
                    GitHub
                </ButtonCn>
                <ButtonCn Variant="ButtonVariant.Outline">
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24"><path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92a5.06 5.06 0 0 1-2.2 3.32v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"/><path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"/><path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05"/><path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"/></svg>
                    Google
                </ButtonCn>
            </div>

            <p class="text-center text-sm text-muted-foreground">
                By clicking continue, you agree to our
                <a href="#" class="underline underline-offset-4 hover:text-primary">Terms of Service</a>
                and
                <a href="#" class="underline underline-offset-4 hover:text-primary">Privacy Policy</a>.
            </p>
        </div>
    </div>
</div>
```

**Step 2: Verify and commit**

```bash
git add docs/BlazorCN.Demo/Pages/Examples/AuthenticationPage.razor
git commit -m "feat: add Authentication example page"
```

---

## Phase 5: Themes & Colors Pages

### Task 14: Create Themes page with ThemeCustomizer

**Files:**
- Create: `docs/BlazorCN.Demo/Pages/ThemesPage.razor`
- Create: `docs/BlazorCN.Demo/Components/ThemeCustomizer.razor`
- Create: `docs/BlazorCN.Demo/Components/CardsDemo.razor`

**Step 1: Create ThemeCustomizer.razor**

```razor
@inject IJSRuntime JS

<div class="space-y-6">
    <div>
        <h3 class="text-sm font-medium">Color</h3>
        <div class="mt-2 grid grid-cols-3 gap-2">
            @foreach (var color in _colors)
            {
                <ButtonCn Variant="@(color.Name == _activeColor ? ButtonVariant.Default : ButtonVariant.Outline)"
                          Size="ButtonSize.Sm"
                          OnClick="@(() => ApplyColor(color))">
                    <span class="mr-1 size-4 rounded-full" style="background: @color.Primary"></span>
                    @color.Name
                </ButtonCn>
            }
        </div>
    </div>

    <div>
        <h3 class="text-sm font-medium">Radius</h3>
        <div class="mt-2 grid grid-cols-5 gap-2">
            @foreach (var r in _radii)
            {
                <ButtonCn Variant="@(r == _activeRadius ? ButtonVariant.Default : ButtonVariant.Outline)"
                          Size="ButtonSize.Sm"
                          OnClick="@(() => ApplyRadius(r))">
                    @(r)rem
                </ButtonCn>
            }
        </div>
    </div>

    <div>
        <h3 class="text-sm font-medium">Mode</h3>
        <div class="mt-2 grid grid-cols-2 gap-2">
            <ButtonCn Variant="@(!_isDark ? ButtonVariant.Default : ButtonVariant.Outline)"
                      Size="ButtonSize.Sm" OnClick="@(() => SetMode(false))">Light</ButtonCn>
            <ButtonCn Variant="@(_isDark ? ButtonVariant.Default : ButtonVariant.Outline)"
                      Size="ButtonSize.Sm" OnClick="@(() => SetMode(true))">Dark</ButtonCn>
        </div>
    </div>
</div>

@code {
    private IJSObjectReference? _module;
    private string _activeColor = "Zinc";
    private string _activeRadius = "0.625";
    private bool _isDark;

    private static readonly double[] _radii = [0, 0.25, 0.5, 0.625, 0.75];

    private static readonly ThemeColor[] _colors =
    [
        new("Zinc", "oklch(0.205 0 0)", "oklch(0.985 0 0)"),
        new("Slate", "oklch(0.208 0.042 265.755)", "oklch(0.985 0.002 247.858)"),
        new("Stone", "oklch(0.216 0.006 56.043)", "oklch(0.985 0.001 106.424)"),
        new("Gray", "oklch(0.21 0.006 285.885)", "oklch(0.985 0.002 247.839)"),
        new("Neutral", "oklch(0.205 0 0)", "oklch(0.985 0 0)"),
    ];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>("import", "./demo.js");
            var theme = await _module.InvokeAsync<string>("getTheme");
            _isDark = theme == "dark";
            StateHasChanged();
        }
    }

    private async Task ApplyColor(ThemeColor color)
    {
        _activeColor = color.Name;
        if (_module is not null)
        {
            await _module.InvokeVoidAsync("setThemeColor", "--primary", color.Primary);
            await _module.InvokeVoidAsync("setThemeColor", "--primary-foreground", color.PrimaryForeground);
        }
    }

    private async Task ApplyRadius(double radius)
    {
        _activeRadius = radius.ToString();
        if (_module is not null)
        {
            await _module.InvokeVoidAsync("setThemeColor", "--radius", $"{radius}rem");
        }
    }

    private async Task SetMode(bool dark)
    {
        _isDark = dark;
        if (_module is not null)
        {
            await _module.InvokeAsync<bool>("toggleTheme");
            // Ensure correct state
            var actual = await _module.InvokeAsync<string>("getTheme");
            if ((actual == "dark") != dark)
                await _module.InvokeAsync<bool>("toggleTheme");
        }
    }

    private record ThemeColor(string Name, string Primary, string PrimaryForeground);
}
```

**Step 2: Create CardsDemo.razor**

A composite preview component showing various BlazorCN components to demonstrate the active theme. Includes cards with forms, buttons in all variants, badges, avatar, switch, etc.

```razor
<div class="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
    <!-- Card with form -->
    <CardCn Class="col-span-1">
        <CardHeaderCn>
            <CardTitleCn>Create project</CardTitleCn>
            <CardDescriptionCn>Deploy your new project in one-click.</CardDescriptionCn>
        </CardHeaderCn>
        <CardContentCn>
            <div class="space-y-4">
                <div class="space-y-2">
                    <LabelCn For="name">Name</LabelCn>
                    <InputCn id="name" placeholder="Name of your project" />
                </div>
                <div class="space-y-2">
                    <LabelCn For="desc">Description</LabelCn>
                    <TextareaCn id="desc" placeholder="Describe your project" />
                </div>
            </div>
        </CardContentCn>
        <CardFooterCn Class="flex justify-between">
            <ButtonCn Variant="ButtonVariant.Outline">Cancel</ButtonCn>
            <ButtonCn>Deploy</ButtonCn>
        </CardFooterCn>
    </CardCn>

    <!-- Button variants card -->
    <CardCn>
        <CardHeaderCn>
            <CardTitleCn>Buttons</CardTitleCn>
        </CardHeaderCn>
        <CardContentCn>
            <div class="flex flex-wrap gap-2">
                <ButtonCn>Default</ButtonCn>
                <ButtonCn Variant="ButtonVariant.Secondary">Secondary</ButtonCn>
                <ButtonCn Variant="ButtonVariant.Destructive">Destructive</ButtonCn>
                <ButtonCn Variant="ButtonVariant.Outline">Outline</ButtonCn>
                <ButtonCn Variant="ButtonVariant.Ghost">Ghost</ButtonCn>
                <ButtonCn Variant="ButtonVariant.Link">Link</ButtonCn>
            </div>
        </CardContentCn>
    </CardCn>

    <!-- Badges & misc card -->
    <CardCn>
        <CardHeaderCn>
            <CardTitleCn>Components</CardTitleCn>
        </CardHeaderCn>
        <CardContentCn>
            <div class="space-y-4">
                <div class="flex flex-wrap gap-2">
                    <BadgeCn>Default</BadgeCn>
                    <BadgeCn Variant="BadgeVariant.Secondary">Secondary</BadgeCn>
                    <BadgeCn Variant="BadgeVariant.Destructive">Destructive</BadgeCn>
                    <BadgeCn Variant="BadgeVariant.Outline">Outline</BadgeCn>
                </div>
                <SeparatorCn />
                <div class="flex items-center gap-4">
                    <AvatarCn>
                        <AvatarFallbackCn>CN</AvatarFallbackCn>
                    </AvatarCn>
                    <div>
                        <p class="text-sm font-medium">BlazorCN</p>
                        <p class="text-xs text-muted-foreground">Blazor Components</p>
                    </div>
                </div>
                <div class="flex items-center gap-2">
                    <SwitchCn />
                    <LabelCn>Notifications</LabelCn>
                </div>
                <div>
                    <ProgressCn Value="66" />
                </div>
            </div>
        </CardContentCn>
    </CardCn>
</div>
```

**Step 3: Create ThemesPage.razor**

```razor
@page "/themes"

<PageTitle>Themes — BlazorCN</PageTitle>

<div class="container px-4 md:px-8 py-8">
    <div class="space-y-2 mb-8">
        <h1 class="text-3xl font-bold tracking-tight">Themes</h1>
        <p class="text-lg text-muted-foreground">
            Hand-picked themes that you can copy and paste into your apps.
        </p>
    </div>

    <div class="flex flex-col gap-8 lg:flex-row">
        <aside class="w-full lg:w-64 shrink-0">
            <ThemeCustomizer />
        </aside>
        <div class="flex-1 min-w-0">
            <CardsDemo />
        </div>
    </div>
</div>
```

**Step 4: Verify and commit**

Run: `dotnet build docs/BlazorCN.Demo/BlazorCN.Demo.csproj`

```bash
git add docs/BlazorCN.Demo/Pages/ThemesPage.razor docs/BlazorCN.Demo/Components/ThemeCustomizer.razor docs/BlazorCN.Demo/Components/CardsDemo.razor
git commit -m "feat: add Themes page with interactive customizer"
```

---

### Task 15: Create Colors page

**Files:**
- Create: `docs/BlazorCN.Demo/Pages/ColorsPage.razor`

**Step 1: Create ColorsPage.razor**

```razor
@page "/colors"

<PageTitle>Colors — BlazorCN</PageTitle>

<div class="container px-4 md:px-8 py-8">
    <div class="space-y-2 mb-8">
        <h1 class="text-3xl font-bold tracking-tight">Colors</h1>
        <p class="text-lg text-muted-foreground">
            The color tokens used by BlazorCN components via CSS variables.
        </p>
    </div>

    <div class="grid gap-4 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4">
        @foreach (var color in _colors)
        {
            <CardCn>
                <CardContentCn Class="p-4">
                    <div class="mb-3 h-16 rounded-md border" style="background: var(@color.Variable)"></div>
                    <div class="space-y-1">
                        <p class="text-sm font-medium">@color.Label</p>
                        <p class="text-xs font-mono text-muted-foreground">@color.Variable</p>
                    </div>
                </CardContentCn>
            </CardCn>
        }
    </div>
</div>

@code {
    private static readonly ColorToken[] _colors =
    [
        new("Background", "--background"),
        new("Foreground", "--foreground"),
        new("Card", "--card"),
        new("Card Foreground", "--card-foreground"),
        new("Popover", "--popover"),
        new("Popover Foreground", "--popover-foreground"),
        new("Primary", "--primary"),
        new("Primary Foreground", "--primary-foreground"),
        new("Secondary", "--secondary"),
        new("Secondary Foreground", "--secondary-foreground"),
        new("Muted", "--muted"),
        new("Muted Foreground", "--muted-foreground"),
        new("Accent", "--accent"),
        new("Accent Foreground", "--accent-foreground"),
        new("Destructive", "--destructive"),
        new("Destructive Foreground", "--destructive-foreground"),
        new("Border", "--border"),
        new("Input", "--input"),
        new("Ring", "--ring"),
        new("Chart 1", "--chart-1"),
        new("Chart 2", "--chart-2"),
        new("Chart 3", "--chart-3"),
        new("Chart 4", "--chart-4"),
        new("Chart 5", "--chart-5"),
        new("Sidebar", "--sidebar"),
        new("Sidebar Foreground", "--sidebar-foreground"),
        new("Sidebar Primary", "--sidebar-primary"),
        new("Sidebar Accent", "--sidebar-accent"),
        new("Sidebar Border", "--sidebar-border"),
    ];

    private record ColorToken(string Label, string Variable);
}
```

**Step 2: Verify and commit**

```bash
git add docs/BlazorCN.Demo/Pages/ColorsPage.razor
git commit -m "feat: add Colors page with CSS variable palette"
```

---

## Phase 6: Search & Polish

### Task 16: Create CommandMenu (Ctrl+K search)

**Files:**
- Create: `docs/BlazorCN.Demo/Components/CommandMenu.razor`
- Modify: `docs/BlazorCN.Demo/Layout/MainLayout.razor` — wire up command menu

**Step 1: Create CommandMenu.razor**

```razor
@inject NavigationManager Nav

<DialogCn @bind-Open="@Open" OpenChanged="OpenChanged">
    <DialogContentCn Class="p-0 max-w-lg">
        <CommandCn>
            <CommandInputCn placeholder="Search components..." @bind-Value="_search" />
            <CommandListCn>
                <CommandEmptyCn>No results found.</CommandEmptyCn>
                @foreach (var section in Data.NavData.GettingStarted)
                {
                    <CommandGroupCn Heading="@section.Title">
                        @foreach (var item in FilterItems(section.Items))
                        {
                            <CommandItemCn OnSelect="@(() => Navigate(item.Href))">@item.Label</CommandItemCn>
                        }
                    </CommandGroupCn>
                }
                @foreach (var section in Data.NavData.Components)
                {
                    <CommandGroupCn Heading="@section.Title">
                        @foreach (var item in FilterItems(section.Items))
                        {
                            <CommandItemCn OnSelect="@(() => Navigate(item.Href))">@item.Label</CommandItemCn>
                        }
                    </CommandGroupCn>
                }
            </CommandListCn>
        </CommandCn>
    </DialogContentCn>
</DialogCn>

@code {
    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }

    private string? _search;

    private Data.NavItem[] FilterItems(Data.NavItem[] items)
    {
        if (string.IsNullOrWhiteSpace(_search)) return items;
        return items.Where(i => i.Label.Contains(_search, StringComparison.OrdinalIgnoreCase)).ToArray();
    }

    private async Task Navigate(string href)
    {
        Nav.NavigateTo(href);
        await OpenChanged.InvokeAsync(false);
    }
}
```

**Step 2: Wire into MainLayout.razor**

Add the CommandMenu to MainLayout and connect the keyboard shortcut:

Add after the mobile nav Sheet:
```razor
<CommandMenu @bind-Open="_searchOpen" />
```

Add keyboard listener in `OnAfterRenderAsync` to open on Ctrl+K (via JS interop or `@onkeydown` on the root div).

**Step 3: Verify and commit**

```bash
git add docs/BlazorCN.Demo/Components/CommandMenu.razor docs/BlazorCN.Demo/Layout/MainLayout.razor
git commit -m "feat: add Ctrl+K command menu search"
```

---

### Task 17: Final verification and cleanup

**Step 1: Full build**

Run: `dotnet build docs/BlazorCN.Demo/BlazorCN.Demo.csproj`
Expected: BUILD SUCCEEDED, 0 errors, 0 warnings

**Step 2: Run the app locally**

Run: `dotnet run --project docs/BlazorCN.Demo/BlazorCN.Demo.csproj`
Open browser and verify:
- Landing page loads
- Navigation works (all routes)
- Sidebar shows all 48 components
- At least one component page renders with preview + code
- Examples load (Dashboard, Tasks, Authentication)
- Themes page customizer works
- Colors page renders
- Dark mode toggle works
- Mobile nav (Sheet) works
- Command menu (Ctrl+K) opens

**Step 3: Run existing library tests (no regressions)**

Run: `dotnet test tests/BlazorCN.Tests/BlazorCN.Tests.csproj`
Expected: All 1018 tests pass

**Step 4: Final commit**

```bash
git add -A
git commit -m "feat: BlazorCN.Demo app — complete docs site with examples"
```

---

## Summary

| Phase | Tasks | Files Created | Description |
|-------|-------|--------------|-------------|
| 1 | 1-5 | ~12 | Project scaffolding, layouts, ComponentPreview, CodeBlock |
| 2 | 6-7 | ~5 | Landing page, Getting Started docs |
| 3 | 8-9 | ~48 | All 48 component documentation pages |
| 4 | 10-13 | ~4 | Mock data, Dashboard, Tasks, Authentication examples |
| 5 | 14-15 | ~4 | Themes page with customizer, Colors page |
| 6 | 16-17 | ~1 | Command menu search, polish, verification |

**Total: ~74 new files, 17 tasks, 0 files modified in BlazorCN library.**
